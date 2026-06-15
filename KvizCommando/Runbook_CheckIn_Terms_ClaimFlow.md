# ops/Runbook_CheckIn_Terms_ClaimFlow.md — Part 1/6

## Purpose
Uniform, claim-based Terms-of-Service (ToS) enforcement for **both** cookie (WASM) and opaque bearer (desktop/mobile) clients with **GDPR-minimal** data handling. No JWT, no custom token tinkering.

---

## Components (where to find things)
- Audit entity (append-only): `src/Server/Domain/Entities/Compliance/TermsConsent.cs`
- EF configuration: `src/Server/Infrastructure/Persistence/Configurations/Compliance/TermsConsentConfiguration.cs`
- Claim constants: `src/Server/Infrastructure/Auth/CustomClaimTypes.cs`
- Claim sync via UserManager:  
  `src/Server/Services/Auth/IClaimsSyncService.cs`  
  `src/Server/Services/Auth/ClaimsSyncService.cs`
- Terms provider (file + short memory cache + file watch): `src/Server/Services/TermsProvider.cs` (implements `ITermsProvider`)
- Business service (check-in logic): `src/Server/Services/CheckIn/CheckInService.cs`
- Controller (no Terms policy here): `src/Server/Controllers/CheckInController.cs`
- Dynamic policy (apply only to secure/business APIs):  
  `src/Server/Authorization/TermsAcceptedRequirement.cs`  
  `src/Server/Authorization/TermsAcceptedHandler.cs`
- Contracts:  
  `src/Shared/Contracts/CheckIn/TermsMeta.cs`  
  `src/Shared/Contracts/CheckIn/CheckInGetResponse.cs`  
  `src/Shared/Contracts/CheckIn/CheckInPostResponse.cs`

---

## Data Model (audit)
Table: `TermsConsents`  
Columns:
- `Id` (PK, identity)
- `UserId` (FK → AspNetUsers.Id)
- `TermsVersion` (ETag/version, e.g., `2025-09-01-ETAG`)
- `AcceptedAtUtc` (UTC)
- `UserAgentHash` (nullable BLOB 32 bytes)
- `IpHash` (nullable BLOB 32 bytes)

Indexes & constraints:
- Unique: `(UserId, TermsVersion)`
- Index: `(UserId, AcceptedAtUtc)` (descending time optional if provider supports)
- Optional CHECKs for 32-byte hashes (provider-specific)


# ops/Runbook_CheckIn_Terms_ClaimFlow.md — Part 2/6

## Terms Provider
Source: `wwwroot/legal/{culture}/manifest.json`  
Returns: `TermsMeta { Version (ETag), Url, PublishedAt (UTC) }`  
Caching: short TTL (~45s) + file change token watch  
Interface add-on: `ITermsProvider.CurrentTermsEtag` → `GetCurrentTerms().Version`

## Claims (GDPR-minimal)
- `terms.accepted.etag` → current accepted ETag (non-PII)
- `terms.accepted.at` → UTC timestamp in ISO-8601 `"O"` format (non-PII)
- No IP/UA in claims; those remain hashed in audit.

Sync flow: `IClaimsSyncService.UpsertTermsClaimsAsync(user, etag, acceptedAtUtc)`
- Upsert via `UserManager.AddClaimAsync` / `ReplaceClaimAsync`
- Cookie/WASM → `SignInManager.RefreshSignInAsync(user)`
- Opaque bearer → client must call `/auth/refresh` (response flag tells it to)

## Authorization Policy (dynamic, not hardcoded)
Name: `"RequireCurrentTerms"` (from `TermsAcceptedRequirement.PolicyName`)  
Behavior: compare principal’s `terms.accepted.etag` with `ITermsProvider.CurrentTermsEtag`.
- If equal → Succeed
- If missing → optional DB fallback (last `TermsConsent`) when `Auth:TermsPolicy:EnableDbFallback=true`
- If different → Fail (forces refresh/re-check-in)

Apply to: only secure/business APIs  
Do NOT apply to: `/api/checkin` (GET/POST), `/auth/login`, `/auth/refresh`, `/health`, `/swagger`


# ops/Runbook_CheckIn_Terms_ClaimFlow.md — Part 3/6

## Endpoints & Contracts

GET `/api/checkin`
- Returns `CheckInGetResponse`:
  - `Success`
  - `NeedsDisplayName`
  - `NeedsTermsAcceptance`
  - `CurrentTerms: TermsMeta { Version, Url, PublishedAt (UTC) }`

POST `/api/checkin`
- Request: `CheckInPostRequest` (may include `DisplayName` and `AcceptedTermsVersion` ETag)
- Server steps:
  1) Validate display name (format + uniqueness on normalized value) when needed  
  2) Validate Terms version vs `ITermsProvider.CurrentTermsEtag`  
  3) Insert audit row (`TermsConsents`) – append-only; unique `(UserId, TermsVersion)`  
  4) Upsert user claims (`terms.accepted.etag`, `terms.accepted.at`) via `IClaimsSyncService`  
  5) Cookie/WASM: `RefreshSignInAsync`; Opaque bearer: set `RequiresTokenRefresh=true` in response
