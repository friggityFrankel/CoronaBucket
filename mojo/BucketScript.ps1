$jh_api = "https://covid19.mathdro.id/api"
$ct_api = "https://api.covidtracking.com"
 
$world_yesterday = Import-CliXml -Path "./worldwide.xml"
$country_yesterdays = Import-Clixml -Path  "./country.xml"
$state_yesterdays = Import-Clixml -Path  "./state.xml"
 
class worldwide
{
    [string]$Name = "World Wide"
    [int]$Infected_Today = 0
    [int]$Infected_Yesterday = 0
    [int]$Infected_Delta = 0
    [int]$Dead_Today = 0
    [int]$Dead_Yesterday = 0
    [int]$Dead_Delta = 0
    [int]$Recovered_Today = 0
    [int]$Recovered_Yesterday = 0
    [int]$Recovered_Delta = 0
    [int]$Unresolved_Today = 0
    [int]$Unresolved_Yesterday = 0
    [int]$Unresolved_Delta = 0
}
 
class country
{
    [string]$Name = ""
    [string]$Query_Name = ""
    [int]$Infected_Today = 0
    [int]$Infected_Yesterday = 0
    [int]$Infected_Delta = 0
    [int]$Dead_Today = 0
    [int]$Dead_Yesterday = 0
    [int]$Dead_Delta = 0
}
 
class state
{
    [string]$Name = ""
    [string]$Query_Name = ""
    [int]$Infected_Today = 0
    [int]$Infected_Yesterday = 0
    [int]$Infected_Delta = 0
    [int]$Hospitalized_Today = 0
    [int]$Hospitalized_Yesterday = 0
    [int]$Hospitalized_Delta = 0
    [int]$Dead_Today = 0
    [int]$Dead_Yesterday = 0
    [int]$Dead_Delta = 0
}
 
$world = [worldwide]::new()
$world.Infected_Yesterday = $world_yesterday.Infected_Today
$world.Recovered_Yesterday = $world_yesterday.Recovered_Today
$world.Dead_Yesterday = $world_yesterday.Dead_Today
$world.Unresolved_Yesterday = $world_yesterday.Unresolved_Today
 
$country_items = @{}
$country_items["United States"] = "US"
$country_items["Brazil"] = "BR"
$country_items["Russia"] = "RU"
$country_items["India"] = "IN"
$country_items["Australia"] = "AUS"
$country_items["South Africa"] = "ZAF"
$country_items["Iran"] = "IR"
$country_items["Mexico"] = "MX"
$country_items["Spain"] = "ES"
$country_items["France"] = "FR"
 
$country_objs = @()
ForEach($country_item in $country_items.Keys)
{
    $tmp_country = [country]::new()
    $tmp_country.Name = $country_item
    $tmp_country.Query_Name = $country_items[$country_item]
    
    ForEach($country_yesterday in $country_yesterdays)
    {
        If($country_yesterday.Name -eq $country_item)
        {
            $tmp_county_yesterday = $country_yesterday
        }
    }
    $tmp_country.Infected_Yesterday = $tmp_county_yesterday.Infected_Today
    $tmp_country.Dead_Yesterday = $tmp_county_yesterday.Dead_Today
 
    $country_objs  += $tmp_country
}
 
$state_items = @{}
$state_items["California"] = "ca"
$state_items["Texas"] = "tx"
$state_items["Florida"] = "fl"
$state_items["Arizona"] = "az"
$state_items["Georgia"] = "ga"
$state_items["Illinois"] = "il"
 
