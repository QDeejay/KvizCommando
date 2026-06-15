# Runbook – tosExporter

This runbook describes how to use the **tosExporter** tool to generate hashed legal Terms of Service files and manifests.

## Purpose

- Read plain input HTML documents from a fixed input directory.
- Generate output files with content-based hashes in the filename.
- Update a `manifest.json` that points to the current version and file.
- Ensure cache invalidation and deterministic publishing.

## Input and Output locations

- **Input root:** `C:\Exports\TermsExporter\Input`
  - `hu-HU\tos.v<version>.html`
  - `en-US\tos.v<version>.html`

- **Output root:** `C:\Exports\TermsExporter\Outputs`
  - `hu-HU\tos.v<version>.<hash>.html`
  - `hu-HU\manifest.json`
  - `en-US\tos.v<version>.<hash>.html`
  - `en-US\manifest.json`

## File naming convention

tos.v<version>.<hash>.html

- <version>: semantic version string from the input filename (e.g. 1.1, 1.2).
- <hash>: SHA256 hash of the file content, first 10 hex characters (lowercase).

## Manifest structure

Each culture folder in the output contains a manifest.json with the following format:

{
  "tos": {
    "version": "1.1",
    "hash": "a1b2c3d4e5",
    "file": "tos.v1.1.a1b2c3d4e5.html"
  }
}

## Steps for usage

1. Place the updated legal document in the input directory:  
   - Example: C:\Exports\TermsExporter\Input\hu-HU\tos.v1.2.html  
   - Example: C:\Exports\TermsExporter\Input\en-US\tos.v1.2.html  

2. Build and run the tool:

   dotnet build tools\tosExporter -c Release  
   dotnet run --project tools\tosExporter -c Release  

3. The tool will:  
   - Compute the hash from the content.  
   - Write the new file into the output culture folder with the hashed name.  
   - Update manifest.json with the new version/hash/file entry.  
   - Leave previous hashed files untouched for archival purposes.  

4. Verify output:  
   - Check the file exists in C:\Exports\TermsExporter\Outputs\<culture>\  
   - Open manifest.json to confirm it points to the new file.  

## Example

Input:  
C:\Exports\TermsExporter\Input\hu-HU\tos.v1.2.html  
C:\Exports\TermsExporter\Input\en-US\tos.v1.2.html  

Output:  
C:\Exports\TermsExporter\Outputs\hu-HU\tos.v1.2.a1b2c3d4e5.html  
C:\Exports\TermsExporter\Outputs\hu-HU\manifest.json  
C:\Exports\TermsExporter\Outputs\en-US\tos.v1.2.f6e7d8c9ab.html  
C:\Exports\TermsExporter\Outputs\en-US\manifest.json  

Manifest (hu-HU):

{
  "tos": {
    "version": "1.2",
    "hash": "a1b2c3d4e5",
    "file": "tos.v1.2.a1b2c3d4e5.html"
  }
}

## Error handling

- If no input file is found for a culture → culture skipped.  
- If filename does not match tos.v<version>.html → culture skipped.  
- If identical content already exists in output → skipped.  
- If any unexpected error occurs, it will be logged, and the process continues for other cultures.  

## Exit codes

- 0 – success, no errors  
- 1 – one or more cultures failed to process  
