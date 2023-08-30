$subscriptionExists = Test-Path -Path env:SUBSCRIPTIONID
if($subscriptionExists -eq $false)
{
    "You must set the subscription id as an environment variable, use following command:"
    "SETX SUBSCRIPTIONID ""YOUR-SUBSCRIPTION-ID"""
    $subscription = ""
    exit 1
}

$subscription = $env:SUBSCRIPTIONID
#Ändrat detta script till att automatiskt använda ditt inloggade användarnamn som prefix
$prefix = $env:USERNAME
$scriptPath = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
$logPath = "$scriptPath\Logs"

"Login to Azure using subscription"
"For more information:"
"https://docs.microsoft.com/sv-se/cli/azure/?view=azure-cli-latest"

az login

"Selected subscription is $subscription. Look in the list of subscriptions above and enter a new one if needed"
"You could change the script AzureLogin so it always use the right one as default!"
$newSubscription = Read-Host 'New subscriptionid (empty for current):'

if([String]::IsNullOrWhiteSpace($newSubscription) -eq $false)
{
    $subscription = $newSubscription
}

az account set --subscription $subscription

if((Test-Path -Path $logPath) -eq $false)
{
    New-Item -ItemType Directory -Force -Path $logPath | Out-Null
}
