Param()

$UnityAssembliesDirectory = (Get-Item -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath "BundledCables-Unity" -AdditionalChildPath @("Assets", "Assemblies")));
$PatchAssembliesDirectory = (Get-Item -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath "BundledCables" -AdditionalChildPath @("libs")));

Function Get-StationeersDirectory() {
  If (Test-Path -LiteralPath (Join-Path -Path "E:\" -ChildPath "SteamLibrary" -AdditionalChildPath @("SteamApps", "common", "Stationeers")) -PathType Leaf) {
    Return (Get-Item -LiteralPath (Join-Path -Path "E:\" -ChildPath "SteamLibrary" -AdditionalChildPath @("SteamApps", "common", "Stationeers")));
  } Else {
    $GetStationeersDir = (Read-Host -Prompt "Path to the Stationeers install:");
    Return (Get-Item -LiteralPath $GetStationeersDir);
  }
}

$PatchAssemblyFiles = @{
  Managed = @(
    "Assembly-CSharp.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.PhysicsModule.dll"
  );
  BepInEx = @(
    "StationeersMods.Interface.dll",
    "0Harmony.dll"
  );
};

$UnityAssemblyFiles = @{
  Managed = @();
  BepInEx = @(
    "0Harmony.dll",
    "MonoMod.RuntimeDetour.dll",
    "MonoMod.Utils.dll"
  );
}

$ManagedPath = (Join-Path -Path (Get-StationeersDirectory).FullName -ChildPath "rocketstation_Data" -AdditionalChildPath @("Managed"));
$BepInExPath = (Join-Path -Path (Get-StationeersDirectory).FullName -ChildPath "BepInEx" -AdditionalChildPath @("core"));

$UnityAssemblyFiles.Managed = (((Get-ChildItem -Path $ManagedPath) | Where-Object {
      # Return -not $_.BaseName.StartsWith("UnityEngine") -and -not ($_.BaseName.StartsWith("Unity") -and -not $_.Name.Equals("Unity.TextMeshPro.dll") -and -not $_.Name.Equals("Unity.Collections.dll")) -and -not $_.BaseName.StartsWith("UnityEngine")
      Return -not ($_.BaseName.StartsWith("Uni"))
    }).Name);


Function Copy-StationeersBinaries() {
  Param(
    [string[]]
    $Paths,
    [ValidateSet("Managed", "BepInEx")]
    [string]
    $Type,
    [string]
    $Destination
  )

  $BasePath = ($Type -eq "Managed" ? $ManagedPath : $BepInExPath);

  ForEach ($Path in $Paths) {
    Copy-Item -Path (Join-Path -Path $BasePath -ChildPath $Path) -Destination (Join-Path -Path $Destination -ChildPath $Path) -Verbose
  }
}

Copy-StationeersBinaries -Paths $PatchAssemblyFiles.Managed -Type "Managed" -Destination $PatchAssembliesDirectory
Copy-StationeersBinaries -Paths $PatchAssemblyFiles.BepInEx -Type "BepInEx" -Destination $PatchAssembliesDirectory

Copy-StationeersBinaries -Paths $UnityAssemblyFiles.Managed -Type "Managed" -Destination $UnityAssembliesDirectory
Copy-StationeersBinaries -Paths $UnityAssemblyFiles.BepInEx -Type "BepInEx" -Destination $UnityAssembliesDirectory
