Param()

$PackageManafest = (Join-Path -Path $PSScriptRoot -ChildPath "$((Get-Item -Path $PWD).Name)-Unity" -AdditionalChildPath @("Packages", "manifest.json"));
$UnityPackages = (Join-Path -Path $env:LocalAppData -ChildPath "Unity" -AdditionalChildPath @("cache", "packages", "packages.unity.com"));

$Manifest = @{}

If (Test-Path -LiteralPath $PackageManafest -PathType Leaf) {
  $_Manifest = (Get-Content $PackageManafest | ConvertFrom-Json);
  $_Manifest.dependencies.psobject.properties | ForEach-Object { $Manifest[$_.Name] = $_.Value }
} Else {
  Write-Host $PackageManafest
  Write-Error -Message "Package manifest does not exist."
  Exit 1;
}

$TextMeshPro = (Join-Path -Path $UnityPackages -ChildPath "com.unity.textmeshpro@$($Manifest["com.unity.textmeshpro"])" -AdditionalChildPath @("Scripts", "Runtime"));
$TextMeshProFix = (Join-Path -Path $PSScriptRoot -ChildPath "TMP_Fix.zip");
If (-not (Test-Path -LiteralPath $TextMeshPro)) {
  Write-Host $TextMeshPro
  Write-Error -Message "TextMeshPro directory does not exist. Please launch the Unity project at least once."
  Exit 1;
}

Try {
  $WebRequest = (Invoke-WebRequest -Uri "https://github.com/jixxed/StationeersMods/raw/main/doc/TMP_Fix.zip" -OutFile $TextMeshProFix -ErrorAction SilentlyContinue);
  Write-Output $WebRequest | Out-Host;
} Catch {
  Write-Error -Exception $_.Exception -Message $_.Exception.Message;
  Exit 1;
}

$TempFolder = (Join-Path -Path $PSScriptRoot -ChildPath "temp");

$Null = (New-Item -Path $TempFolder -ItemType Directory -Force);

Expand-Archive -LiteralPath $TextMeshProFix -DestinationPath $TempFolder

ForEach ($Item in (Get-ChildItem -LiteralPath $TempFolder)) {
  If ((Test-Path -LiteralPath (Join-Path -Path $TextMeshPro -ChildPath $Item.Name)) -and -not (Test-Path -LiteralPath (Join-Path -Path $TextMeshPro -ChildPath "$($Item.Name).bak"))) {
    Rename-Item -LiteralPath (Join-Path -Path $TextMeshPro -ChildPath $Item.Name) -NewName "$($Item.Name).bak";
  }
  If ((Test-Path -LiteralPath (Join-Path -Path $TextMeshPro -ChildPath $Item.Name)) -and (Test-Path -LiteralPath (Join-Path -Path $TextMeshPro -ChildPath "$($Item.Name).bak"))) {
    Remove-Item -LiteralPath (Join-Path -Path $TextMeshPro -ChildPath $Item.Name);
  }
  Move-Item -LiteralPath $Item.FullName -Destination (Join-Path -Path $TextMeshPro -ChildPath $Item.Name);
}

Remove-Item -LiteralPath $TextMeshProFix
Remove-Item -Recurse $TempFolder;

Exit $LASTEXITCODE