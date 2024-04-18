# Define the path to the directory containing the zip files
$zipFilesDirectory = ".\"

# Get all zip files in the specified directory
$zipFiles = Get-ChildItem -Path $zipFilesDirectory -Filter *.zip

# Iterate through each zip file found
foreach ($zipFile in $zipFiles) {
    # Create a folder path based on the zip file name (without the extension)
    $extractPath = Join-Path -Path $zipFilesDirectory -ChildPath ($zipFile.BaseName)

    # Check if the directory already exists
    if (-Not (Test-Path -Path $extractPath)) {
        # Create the directory if it does not exist
        New-Item -Path $extractPath -ItemType Directory
    }

    # Try to extract the zip file to the newly created directory
    try {
        # Extract the zip file using Expand-Archive cmdlet
        Expand-Archive -Path $zipFile.FullName -DestinationPath $extractPath -Force
        Write-Host "Successfully extracted '$($zipFile.Name)' to '$extractPath'"
    }
    catch {
        # Handle any errors during extraction
        Write-Host "Error extracting '$($zipFile.Name)': $_"
    }
}

Write-Host "Finished extracting all zip files."
