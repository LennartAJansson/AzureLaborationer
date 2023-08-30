#Always start Azure scripts with this, to change your subscriptionid edit AzureLogin.ps1:
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ("$scriptPath\AzureLogin.ps1")

"Active subscription $subscription"
if($subscription -eq "")
{
	exit 1
}

$location = "swedencentral"
$resourceGroup = "$($Prefix)WeatherWebApiRG"
$webApiServicePlan = "$($Prefix)WeatherWebApiServicePlan"
$webApi = "$($Prefix)WeatherWebApi"
$webApiIdentity = "$($Prefix)WeatherWebApiIdentity"
$webApiKeyVaultName = "AzureKeyVault__Uri"
$keyVaultName = "$($Prefix)KV"
$firewallRule = "SqlServerFW"

$keyVaultNames = az keyvault list --query '[].[name]' --output tsv
if (!($keyVaultNames -Contains $keyVaultName))
{
	"You must create a keyvault with SetupKeyVault.ps1 first"
	exit 1
}

# Create a resource group for all related to our web api
"az group create -n $resourceGroup -l $location"
$json = az group create -n $resourceGroup -l $location
$json | Out-File -FilePath "$logPath\$resourceGroup.json"

# Create service plan for this web api
"az appservice plan create -g $resourceGroup -n $webApiServicePlan --is-linux"
$json = az appservice plan create -g $resourceGroup -n $webApiServicePlan --is-linux
$json | Out-File -FilePath "$logPath\$webApiServicePlan.json"

# Create app service for this web api
"az webapp create -g $resourceGroup -p $webApiServicePlan -n $webApi --runtime 'DOTNET:6.0'"
$json = az webapp create -g $resourceGroup -p $webApiServicePlan -n $webApi --runtime 'DOTNET:6.0' 
$json | Out-File -FilePath "$logPath\$webApi.json"
$webApiObj = $json | ConvertFrom-Json
$ipAddresses = $webApiObj.outboundIpAddresses.Split(",")

# Create an identity for this web api that will be used when identifying against keyvault
"az webapp identity assign -g $resourceGroup -n $webApi"
$json = az webapp identity assign -g $resourceGroup -n $webApi
$json | Out-File -FilePath "$logPath\$webApiIdentity.json"
$identity = $json | ConvertFrom-Json
$objectId = $identity.principalId

# Set policy on keyvault so this web api is allowed to use it
"az keyvault set-policy -n $keyVaultName --object-id $objectId --secret-permissions get list"
$json = az keyvault set-policy -n $keyVaultName --object-id $objectId --secret-permissions get list
$json | Out-File -FilePath "$logPath\$keyVaultName-Policy.json"
$keyVaultObj = $json | ConvertFrom-Json
$keyVaultUri = $keyVaultObj.properties.vaultUri

# Set web api configuration so it finds the keyvault
"az webapp config appsettings set -g $resourceGroup -n $webApi --settings '$webApiKeyVaultName=$keyVaultUri'"
$json = az webapp config appsettings set -g $resourceGroup -n $webApi --settings "$webApiKeyVaultName=$keyVaultUri"
$json | Out-File -FilePath "$logPath\$webApiKeyVaultName.json"


# Find your SQL Servers to manage the firewall settings in them
$json = az sql server list
$sqlServers = $json | ConvertFrom-json

# Add firewall openings to earlier created SQL Server for all IP addresses to this web api
foreach($sqlServer in $sqlServers)
{
	"Adding firewall settings for server $($sqlServer.name)"
	foreach($ipAddress in $ipAddresses)
	{
		"Adding firewall settings for IP $ipAddress"
		"az sql server firewall-rule create -g $($sqlServer.resourceGroup) -s $($sqlServer.name) -n $firewallRule-$ipAddress --start-ip-address $ipAddress --end-ip-address $ipAddress"
		$json = az sql server firewall-rule create -g $($sqlServer.resourceGroup) -s $($sqlServer.name) -n "$firewallRule-$ipAddress" --start-ip-address $ipAddress --end-ip-address $ipAddress
		$json | Out-File -FilePath "$logPath\$($sqlServer.name)-$firewallRule-$ipAddress.json"
	}
}