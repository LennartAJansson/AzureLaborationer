#Always start Azure scripts with this, to change your subscriptionid edit AzureLogin.ps1:
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ("$scriptPath\AzureLogin.ps1")

"Active subscription $subscription"
if($subscription -eq "")
{
	exit 1
}

$location = "northeurope"
$resourceGroup = "$($Prefix)SqlServerRG"
$sqlServer = "$($Prefix)SqlServer"
$firewallRule = "$($Prefix)SqlServerFirewall"
$dbName = "weatherforecastdb"
$dbUser = "$($Prefix)sa"
$dbPassword = "SuperSecret123!"

$keyVaultName = "$($Prefix)KV"
$connectionStringName = "ConnectionStrings--WeatherForecastDb"

$keyVaultNames = az keyvault list --query '[].[name]' --output tsv
if (!($keyVaultNames -Contains $keyVaultName))
{
	"You must create a keyvault with SetupKeyVault.ps1 first"
	exit 1
}

# Create a resource group
"az group create -n $resourceGroup -l $location"
$json = az group create -n $resourceGroup -l $location
$json | Out-File -FilePath "$logPath\$resourceGroup.json"

# Create a SQL Server 
"az sql server create -n $sqlServer -g $resourceGroup -l $location -u $dbUser -p $dbPassword"
$json = az sql server create -n $sqlServer -g $resourceGroup -l $location -u $dbUser -p $dbPassword

# For some reason az sql create doesn't return a clean json as output
# It contains the word "None" before the actual json text
"Cleaning errors in returned json due to official bug: https://github.com/Azure/azure-cli/issues/18948"
$json = $json -replace ".*None"
$json | Out-File -FilePath "$logPath\$sqlServer.json"

# Create an object of the json so we can get $serverObject.fullyQualifiedDomainName
$serverObject = $json | ConvertFrom-Json
$fqdn = $serverObject.fullyQualifiedDomainName
$connectionString = "Server=tcp:$fqdn,1433;Initial Catalog=$dbName;Persist Security Info=False;User ID=$dbUser;Password=$dbPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Trick to get my own public ip
$myIp = curl ifconfig.me

# Create a firewall rule for accessing the SQL-server from SSMS or Visual Studio
"az sql server firewall-rule create -g $resourceGroup -s $sqlServer -n $firewallRule --start-ip-address $myIp --end-ip-address $myIp"
$json = az sql server firewall-rule create -g $resourceGroup -s $sqlServer -n $firewallRule --start-ip-address $myIp --end-ip-address $myIp
$json | Out-File -FilePath "$logPath\$firewallRule.json"

# Create a SQL Database 
"az sql db create -g $resourceGroup -s $sqlServer -n $dbName"
$json = az sql db create -g $resourceGroup -s $sqlServer -n $dbName 
$json | Out-File -FilePath "$logPath\$dbName.json"

# Add connectionstring to earlier created KeyVault
"az keyvault secret set --vault-name $keyVaultName --name $connectionStringName --value $connectionString"
$json = az keyvault secret set --vault-name $keyVaultName --name $connectionStringName --value $connectionString
$json | Out-File -FilePath "$logPath\$connectionStringName.json"


"Your connectionstring is:"
$connectionString