$ErrorActionPreference = "Stop"

$users = @(
    @{ Role = "Admin"; Email = "admin@realestate.com"; Pass = "Admin123!" },
    @{ Role = "Agent"; Email = "agent@realestate.com"; Pass = "Agent123!" },
    @{ Role = "Client"; Email = "client@realestate.com"; Pass = "Client123!" },
    @{ Role = "Developer"; Email = "developer@realestate.com"; Pass = "Developer123!" }
)

Write-Host "--- VALIDANDO AUTENTICACION DE USUARIOS ---" -ForegroundColor Cyan
foreach ($u in $users) {
    try {
        $body = @{ email = $u.Email; password = $u.Pass } | ConvertTo-Json
        $res = Invoke-RestMethod -Uri "http://localhost:5196/api/v1/Account/authenticate" -Method Post -Body $body -ContentType "application/json"
        Write-Host "[$($u.Role)] OK -> User: $($res.userName) | Email: $($res.email) | Roles: $($res.roles -join ', ') | Verified: $($res.isVerified)" -ForegroundColor Green
    } catch {
        Write-Host "[$($u.Role)] ERROR -> $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n--- VALIDANDO DATOS DE PRUEBA EN BD ---" -ForegroundColor Cyan
try {
    $props = Invoke-RestMethod -Uri "http://localhost:5196/api/v1/Properties" -Method Get
    Write-Host "Propiedades Cargadas: $($props.Count)" -ForegroundColor Green
    foreach ($p in $props) {
        Write-Host " - [$($p.code)] $($p.propertyType) en $($p.location) ($($p.saleType)) - RD$ $($p.price)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "Error al obtener propiedades: $($_.Exception.Message)" -ForegroundColor Red
}
