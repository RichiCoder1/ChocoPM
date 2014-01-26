$packageName = 'ChocoPM.install'
$installerType = 'MSI' 
$installDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)" 
$msiPath = Join-Path $installDir 'ChocoPM.msi'
$silentArgs = '/quiet'
$validExitCodes = @(0) 

Install-ChocolateyInstallPackage "$packageName" "$installerType" "$silentArgs" "$msiPath" -validExitCodes $validExitCodes