$state_objs = @()
ForEach($state_item in $state_items.Keys)
{
    $tmp_state = [state]::new()
    $tmp_state.Name = $state_item
    $tmp_state.Query_Name = $state_items[$state_item]
 
    ForEach($state_yesterday in $state_yesterdays)
    {
        If($state_yesterday.Name -eq $state_item)
        {
            $tmp_state_yesterday = $state_yesterday
        }
    }
    $tmp_state.Infected_Yesterday = $tmp_state_yesterday.Infected_Today
    $tmp_state.Hospitalized_Yesterday = $tmp_state_yesterday.Hospitalized_Today
    $tmp_state.Dead_Yesterday = $tmp_state_yesterday.Dead_Today
 
    $state_objs  += $tmp_state
}
 
 
## Gather data
$tmp_data_json = Invoke-WebRequest -Uri "$($jh_api)"
$tmp_data = ConvertFrom-Json $tmp_data_json.Content
$world.Infected_Today = $tmp_data.confirmed.value
$world.Infected_Delta = $world.Infected_Today - $world.Infected_Yesterday
$world.Dead_Today = $tmp_data.deaths.value
$world.Dead_Delta = $world.Dead_Today - $world.Dead_Yesterday
$world.Recovered_Today = $tmp_data.recovered.value
$world.Recovered_Delta = $world.Recovered_Today - $world.Recovered_Yesterday
$world.Unresolved_Today = $world.Infected_Today - $world.Dead_Today - $world.Recovered_Today
$world.Unresolved_Delta = $world.Unresolved_Today - $world.Unresolved_Yesterday
 
ForEach($country_obj in $country_objs)
{
    Write-Host "Pulling $($country_obj.Query_Name)"
    $tmp_data_json = Invoke-WebRequest -Uri "$($jh_api)/countries/$($country_obj.Query_Name)"
    $tmp_data = ConvertFrom-Json $tmp_data_json.Content
 
    $country_obj.Infected_Today = $tmp_data.confirmed.value
    $country_obj.Infected_Delta = $country_obj.Infected_Today - $country_obj.Infected_Yesterday
    $country_obj.Dead_Today = $tmp_data.deaths.value
    $country_obj.Dead_Delta = $country_obj.Dead_Today - $country_obj.Dead_Yesterday
 
    If($country_obj.Query_Name -eq "US")
    {
        $usa = $country_obj
    }
    Write-Host "Got results for $($country_obj.Query_Name)"
}
 
ForEach($state_obj in $state_objs)
{
    Write-Host "Pulling $($state_obj.Query_Name)"
    $tmp_data_json = Invoke-WebRequest -Uri "$($ct_api)/v1/states/$($state_obj.Query_Name)/current.json"
    $tmp_data = ConvertFrom-Json $tmp_data_json.Content
 
    $state_obj.Infected_Today = $tmp_data.positive
    $state_obj.Infected_Delta = $state_obj.Infected_Today - $state_obj.Infected_Yesterday
    $state_obj.Hospitalized_Today = $tmp_data.hospitalizedCurrently
    $state_obj.Hospitalized_Delta = $state_obj.Hospitalized_Today - $state_obj.Hospitalized_Yesterday
    $state_obj.Dead_Today = $tmp_data.death
    $state_obj.Dead_Delta = $state_obj.Dead_Today - $state_obj.Dead_Yesterday
    Write-Host "Pulled $($state_obj.Query_Name)"
}
 
$country_objs = $country_objs | Sort-Object -Property "Infected_Today" -Descending
$state_objs = $state_objs | Sort-Object -Property "Infected_Today" -Descending
 
$world | ft
$country_objs | ft
$state_objs | ft
 
$today = Get-Date
$date_string = "$($today.Year)_$($today.Month)_$($today.Day)"
$time_string = "$($today.Year)_$($today.Month)_$($today.Day)_$($today.Hour)_$($today.Minute)"
 
If($(Test-Path -Path "./worldwide_$($date_string).xml") -eq $FALSE)
{
    Remove-Item -Path "./worldwide.xml" -Confirm:$false -Force
    Export-Clixml -Path "./worldwide.xml" -InputObject $world
    Export-Clixml -Path "./worldwide_$($date_string).xml" -InputObject $world
}
Export-Clixml -Path "./worldwide_$($time_string).xml" -InputObject $world
 
