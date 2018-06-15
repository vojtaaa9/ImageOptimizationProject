param (
  [Parameter(Mandatory=$True)]
  [string]$dir
)

Write-Host "";
Write-Host "------------------------"
Write-Host "SVGO batch optimize and rename with .min.svg Ending"
Write-Host You need SVGO to process the SVGs! Install it with 'npm install -g svgo' or refer to https://github.com/svg/svgo
Write-Host "------------------------"
Write-Host ""


$inDir = Get-ChildItem -Path $dir -Filter *.svg

# Create Output directory, if none exists
$outDir = "$dir\out"
Write-Host Output in: $outDir

If(!(Test-Path $outDir))
{
      New-Item -ItemType Directory -Force -Path $outDir
      Write-Host "Directory for output was created: $outDir"
}


foreach($image in $inDir) {
    $name = $image.FullName
    $outputName = $image.Directory.FullName + "\out\" + $image.BaseName + ".min.svg" ;
    
    
    svgo -i $name -o $outputName
}

Write-Host "";
Write-Host "------------------------"
Write-Host Processed $inDir.Length SVG Files
Write-Host You can find the minified output in 