- Response: `CheckInPostResponse` (always 200 OK; client keys/localizes errors)

{
  "success": true,
  "errors": [],
  "currentTerms": {
    "version": "2025-09-01-ETAG",
    "url": "/legal/hu-HU/terms.html",
    "publishedAt": "2025-09-01T00:00:00Z"
  },
  "requiresTokenRefresh": false
}

# ops/Runbook_CheckIn_Terms_ClaimFlow.md — Part 4/6

## Program.cs — Essentials
- Register handler: `builder.Services.AddScoped<IAuthorizationHandler, TermsAcceptedHandler>();`
- Add policy:  
  `options.AddPolicy("RequireCurrentTerms", p => p.RequireAuthenticatedUser().AddRequirements(new TermsAcceptedRequirement()));`
- Route grouping:
  - Whitelist (no Terms policy): `/api/checkin`, `/auth/login`, `/auth/refresh`, `/health`, `/swagger`
  - Secure group: apply `"RequireCurrentTerms"` to business controllers/endpoints
- Database resiliency:
  - Use `EnableRetryOnFailure` for SQL Server/Npgsql (opt-in via configuration)
  - Controller wraps `service.CompleteAsync(...)` with `db.Database.CreateExecutionStrategy()` (no external transaction)

## Request Flows (quick)

WASM (cookie)
1. Login (cookie)  
2. GET `/api/checkin` → `NeedsTermsAcceptance = true`  
3. POST `/api/checkin` → `success=true`, `requiresTokenRefresh=false` (cookie refreshed)  
4. Protected API (Terms policy) → pass

Desktop/Mobile (opaque bearer)
1. POST `/auth/login` → access + refresh (opaque)  
2. GET `/api/checkin` → `NeedsTermsAcceptance = true`  
3. POST `/api/checkin` → `success=true`, `requiresTokenRefresh=true`  
4. POST `/auth/refresh` → new access token (updated claims)  
5. Protected API → pass

New Terms published (ETag changed)
- Protected API with old token → 403 (Terms policy)  
  WASM UX: redirect to `/checkin`; Bearer: call `/auth/refresh`, then accept Terms if needed


# ops/Runbook_CheckIn_Terms_ClaimFlow.md — Part 5/6

## Troubleshooting

| Symptom | Likely Cause | Fix |
|---|---|---|
| 403 on `/api/checkin` or `/auth/refresh` | Terms policy applied there | Whitelist those routes; keep policy only on business endpoints |
| Bearer still 403 after acceptance | Client didn’t refresh access token | Honor `requiresTokenRefresh=true` and call `/auth/refresh` |
| WASM not updated after acceptance | `RefreshSignInAsync` failed/missing | Check server logs; verify cookie auth |
| Terms manifest not found | Missing `wwwroot/legal/{culture}/manifest.json` | Deploy manifest or rely on fallback |
| Duplicate audit row | Missing unique `(UserId, TermsVersion)` | Ensure unique index exists |
| High DB hits on policy | DB fallback enabled | Keep `Auth:TermsPolicy:EnableDbFallback=false` unless strictly needed |

## Ops Notes
- Single active session: New login invalidates previous sessions (SecurityStamp) → “one active login/user”
- GDPR-minimal: Only ETag + timestamp in claims; IP/UA stored HMACed in audit table; no PII in tokens/cookies
- Resiliency: EF execution strategy for transient errors; idempotent operations guarded by unique keys

## Configuration Toggles
- `Database:EnableRetryOnFailure` (SQL Server/Npgsql)
- `Auth:TermsPolicy:EnableDbFallback` (default false)
- `AuditHash:Secret` (Base64) for optional IP/UA HMAC; if missing, hashes stay null

# ops/Runbook_CheckIn_Terms_ClaimFlow.md — Part 6/6

## Pre-release Checklist
- [ ] `TermsConsents` schema + indexes present  
- [ ] `CustomClaimTypes` + `IClaimsSyncService` registered  
- [ ] `TermsProvider` returns correct ETag + UTC `PublishedAt` (cache + file watch OK)  
- [ ] Policy + handler registered; applied only to secure APIs; check-in/auth routes whitelisted  
- [ ] WASM path tested (no refresh needed)  
- [ ] Desktop/Mobile path tested (`requiresTokenRefresh` handled)  
- [ ] Logs show audit insert + claim upsert on acceptance

## Optional: XML/Export Strategy (deferred)
- No automatic XML generation.
- If needed later (admin-only, on demand):
  - Path: `ops/exports/terms/{yyyy-MM-dd}/terms-consents.xml|json`
  - Fields: `UserId`, `TermsVersion`, `AcceptedAtUtc`, optional hashed `Ip/UA`
  - Retention (e.g., 90 days); no automatic sending (manual pickup if requested)

