$location = "swedencentral"
$resourceGroup = "$($Prefix)WeatherWebApiRG"
$webApiServicePlan = "$($Prefix)WeatherWebApiServicePlan"
$webApi = "$($Prefix)WeatherWebApi"


az webapp up -n $webApi -g $resourceGroup -b -p $webApiServicePlan -l $location --os-type linux --runtime "DOTNETCORE:6.0"