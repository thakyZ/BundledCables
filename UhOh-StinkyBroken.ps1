Param()

$DidJoke = $False;
$Joke = $False;

If (Test-Path -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath ".soup") -PathType Leaf) {
  $Joke = $True;
}

Function Invoke-AskWhyItBroke() {
  $Prompt = (Read-Host -Prompt "Please describe in great detail why the Unity project broke");

  If (-not [string]::IsNullOrEmpty($Prompt)) {
    Write-Host -ForegroundColor Red -Object "It doesn't matter I wasted your time ;3c";
  }
}

If (-not $Joke) {
  Invoke-AskWhyItBroke;
  New-Item -ItemType File -Path (Join-Path -Path $PSScriptRoot -ChildPath ".soup") | Out-Null;
  $DidJoke = $True;
}

$Process = (Get-Process -ErrorAction SilentlyContinue -Name "unity");

While ($Null -ne $Process) {
  If ($Null -ne $Process) {
    If ($DidJoke) {
      Write-Host -ForegroundColor Yellow -Object "But, in all seriousness please shut down unity."
    } Else {
      Write-Host -ForegroundColor Yellow -Object "Please shut down unity."
    }
  }
  $Null = Read-Host -Prompt "Press any key to continue..."
  $Process = (Get-Process -ErrorAction SilentlyContinue -Name "unity");
}

If ($Null -eq $Process) {
  Write-Host -Object "Working...";

  Push-Location -LiteralPath (Join-Path -Path $PSScriptRoot -ChildPath "BundledCables-Unity");
  Remove-Item -Force -Recurse -LiteralPath (Join-Path -Path $PWD -ChildPath ".vs");
  Remove-Item -Force -Recurse -LiteralPath (Join-Path -Path $PWD -ChildPath "Library");
  Remove-Item -Force -Recurse -LiteralPath (Join-Path -Path $PWD -ChildPath "Logs");
  Remove-Item -Force -Recurse -LiteralPath (Join-Path -Path $PWD -ChildPath "Temp");
  Remove-Item -Force -Recurse -LiteralPath (Join-Path -Path $PWD -ChildPath "UserSettings");
  Pop-Location;
}

Write-Host -Object "Done!"