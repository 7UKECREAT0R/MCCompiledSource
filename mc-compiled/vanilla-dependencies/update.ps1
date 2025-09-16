# Base URL for the files
$baseUri = "https://raw.githubusercontent.com/Mojang/bedrock-samples/refs/heads/main/metadata/vanilladata_modules/"

# Array of file names to download
$files = @(
    "mojang-blocks.json",
    "mojang-camera-presets.json",
    "mojang-effects.json",
    "mojang-enchantments.json",
    "mojang-entities.json",
    "mojang-items.json"
)

# Loop through each file and download it
foreach ($file in $files) {
    $downloadUri = $baseUri + $file
    $outputFile = Join-Path -Path $PWD -ChildPath $file

    Write-Host "Downloading $downloadUri to $outputFile..."
    Invoke-WebRequest -Uri $downloadUri -OutFile $outputFile
}

Write-Host "All files have been downloaded."