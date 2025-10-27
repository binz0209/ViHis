# Test Embeddings API Script

$baseUrl = "https://vihisprj-g2gyaehmasbahnff.malaysiawest-01.azurewebsites.net"

Write-Host "🔍 Checking embedding status..." -ForegroundColor Cyan

# Check status
$status = Invoke-RestMethod -Uri "$baseUrl/api/v1/admin/embeddings/status" -Method Get
Write-Host "Total chunks: $($status.total)" -ForegroundColor Yellow
Write-Host "With embeddings: $($status.withEmbedding)" -ForegroundColor Green
Write-Host "Without embeddings: $($status.withoutEmbedding)" -ForegroundColor Red
Write-Host "Percentage: $($status.percentage)%" -ForegroundColor Cyan

if ($status.withoutEmbedding -gt 0) {
    Write-Host "`n⚠️  Some chunks missing embeddings" -ForegroundColor Yellow
    Write-Host "Generate embeddings? (Y/N)" -ForegroundColor Yellow
    $answer = Read-Host
    
    if ($answer -eq "Y" -or $answer -eq "y") {
        Write-Host "`n📝 Enter limit (leave empty for ALL):" -ForegroundColor Cyan
        $limit = Read-Host
        
        $url = "$baseUrl/api/v1/admin/embeddings/generate-all"
        if ($limit) {
            $url += "?limit=$limit"
        }
        
        Write-Host "🚀 Generating embeddings..." -ForegroundColor Green
        Invoke-RestMethod -Uri $url -Method Post | ConvertTo-Json -Depth 3
    }
}

Write-Host "`n✅ Done!" -ForegroundColor Green




