properties {
    $projectName            = "Cedar.CommandHandling"
    $buildNumber            = 0
    $rootDir                = Resolve-Path .\
    $buildOutputDir         = "$rootDir\build"
    $mergedDir              = "$buildOutputDir\merged"
    $reportsDir             = "$buildOutputDir\reports"
    $srcDir                 = "$rootDir\src"
    $packagesDir            = "$srcDir\packages"
    $solutionFilePath       = "$srcDir\$projectName.sln"
    $assemblyInfoFilePath   = "$srcDir\SharedAssemblyInfo.cs"
    $ilmergePath            = FindTool "ILMerge.*\tools\ilmerge.exe" "$packagesDir"
    $xunitRunner            = FindTool "xunit.runner.console.*\tools\xunit.console.exe" "$packagesDir"
    $npmDirectory           = "$srcDir\node_modules"
    $nugetPath              = "$srcDir\.nuget\nuget.exe"
    $nodePath               = FindTool "Node.js.*\node.exe" "$packagesDir"
    $npmPath                = FindTool "Npm.js.*\tools\npm.cmd" "$packagesDir"
    $gulpPath               = "$npmDirectory\gulp\bin\gulp.js"
    $karmaPath              = "$npmDirectory\karma\bin\karma"
    $script:errorOccured    = $false
}

task default -depends Clean, UpdateVersion, RunTests, CreateNuGetPackages, AssertBuildResult

task Clean {
    Remove-Item $buildOutputDir -Force -Recurse -ErrorAction SilentlyContinue
    exec { msbuild /nologo /verbosity:quiet $solutionFilePath /t:Clean /p:platform="Any CPU"}
}

task RestoreNuget {
    Get-PackageConfigs |% {
        "Restoring " + $_
        &$nugetPath install $_ -o "$srcDir\packages" -configfile $_
    }
}

task UpdateVersion {
    $version = Get-Version $assemblyInfoFilePath
    $oldVersion = New-Object Version $version
    $newVersion = New-Object Version ($oldVersion.Major, $oldVersion.Minor, $oldVersion.Build, $buildNumber)
    Update-Version $newVersion $assemblyInfoFilePath
}

task Compile -depends CompileJs {
    exec { msbuild /nologo /verbosity:quiet $solutionFilePath /p:Configuration=Release /p:platform="Any CPU"}
}

task RunTests -depends Compile, TestJs {
    New-Item $reportsDir\xUnit\$project -Type Directory -ErrorAction SilentlyContinue
    .$xunitRunner "$srcDir\Cedar.CommandHandling.Tests\bin\Release\Cedar.CommandHandling.Tests.dll" -html "$reportsDir\xUnit\$project\index.html"
}

task ILMerge -depends Compile {
    New-Item $mergedDir -Type Directory -ErrorAction SilentlyContinue

    $dllDir = "$srcDir\Cedar.CommandHandling.Http\bin\Release"
    $inputDlls = "$dllDir\Cedar.CommandHandling.Http.dll"
    @(  "CuttingEdge.Conditions",
        "Microsoft.Owin",
        "Newtonsoft.Json",
        "Owin",
        "OwinHttpMessageHandler",
        "System.Net.Http.Formatting",
        "System.Web.Http",`
        "System.Web.Http.Owin") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
    Invoke-Expression "$ilmergePath /targetplatform:v4 /internalize /allowDup /target:library /log /out:$mergedDir\Cedar.CommandHandling.Http.dll $inputDlls"
}

task CompileJs -depends RestoreNpm {
    pushd $srcDir; pushd "Cedar.CommandHandling.Http.Js"

    try {
            Exec {
                & $nodePath $gulpPath "compile" --outdir=$buildOutputDir
            }
        }
        catch [Exception] {
            $script:errorOccured = $true
        }

    popd; popd
}

task TestJs {
    pushd $srcDir; pushd "Cedar.CommandHandling.Http.Js"

    try {
            Exec {
                & $nodePath $karmaPath start --single-run --outdir=$buildOutputDir
            }
        }
        catch [Exception] {
            $script:errorOccured = $true
        }

    popd; popd
}

task RestoreNpm -depends RestoreNuget {
    $env:Path = $nodePath + ";" + $env:Path
    pushd $srcDir
    & $npmPath install
    popd
}

task CreateNuGetPackages -depends ILMerge {
    $versionString = Get-Version $assemblyInfoFilePath
    $version = New-Object Version $versionString
    $packageVersion = $version.Major.ToString() + "." + $version.Minor.ToString() + "." + $version.Build.ToString() + "-build" + $buildNumber.ToString().PadLeft(5,'0')
    $packageVersion
    gci $srcDir -Recurse -Include *.nuspec | % {
        exec { .$srcDir\.nuget\nuget.exe pack $_ -o $buildOutputDir -version $packageVersion }
    }
}

task AssertBuildResult {
    if ($script:errorOccured){
        Throw ("Error: One of the build tasks failed. Please check the output above")
    }
}

function Get-PackageConfigs {
    $packages = gci $srcDir -Recurse "packages.config" -ea SilentlyContinue
    $customPachage = gci $srcDir -Recurse "packages.*.config" -ea SilentlyContinue
    $packages + $customPachage  | foreach-object { $_.FullName }
}

function FindTool {
	param(
		[string]$name,
		[string]$packageDir
	)

	$result = Get-ChildItem "$packageDir\$name" | Select-Object -First 1

	return $result.FullName
}

function Get-Version
{
	param
	(
		[string]$assemblyInfoFilePath
	)
	Write-Host "path $assemblyInfoFilePath"
	$pattern = '(?<=^\[assembly\: AssemblyVersion\(\")(?<versionString>\d+\.\d+\.\d+\.\d+)(?=\"\))'
	$assmblyInfoContent = Get-Content $assemblyInfoFilePath
	return $assmblyInfoContent | Select-String -Pattern $pattern | Select -expand Matches |% {$_.Groups['versionString'].Value}
}

function Update-Version
{
	param
    (
		[string]$version,
		[string]$assemblyInfoFilePath
	)

	$newVersion = 'AssemblyVersion("' + $version + '")';
	$newFileVersion = 'AssemblyFileVersion("' + $version + '")';
	$tmpFile = $assemblyInfoFilePath + ".tmp"

	Get-Content $assemblyInfoFilePath |
		%{$_ -replace 'AssemblyFileVersion\("[0-9]+(\.([0-9]+|\*)){1,3}"\)', $newFileVersion }  | Out-File -Encoding UTF8 $tmpFile

	Move-Item $tmpFile $assemblyInfoFilePath -force
}
