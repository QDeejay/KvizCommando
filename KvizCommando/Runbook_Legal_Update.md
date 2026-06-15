# Runbook – Static Legal Content Update

This document describes the process of updating static legal content (e.g. Terms of Service) in the KvizCommando project.

## File location

Legal content is stored under language-specific folders:

- src/Server/wwwroot/legal/hu-HU/
- src/Server/wwwroot/legal/en-US/

Each version is a separate HTML file with a hashed filename.

## File naming convention

tos.v<version>.<hash>.html

- <version>: semantic version string (e.g. 1.1, 1.2).
- <hash>: short identifier generated from the file contents (e.g. abcdef1234).
- The hash is used for client-side cache invalidation.

## Manifest file

Each language folder contains a manifest.json that points to the currently valid file:

{
  "tos": {
    "version": "1.1",
    "hash": "abcdef1234",
    "file": "tos.v1.1.abcdef1234.html"
  }
}

The client always fetches the manifest first and then loads the HTML file specified inside.

## Steps for releasing a new version

1. Create new HTML file  
   - Draft the updated legal document in HTML format.  
   - Save it into src/Server/wwwroot/legal/<culture>/.  
   - Name it according to the convention: tos.v<new_version>.<new_hash>.html.  

2. Update manifest  
   - Open the language-specific manifest.json.  
   - Update the version, hash, and file fields to point to the new file.  

3. Handle old files  
   - Do not delete previous HTML files immediately; keep them for archival purposes.  
   - The manifest should always point to the single active version.  

4. Build and deploy  
   - Run the full build process.  
   - After deployment, verify that the client loads the new file through the manifest.  

5. Testing  
   - Manually open the following in a browser:  
     - https://<host>/legal/hu-HU/manifest.json  
     - https://<host>/legal/en-US/manifest.json  
   - Confirm that the JSON reflects the expected version.  
   - Open the file entry directly and check that the HTML displays correctly.  

## Example

src/Server/wwwroot/legal/hu-HU/tos.v1.2.9876543210.html  
src/Server/wwwroot/legal/en-US/tos.v1.2.9876543210.html  

manifest.json:

{
  "tos": {
    "version": "1.2",
    "hash": "9876543210",
    "file": "tos.v1.2.9876543210.html"
  }
}
