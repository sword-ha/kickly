$proc = Start-Process -FilePath "dotnet" -ArgumentList "run --project src/SportsBooking.API/SportsBooking.API.csproj" -RedirectStandardOutput "out.log" -RedirectStandardError "err.log" -NoNewWindow -PassThru
Start-Sleep -Seconds 15
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
try { Invoke-RestMethod -Uri "https://localhost:55814/swagger/v1/swagger.json" > $null } catch {}
Stop-Process -Id $proc.Id -Force
Get-Content err.log
Get-Content out.log