If($(Test-Path -Path "./country_$($date_string).xml") -eq $FALSE)
{
    Remove-Item -Path "./country.xml" -Confirm:$false -Force
    Export-Clixml -Path "./country.xml" -InputObject $country_objs
    Export-Clixml -Path "./country_$($date_string).xml" -InputObject $country_objs
}
Export-Clixml -Path "./country_$($time_string).xml" -InputObject $country_objs
 
If($(Test-Path -Path "./state_$($date_string).xml") -eq $FALSE)
{
    Remove-Item -Path "./state.xml" -Confirm:$false -Force
    Export-Clixml -Path "./state.xml" -InputObject $state_objs
    Export-Clixml -Path "./state_$($date_string).xml" -InputObject $state_objs
}
Export-Clixml -Path "./state_$($time_string).xml" -InputObject $state_objs
 
$world_unresolved_sign = "+"
If($world.Unresolved_Delta -lt 0)
{
    $world_unresolved_sign = ""
}
Write-Host "`n`n------------`n"
Write-Host "b{World Wide}b totals:"
Write-Host "y{Cases}y: $($world.Infected_Today.ToString('N0')) (+$($world.Infected_Delta.ToString('N0')))"
Write-Host "r{Deaths}r: $($world.Dead_Today.ToString('N0')) (+$($world.Dead_Delta.ToString('N0')))"
Write-Host "g{Recovered}g: $($world.Recovered_Today.ToString('N0')) (+$($world.Recovered_Delta.ToString('N0')))"
Write-Host "p[Unresolved]p: $($world.Unresolved_Today.ToString('N0')) ($($world_unresolved_sign)$($world.Unresolved_Delta.ToString('N0')))`n"
 
Write-Host "r{U}rb[S]bb{A}b totals:"
Write-Host "Cases: $($usa.Infected_Today.ToString('N0')) (+$($usa.Infected_Delta.ToString('N0')))"
Write-Host "Deaths: $($usa.Dead_Today.ToString('N0')) (+$($usa.Dead_Delta.ToString('N0')))`n"

Write-Host "`n------------`n"
Write-Host "e[Deep Stats]e"
Write-Host "`n/[Top US States:]/"
ForEach($state_obj in $state_objs)
{
    $state_hospitalized_sign = "+"
    If($state_obj.Hospitalized_Delta -lt 0)
    {
        $state_hospitalized_sign = ""
    }
    Write-Host "b[$($state_obj.Name)]b"
    Write-Host "Cases: $($state_obj.Infected_Today.ToString('N0')) (+$($state_obj.Infected_Delta.ToString('N0')))"
    Write-Host "Hospitalized: $($state_obj.Hospitalized_Today.ToString('N0')) ($($state_hospitalized_sign)$($state_obj.Hospitalized_Delta.ToString('N0')))"
    Write-Host "Deaths: $($state_obj.Dead_Today.ToString('N0')) (+$($state_obj.Dead_Delta.ToString('N0')))`n"
}
Write-Host "`n/[Top Countries:]/"
ForEach($country_obj in $country_objs)
{
    If($country_obj.Query_Name -ne "US")
    {
        Write-Host "b[$($country_obj.Name)]b"
        Write-Host "Cases: $($country_obj.Infected_Today.ToString('N0')) (+$($country_obj.Infected_Delta.ToString('N0')))"
        Write-Host "Deaths: $($country_obj.Dead_Today.ToString('N0')) (+$($country_obj.Dead_Delta.ToString('N0')))`n"
    }
}
 
Write-Host "`n/[Other Notable Areas]/"
Write-Host "b[Chatty]b"
Write-Host "Cases: 7 (+0)"
Write-Host "Deaths: 0 (+0)"
Write-Host "`nb[y{mojoald}y's apartment building]b"
Write-Host "Cases: 4 (+0)"
Write-Host "Deaths: 0 (+0)"
Write-Host "`n`n------------`n"