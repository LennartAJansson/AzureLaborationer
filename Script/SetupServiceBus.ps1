#Always start Azure scripts with this, to change your subscriptionid edit AzureLogin.ps1:
$ScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
. ("$ScriptDirectory\AzureLogin.ps1")

"Active subscription $subscription"
if($subscription -eq "")
{
	exit 1
}

$resourceGroup = "$($Prefix)SBRG"
$location = "swedencentral"
$keyVaultName = "$($Prefix)KV"

$serviceBusName = "$($Prefix)ServiceBus"
$topicName = "MyTopic"
$subscriptionName = "MySubscription"
$queueName = "MyQueue"

$keyVaultNames = az keyvault list --query '[].[name]' --output tsv
if (!($keyVaultNames -Contains $keyVaultName))
{
	"You must create a keyvault with SetupKeyVault.ps1 first"
	exit 1
}

$serviceBusNames = az servicebus namespace list --query '[].name' --output tsv
if (!($serviceBusNames -Contains $serviceBusName)) 
{
	Write-Output "Creating a resource group"
	$json = az group create --name $resourceGroup --location $location
	$json | Out-File -FilePath "$logPath\$resourceGroup.json"
	$group = $json | ConvertFrom-Json
	Write-Output "Your properties for the resourcegroup is stored in $logPath\$resourceGroup.json"

	Write-Output "Creating a servicebus namespace"
	$json = az servicebus namespace create --name $serviceBusName --resource-group $resourceGroup --location $location --sku Standard --subscription $subscription
	$json | Out-File -FilePath "$logPath\$serviceBusName.json"
	$servicebus = $json | ConvertFrom-Json
	Write-Output "Your properties for the servicebus namespace is stored in $logPath\$serviceBusName.json"

	Write-Output "Getting the connectionstring for the servicebus namespace"
	$connectionString = $(az servicebus namespace authorization-rule keys list --resource-group $resourceGroup --namespace-name $serviceBusName --name RootManageSharedAccessKey --query primaryConnectionString --output tsv)

	Write-Output "Creating Queue, Topic and Subscriber for the SDK examples in this solution"
	$json = az servicebus queue create --name $queueName --namespace-name $serviceBusName --resource-group $resourceGroup --subscription $subscription
	$json = az servicebus topic create --name $topicName --namespace-name $serviceBusName --resource-group $resourceGroup --subscription $subscription
	$json = az servicebus topic subscription create --name $subscriptionName --topic-name $topicName --namespace-name $serviceBusName --resource-group $resourceGroup --subscription $subscription
	Write-Output "Creating Queue, Topic and Subscriber for the SDK examples in this solution"

	Write-Output "Creating a secret in the keyvault for the connectionstring"
	$json = az keyvault secret set --name "ServiceBus--ConnectionString" --vault-name $keyVaultName --value $connectionString
	$json | Out-File -FilePath "$logPath\ServiceBus--ConnectionString.json"
	$secret = $json | ConvertFrom-Json
	Write-Output "Your connectionstring for the servicebus namespace is stored in the keyvault as ServiceBus--ConnectionString (ServiceBus:ConnectionString)"
	
	$json = az keyvault secret set --name "ServiceBus--Queue" --vault-name $keyVaultName --value $queueName
	$json | Out-File -FilePath "$logPath\ServiceBus--Queue.json"
	$json = az keyvault secret set --name "ServiceBus--Topic" --vault-name $keyVaultName --value $topicName
	$json | Out-File -FilePath "$logPath\ServiceBus--Topic.json"
	$json = az keyvault secret set --name "ServiceBus--Subscription" --vault-name $keyVaultName --value $subscriptionName
	$json | Out-File -FilePath "$logPath\ServiceBus--Subscription.json"

	Write-Output "Your connectionstring to the service bus is $connectionString"
	Write-Output "It is stored in the keyvault and in .\servicebus.json"
	Write-Output "Remember that if you are using ServiceBus SDK, you will have to create Queues, Topics and Subscribers on your own."
	Write-Output "This script has created a queue named $queueName, a topic named $topicName and a topic subscription named $subscriptionName, these are the names used in the SDK examples"
	Write-Output "When using MassTransit it will take care of creating queues, topics and subscriptions for you."

	$json = "{""ServiceBus"":{""ConnectionString"":""$connectionString"",""Queue"":""$queueName"",""Topic"":""$topicName"",""Subscription"":""$subscriptionName""}}"
	$json | Out-File -FilePath "..\servicebus.json"
	Write-Output "Your connectionstring for the servicebus namespace is stored in ..\servicebus.json"
}
else
{
	Write-Output "ServiceBus namespace $serviceBusName already created"
}