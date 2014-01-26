#Create Product Code retriever.
Add-Type @' 
using System;
using System.Text;
using System.Runtime.InteropServices;
      
public static class ProductHelper
{       
    [DllImport("Msi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern UInt32 MsiEnumRelatedProducts([In]string strUpgradeCode,[In] int reserved,[In] int iIndex,[Out] StringBuilder sbProductCode);

    [DllImport("Msi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern UInt32 MsiGetProductInfo([In]string productCode, [In] string property, [Out] StringBuilder value, [In][Out]ref Int32 length);
    
    public static string GetProductCodeForUpgradeCodeAndVersionString(string upgradeCode, string versionString){ 
        var index = 0;
        var productCode = new StringBuilder(39);
        while (0 == MsiEnumRelatedProducts(upgradeCode, 0, index++, productCode))
        {                
            Int32 length = 512;
            var version = new StringBuilder(length);
            MsiGetProductInfo(productCode.ToString(), "VersionString", version, ref length);

            if(version.ToString() == versionString)
                return productCode.ToString();
        }
        return null;
    } 
} 
'@ 

$packageName = 'ChocoPM.install'
$packageVersionString = '0.1.1.20140126'
$installerType = 'MSI' 
$msiUpgradeCode = "{34f1c107-b1aa-447a-95bb-ebcffaa2e9c2}"
$silentArgs = ''
$validExitCodes = @(0) 
$productCode = [ProductHelper]::GetProductCodeForUpgradeCodeAndVersionString($msiUpgradeCode, $packageVersionString)
if($productCode -eq $null)
{
	Write-Host "$packageName ($packageVersionString) is not current installed"
}
else
{
	$msiArgs = "/x $productCode /qn /norestart"

	$uninstallMessage = "Uninstalling $packageName..."
	write-host $uninstallMessage

	Start-ChocolateyProcessAsAdmin "$msiArgs" 'msiexec' -validExitCodes $validExitCodes
}