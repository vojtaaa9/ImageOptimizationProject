param (
  [Parameter(Mandatory=$True)]
  [string]$dir
)

Write-Host "";
Write-Host "------------------------"
Write-Host "Normalize All tif Images using libvips"
Write-Host "------------------------"
Write-Host ""


$outDir = Get-ChildItem -Path $dir -Filter *.tif

foreach($image in $outDir) {
    $newName = $image.Directory.FullName + "\vips-" + $image.BaseName +".tif" 
    Write-Host $newName
    vips.exe thumbnail $image.FullName $newName 2048 --height 2048 --vips-info
}

Write-Host Processed $outDir.Length TIFF Files