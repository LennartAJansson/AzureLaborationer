#Always start Azure scripts with this, to change your subscriptionid edit AzureLogin.ps1:
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ("$scriptPath\AzureLogin.ps1")

"Active subscription $subscription"
if($subscription -eq "")
{
	exit 1
}

$resourceGroup = "$($Prefix)KVRG"
$location = "swedencentral"

$keyVaultName = "$($Prefix)KV"
$secretName = "ConnectionStrings--MyAppSecret"
$secretValue = "This is a secret from keyvault"

$keyVaultNames = az keyvault list --query '[].[name]' --output tsv
if (!($keyVaultNames -Contains $keyVaultName)) 
{
	"az group create --name $resourceGroup --location $location"
	$json = az group create --name $resourceGroup --location $location
	$json | Out-File -FilePath "$logPath\$resourceGroup.json"
	$group = $json | ConvertFrom-Json

	"az keyvault create --location $location --name $keyVaultName --resource-group $resourceGroup"
	$json = az keyvault create --location $location --name $keyVaultName --resource-group $resourceGroup
	$json | Out-File -FilePath "$logPath\$keyVaultName.json"
	$keyVault = $json | ConvertFrom-Json
	$keyVaultUri = $keyVault.properties.vaultUri

	"az keyvault secret set --vault-name $keyVaultName --name $secretName --value $secretValue"
	$json = az keyvault secret set --vault-name $keyVaultName --name $secretName --value $secretValue
	$json | Out-File -FilePath "$logPath\$secretName.json"
	$secret = $json | ConvertFrom-Json

	Write-Output "Use following setting when using this keyvault from .NET Core:"
	Write-Output "Name: $keyVaultName"
	Write-Output "Uri: $keyVaultUri"
} 
else 
{
	Write-Output "Key vault $keyVaultName already created"
}