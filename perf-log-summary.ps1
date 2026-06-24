param(
    [string]$LogPath = "Web\App_Data\perf-web.log",
    [int]$TopRequests = 10,
    [int]$TopDb = 15
)

if (-not (Test-Path -LiteralPath $LogPath)) {
    Write-Host "No se encontro el log: $LogPath"
    exit 1
}

$lines = Get-Content -LiteralPath $LogPath

$requests = New-Object System.Collections.Generic.List[object]
$dbItems = New-Object System.Collections.Generic.List[object]
$currentRequest = $null

foreach ($line in $lines) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    if ($line -match '^\s*\d{4}-\d{2}-\d{2} .* \| WEB \| (?<screen>[^|]+) \| (?<method>[^|]+) \| (?<url>[^|]+) \| status=(?<status>\d+) \| total=(?<total>\d+) ms \| db=(?<db>\d+) ms/(?<calls>\d+) calls \| reqId=(?<reqid>\S+)\s*$') {
        $currentRequest = [pscustomobject]@{
            Screen   = $matches['screen'].Trim()
            Method   = $matches['method'].Trim()
            Url      = $matches['url'].Trim()
            Status   = [int]$matches['status']
            TotalMs  = [int]$matches['total']
            DbMs     = [int]$matches['db']
            DbCalls  = [int]$matches['calls']
            RequestId = $matches['reqid'].Trim()
        }
        $requests.Add($currentRequest) | Out-Null
        continue
    }

    if ($line -match '^\s+DB -> (?<caller>[^|]+) \| (?<cmd>[^|]+) \| (?<ms>\d+) ms \| (?<op>.+)$') {
        $dbItems.Add([pscustomobject]@{
            RequestId = if ($currentRequest) { $currentRequest.RequestId } else { "" }
            Screen    = if ($currentRequest) { $currentRequest.Screen } else { "" }
            Caller    = $matches['caller'].Trim()
            Command   = $matches['cmd'].Trim()
            Ms        = [int]$matches['ms']
            Operation = $matches['op'].Trim()
        }) | Out-Null
    }
}

Write-Host ""
Write-Host "Resumen general"
Write-Host "---------------"
Write-Host ("Requests analizadas : {0}" -f $requests.Count)
Write-Host ("Operaciones DB      : {0}" -f $dbItems.Count)

if ($requests.Count -eq 0) {
    Write-Host ""
    Write-Host "No se encontraron entradas WEB en el log."
    exit 0
}

Write-Host ""
Write-Host "Top pantallas por tiempo total"
Write-Host "------------------------------"
$requests |
    Sort-Object TotalMs -Descending |
    Select-Object -First $TopRequests Screen, Method, Status, TotalMs, DbMs, DbCalls, Url |
    Format-Table -AutoSize

Write-Host ""
Write-Host "Pantallas agrupadas"
Write-Host "-------------------"
$requests |
    Group-Object Screen |
    ForEach-Object {
        $items = $_.Group
        [pscustomobject]@{
            Screen      = $_.Name
            Cantidad    = $items.Count
            PromedioMs  = [math]::Round(($items | Measure-Object TotalMs -Average).Average, 2)
            MaximoMs    = ($items | Measure-Object TotalMs -Maximum).Maximum
            PromedioDb  = [math]::Round(($items | Measure-Object DbMs -Average).Average, 2)
            MaxDbCalls  = ($items | Measure-Object DbCalls -Maximum).Maximum
        }
    } |
    Sort-Object PromedioMs -Descending |
    Format-Table -AutoSize

if ($dbItems.Count -gt 0) {
    Write-Host ""
    Write-Host "Top operaciones DB"
    Write-Host "------------------"
    $dbItems |
        Sort-Object Ms -Descending |
        Select-Object -First $TopDb Screen, Caller, Command, Ms, Operation |
        Format-Table -AutoSize

    Write-Host ""
    Write-Host "Metodos DB agrupados"
    Write-Host "--------------------"
    $dbItems |
        Group-Object Caller |
        ForEach-Object {
            $items = $_.Group
            [pscustomobject]@{
                Caller      = $_.Name
                Cantidad    = $items.Count
                PromedioMs  = [math]::Round(($items | Measure-Object Ms -Average).Average, 2)
                MaximoMs    = ($items | Measure-Object Ms -Maximum).Maximum
                TotalMs     = ($items | Measure-Object Ms -Sum).Sum
            }
        } |
        Sort-Object TotalMs -Descending |
        Format-Table -AutoSize
}

Write-Host ""
Write-Host "Uso:"
Write-Host "  .\perf-log-summary.ps1"
Write-Host "  .\perf-log-summary.ps1 -TopRequests 20 -TopDb 30"
