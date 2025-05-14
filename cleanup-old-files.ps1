# Removes old files that have been refactored
# Run this script after verifying everything works

# Backup old files first
if (-not (Test-Path -Path "backup")) {
    New-Item -ItemType Directory -Path "backup"
}

# Backup and remove old files
$oldFiles = @(
    "DatabaseMetadataProvider.cs",
    "SqlServerTools.cs"
)

foreach ($file in $oldFiles) {
    if (Test-Path -Path $file) {
        Write-Host "Backing up $file..."
        Copy-Item -Path $file -Destination "backup/$file"
        Write-Host "Removing $file..."
        Remove-Item -Path $file
    } else {
        Write-Host "$file not found, skipping..."
    }
}

Write-Host "Cleanup complete! Old files have been backed up to the 'backup' folder."
