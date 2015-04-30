param(
    [int]$buildNumber = 0,
    [string]$task = "default",
    [string]$nugetSource = "http://www.nuget.org/api/v2"
    )

if(Test-Path Env:\APPVEYOR_BUILD_NUMBER){
    $buildNumber = [int]$Env:APPVEYOR_BUILD_NUMBER
    Write-Output "Using APPVEYOR_BUILD_NUMBER"
}

if(Test-Path Env:\TEAMCITY_PROJECT_NAME){
    Write-Output "Building in TeamCity"
}

"Build number $buildNumber"

src\.nuget\nuget.exe i src\.nuget\packages.config -o src\packages -source $nugetSource

Import-Module .\src\packages\psake.4.4.1\tools\psake.psm1
Import-Module .\teamcity.psm1

Invoke-Psake .\default.ps1 $task -framework "4.5.1x64" -properties @{ buildNumber=$buildNumber; nugetSource=$nugetSource }

Remove-Module teamcity
Remove-Module psake
