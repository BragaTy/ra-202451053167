# ==========================================================================
# Cafe Newton API - roteiro de verificacao em cURL (Aula 03.1)
# Rode com:  powershell -ExecutionPolicy Bypass -File .\curl-cafeteria.ps1
# Antes:     cd ..\CafeteriaApi ; dotnet run
#
# DUAS ARMADILHAS DO WINDOWS, resolvidas aqui:
#
# 1) "curl" no Windows PowerShell 5.1 e um ALIAS para Invoke-WebRequest, que
#    nao entende -X, -H nem -d. Por isso chamamos sempre curl.exe.
#
# 2) Passar JSON inline (-d '{"nome":"Cafe da Casa"}') e traicoeiro: o
#    PowerShell 5.1 come as aspas duplas e quebra o argumento nos espacos.
#    A forma que funciona em TODAS as versoes e gravar o corpo num arquivo e
#    usar -d "@arquivo.json" - que e, aliás, como se faz em scripts de verdade.
# ==========================================================================
param([string]$BaseUrl = "http://localhost:5301")

$corpos = Join-Path ([System.IO.Path]::GetTempPath()) "aula031-cafeteria"
New-Item -ItemType Directory -Path $corpos -Force | Out-Null

function Corpo($nome, $json) {
    $caminho = Join-Path $corpos $nome
    Set-Content -Path $caminho -Value $json -Encoding UTF8
    return $caminho
}

function Titulo($texto) {
    Write-Host ""
    Write-Host "=============================================================="
    Write-Host $texto
    Write-Host "=============================================================="
}

$cafeNovo     = Corpo "cafe-novo.json"     '{ "nome": "Espresso da Aula", "origem": "Sul de Minas", "torra": "media", "preco": 9.5, "disponivel": true }'
$cafeInvalido = Corpo "cafe-invalido.json" '{ "origem": "Sul de Minas", "torra": "media", "preco": 9.5 }'
$cafeCompleto = Corpo "cafe-put.json"      '{ "nome": "Coado Reserva", "origem": "Cerrado Mineiro", "torra": "clara", "preco": 11.0, "disponivel": true }'
$cafePatch    = Corpo "cafe-patch.json"    '{ "preco": 13.75 }'
$pedidoNovo   = Corpo "pedido-novo.json"   '{ "cliente": "Turma de ADS" }'
$itemNovo     = Corpo "item-novo.json"     '{ "cafeId": 5, "quantidade": 2 }'
$statusNovo   = Corpo "status.json"        '{ "status": "entregue" }'

Titulo "0. A API esta no ar?"
curl.exe -s -o NUL -w "  GET /api/v1/cafes -> HTTP %{http_code}`n" "$BaseUrl/api/v1/cafes?size=1"

Titulo "1. 200 - colecao paginada (envelope page/size/total/items)"
curl.exe -s "$BaseUrl/api/v1/cafes?page=1&size=3"
Write-Host ""

Titulo "2. 200 - filtro e ordenacao como query string do recurso"
curl.exe -s "$BaseUrl/api/v1/cafes?torra=escura&sort=-preco&size=3"
Write-Host ""

Titulo "3. 200 - um cafe, com ETag e Cache-Control"
curl.exe -s -D - -o NUL "$BaseUrl/api/v1/cafes/1" | Select-String -Pattern "^(HTTP|ETag|Cache-Control)"

Titulo "4. 304 - Not Modified (o cliente ja tem esta versao)"
# O ETag vem entre aspas ("cafe-1-v1") e o PowerShell come essas aspas ao
# montar um -H na mao. O cURL resolve isso sozinho: --etag-save grava o ETag
# num arquivo e --etag-compare o reenvia como If-None-Match.
$arquivoEtag = Join-Path $corpos "etag.txt"
curl.exe -s -o NUL --etag-save $arquivoEtag "$BaseUrl/api/v1/cafes/1"
Write-Host ("  ETag recebido: " + (Get-Content $arquivoEtag -Raw).Trim())
curl.exe -s -o NUL -w "  reenviando em If-None-Match -> HTTP %{http_code} (sem corpo)`n" --etag-compare $arquivoEtag "$BaseUrl/api/v1/cafes/1"

