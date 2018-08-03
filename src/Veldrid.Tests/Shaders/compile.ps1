$fileNames = Get-ChildItem -Path $PSScriptRoot -Recurse

foreach ($file in $fileNames)
{
    if ($file.Name.EndsWith("vert") -Or $file.Name.EndsWith("frag") -Or $file.Name.EndsWith("comp"))
    {
        $inPath = $file.FullName
        $outPath = $inPath + ".spv"
        $inputLastWrite = (Get-ChildItem $inPath).LastWriteTime
        if (-not [System.IO.File]::Exists($outPath) -or (Get-ChildItem $outPath).LastWriteTime -le $inputLastWrite)
        {
            Write-Host "Compiling $file" -> $outPath
            glslangvalidator -V $inPath -o $outPath
        }
    }
}