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

function FindTool {
	param(
		[string]$name,
		[string]$package_dir
	)

	Write-Host $package_dir
	$result = Get-ChildItem "$package_dir\$name" | Select-Object -First 1

	return $result.FullName
}