Titulo "5. 404 - recurso inexistente, em application/problem+json"
curl.exe -s -i "$BaseUrl/api/v1/cafes/99999" | Select-Object -First 12

Titulo "6. 201 + Location - criacao"
$cabecalhos = curl.exe -s -D - -o NUL -X POST "$BaseUrl/api/v1/cafes" -H "Content-Type: application/json" -d "@$cafeNovo"
$cabecalhos | Select-String -Pattern "^(HTTP|Location)"
# O cliente NAO precisa adivinhar o id: ele vem no header Location.
$criado = (($cabecalhos | Select-String -Pattern "^Location:") -split "/")[-1].Trim()
Write-Host "  id extraido do Location: $criado"

Titulo "7. 400 - corpo invalido (falta o campo nome)"
curl.exe -s -X POST "$BaseUrl/api/v1/cafes" -H "Content-Type: application/json" -d "@$cafeInvalido"
Write-Host ""

Titulo "8. 400 - o servidor define o teto de size (max. 100)"
curl.exe -s -o NUL -w "  GET /api/v1/cafes?size=100000 -> HTTP %{http_code}`n" "$BaseUrl/api/v1/cafes?size=100000"

Titulo "9. PUT e idempotente - tres chamadas identicas, mesmo estado"
foreach ($i in 1..3) {
    $codigo = curl.exe -s -o NUL -w "%{http_code}" -X PUT "$BaseUrl/api/v1/cafes/2" -H "Content-Type: application/json" -d "@$cafeCompleto"
    Write-Host "  chamada $i -> HTTP $codigo"
}
Write-Host "  estado final (repare que 'versao' nao subiu tres vezes):"
curl.exe -s "$BaseUrl/api/v1/cafes/2"
Write-Host ""

Titulo "10. 200 - PATCH altera so o campo enviado"
curl.exe -s -X PATCH "$BaseUrl/api/v1/cafes/2" -H "Content-Type: application/json" -d "@$cafePatch"
Write-Host ""

Titulo "11. 204 e depois 404 - DELETE (no cafe criado no passo 6)"
curl.exe -s -o NUL -w "  1a chamada -> HTTP %{http_code}   (204 No Content)`n" -X DELETE "$BaseUrl/api/v1/cafes/$criado"
curl.exe -s -o NUL -w "  2a chamada -> HTTP %{http_code}   (o recurso nao existe mais)`n" -X DELETE "$BaseUrl/api/v1/cafes/$criado"

Titulo "12. Pedidos e itens - aninhamento de 1 nivel"
$pedidoId = (curl.exe -s -X POST "$BaseUrl/api/v1/pedidos" -H "Content-Type: application/json" -d "@$pedidoNovo" | ConvertFrom-Json).id
Write-Host "  pedido criado: $pedidoId"
curl.exe -s -D - -o NUL -X POST "$BaseUrl/api/v1/pedidos/$pedidoId/itens" -H "Content-Type: application/json" -d "@$itemNovo" | Select-String -Pattern "^(HTTP|Location)"
Write-Host "  itens do pedido:"
curl.exe -s "$BaseUrl/api/v1/pedidos/$pedidoId/itens"
Write-Host ""
Write-Host "  pedido com os totais recalculados:"
curl.exe -s "$BaseUrl/api/v1/pedidos/$pedidoId"
Write-Host ""
curl.exe -s -o NUL -w "  PATCH status=entregue -> HTTP %{http_code}`n" -X PATCH "$BaseUrl/api/v1/pedidos/$pedidoId" -H "Content-Type: application/json" -d "@$statusNovo"

Write-Host ""
Write-Host "Fim. Reinicie a API (Ctrl+C e dotnet run) para voltar ao estado inicial."
Write-Host "Os corpos JSON usados ficaram em: $corpos"
