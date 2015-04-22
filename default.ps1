properties {
    $projectName            = "Cedar.CommandHandling"
    $buildNumber            = 0
    $rootDir                = Resolve-Path .\
    $buildOutputDir         = "$rootDir\build"
    $mergedDir              = "$buildOutputDir\merged"
    $reportsDir             = "$buildOutputDir\reports"
    $srcDir                 = "$rootDir\src"
    $package_directory      = "$srcDir\packages"
    $solutionFilePath       = "$srcDir\$projectName.sln"
    $assemblyInfoFilePath   = "$srcDir\SharedAssemblyInfo.cs"
    $ilmergePath            = "$srcDir\packages\ILMerge.2.14.1208\tools\ilmerge.exe"
    $npm_directory          = "$srcDir\node_modules"
    $nugetPath 		          = "$srcDir\.nuget\nuget.exe"
    $node_path              = FindTool "Node.js.*\node.exe" "$package_directory"
    $npm_path               = FindTool "Npm.js.*\tools\npm.cmd" "$package_directory"
    $gulp_path              = "$npm_directory\gulp\bin\gulp.js"
    $karma_path             = "$npm_directory\karma\bin\karma"
    $script:errorOccured = $false
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
    $xunitRunner = "$srcDir\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.exe"

    .$xunitRunner "$srcDir\Cedar.CommandHandling.Tests\bin\Release\Cedar.CommandHandling.Tests.dll" /html "$reportsDir\xUnit\$project\index.html"
}

task ILMerge -depends Compile {
    New-Item $mergedDir -Type Directory -ErrorAction SilentlyContinue

    $dllDir = "$srcDir\Cedar.CommandHandling.Http\bin\Release"
    $inputDlls = "$dllDir\Cedar.CommandHandling.Http.dll"
    @("CuttingEdge.Conditions", "Microsoft.Owin", "Newtonsoft.Json", "Owin", "OwinHttpMessageHandler", "System.Net.Http.Formatting", "System.Web.Http",`
        "System.Web.Http.Owin") |% { $inputDlls = "$inputDlls $dllDir\$_.dll" }
    Invoke-Expression "$ilmergePath /targetplatform:v4 /internalize /allowDup /target:library /log /out:$mergedDir\Cedar.CommandHandling.Http.dll $inputDlls"
}

task CompileJs -depends RestoreNpm {
    pushd $srcDir; pushd "Cedar.CommandHandling.Http.Js"

    try {
            Exec {
                & $node_path $gulp_path "compile" --outdir=$output_directory
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
                & $node_path $karma_path start --single-run --outdir=$output_directory
            }
        }
        catch [Exception] {
            $script:errorOccured = $true
        }

    popd; popd
}

task RestoreNpm -depends RestoreNuget {
    $env:Path = $node_path + ";" + $env:Path
    pushd $srcDir
    exec { & $npm_path install --registry $npm_registry }
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
