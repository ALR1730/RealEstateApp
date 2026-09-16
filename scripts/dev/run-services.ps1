param(
    [ValidateSet("start", "stop", "restart", "status")]
    [string]$Action = "start",
    [ValidateSet("api", "vite", "all")]
    [string]$Service = "all"
)

$root = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
$clientApp = Join-Path $root "src\Presentation\RealEstateApp.ClientApp"
$apiProject = Join-Path $root "src\Presentation\RealEstateApp.Presentation.WebApi"
$liveDir = Join-Path $env:TEMP "opencode\live"
New-Item -ItemType Directory -Force -Path $liveDir | Out-Null

$apiPidFile = Join-Path $liveDir "api.pid"
$vitePidFile = Join-Path $liveDir "vite.pid"
$apiLog = Join-Path $liveDir "api.log"
$viteLog = Join-Path $liveDir "vite.log"

function Test-PidAlive([string]$pidFile) {
    if (!(Test-Path $pidFile)) { return $false }
    $pidNum = 0
    try { $pidNum = [int](Get-Content $pidFile -Raw).Trim() } catch { return $false }
    return [bool](Get-Process -Id $pidNum -ErrorAction SilentlyContinue)
}

function Get-Ports {
    $apiBusy = (Test-NetConnection -ComputerName localhost -Port 5196 -WarningAction SilentlyContinue).TcpTestSucceeded
    $viteBusy = (Test-NetConnection -ComputerName localhost -Port 5173 -WarningAction SilentlyContinue).TcpTestSucceeded
    [pscustomobject]@{ Api5196 = $apiBusy; Vite5173 = $viteBusy }
}

function Start-Detached([string]$name, [string]$workDir, [string]$commandLine, [string]$pidFile, [string]$logFile) {
    if (Test-PidAlive $pidFile) {
        Write-Host "$name ya corriendo (pid $(Get-Content $pidFile -Raw -ErrorAction SilentlyContinue))"
        return
    }
    # cmd wrapper con redireccion de logs; Win32_Process.Create desvincula del job del shell (sobrevive)
    $redir = "> ""$logFile"" 2>&1"
    $cmd = "cmd.exe /c ""cd /d `"$workDir`" && $commandLine $redir"""
    $p = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{
        CommandLine = $cmd
        CurrentDirectory = $workDir
    }
    if ($p.ReturnValue -eq 0 -and $p.ProcessId) {
        Set-Content -Path $pidFile -Value $p.ProcessId
        Write-Host "$name iniciado (pid $($p.ProcessId))"
    } else {
        Write-Host "ERROR: no se pudo iniciar $name (return $($p.ReturnValue))" -ForegroundColor Red
    }
}

function Stop-Named([string]$name, [string]$match) {
    Get-CimInstance Win32_Process -Filter "Name = '$name'" -ErrorAction SilentlyContinue |
        Where-Object { $_.CommandLine -match $match } |
        ForEach-Object { Write-Host "  stop: $($_.Name) $($_.CommandLine.Substring(0, [Math]::Min(90, $_.CommandLine.Length)))"; Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
}

function Stop-All {
    # Mata a los listeners reales de ambos puertos
    foreach ($port in 5196, 5173) {
        Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue |
            ForEach-Object {
                Write-Host "  stop listener port $port pid $($_.OwningProcess)"
                Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
            }
    }
    Start-Sleep -Milliseconds 800
    # Procesos wrapper (dotnet host / vite) que hayan sobrevivido
    Stop-Named "dotnet" "RealEstateApp.Presentation.WebApi"
    Stop-Named "RealEstateApp.Presentation.WebApi.exe" "bin"
    Stop-Named "node.exe" "ClientApp"
    Remove-Item $apiPidFile, $vitePidFile -Force -ErrorAction SilentlyContinue
    Write-Host "Servicios detenidos."
}

switch ($Action) {
    "start" {
        if ($Service -in @("all", "api")) {
            Start-Detached "API" $root "dotnet run --no-build --project `"$apiProject`"" $apiPidFile $apiLog
        }
        if ($Service -in @("all", "vite")) {
            Start-Detached "VITE" $clientApp "npm run dev -- --port 5173 --strictPort" $vitePidFile $viteLog
        }
        Write-Host "Esperando health checks..."
        $okApi = $false; $okVite = $false
        for ($i = 0; $i -lt 60; $i++) {
            Start-Sleep -Seconds 2
            try {
                $r = Invoke-WebRequest -Uri "http://localhost:5196/api/v1/subscriptions/plans" -UseBasicParsing -TimeoutSec 3
                if ($r.StatusCode -eq 200) { $okApi = $true }
            } catch {}
            try {
                $r = Invoke-WebRequest -Uri "http://localhost:5173" -UseBasicParsing -TimeoutSec 3
                if ($r.StatusCode -eq 200) { $okVite = $true }
            } catch {}
            if ($okApi -and $okVite) { break }
        }
        "API :5196 -> $okApi | VITE :5173 -> $okVite"
    }
    "stop" { Stop-All }
    "restart" { Stop-All; Start-Sleep -Seconds 2; & $PSCommandPath -Action start -Service $Service }
    "status" {
        $ports = Get-Ports
        "API pid alive: $(Test-PidAlive $apiPidFile) | port 5196: $($ports.Api5196)"
        "VITE pid alive: $(Test-PidAlive $vitePidFile) | port 5173: $($ports.Vite5173)"
        if (Test-Path $apiLog) { "--- api.log tail ---"; Get-Content $apiLog -Tail 4 }
        if (Test-Path $viteLog) { "--- vite.log tail ---"; Get-Content $viteLog -Tail 4 }
    }
}