#NOTE: Please remove any commented lines to tidy up prior to releasing the package, including this one

$packageName = 'ChocoPM.portable' # arbitrary name for the package, used in messages
$installDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

try { 
  $file = Join-Path "$installDir" "$($packageName).7z"

  Start-Process "7za" -ArgumentList "x -o`"$installDir`" -y `"$file`"" -Wait

  Write-ChocolateySuccess "$packageName"
} catch {
  Write-ChocolateyFailure "$packageName" "$($_.Exception.Message)"
  throw 
}