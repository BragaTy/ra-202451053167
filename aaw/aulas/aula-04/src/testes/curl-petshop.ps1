# ==========================================================================
# PetHouse API - os 12 endpoints a auditar (Aula 03.1)
# Rode com:  powershell -ExecutionPolicy Bypass -File .\curl-petshop.ps1
# Antes:     cd ..\PetShopApi ; dotnet run
#
# Este script NAO diz qual e o erro de cada endpoint - ele so faz as chamadas
# e mostra o que voce precisa observar. O diagnostico e seu.
#
# Notas de Windows: usamos curl.exe (nao o alias "curl" do PowerShell 5.1) e
# passamos o JSON por arquivo (-d "@arquivo.json"), que funciona em qualquer
# versao do PowerShell.
# ==========================================================================
param([string]$BaseUrl = "http://localhost:5302")

$corpos = Join-Path ([System.IO.Path]::GetTempPath()) "aula031-petshop"
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

$petNovo   = Corpo "pet-novo.json" '{ "nome": "Bidu", "especie": "cachorro", "raca": "SRD", "tutorId": 1, "pesoEmKg": 8.5 }'
$vacina    = Corpo "vacina.json"   '{ "nome": "V10" }'
$loginAna  = Corpo "login-ana.json"   '{ "usuario": "Ana", "senha": "123" }'
$loginBrun = Corpo "login-bruno.json" '{ "usuario": "Bruno", "senha": "123" }'

Titulo "0. A API esta no ar?"
curl.exe -s -o NUL -w "  GET /api/v1/pet/1 -> HTTP %{http_code}`n" "$BaseUrl/api/v1/pet/1"

Titulo "01 - POST /api/v1/getPets - observe a URI e o metodo"
curl.exe -s -o NUL -w "  POST -> HTTP %{http_code}`n" -X POST "$BaseUrl/api/v1/getPets"
curl.exe -s -o NUL -w "  o mesmo caminho com GET -> HTTP %{http_code}`n" -X GET "$BaseUrl/api/v1/getPets"

Titulo "02 - GET /api/v1/deletarPet?id=N - observe o que um GET faz"
# Cria um pet so para esta demonstracao (assim o script pode rodar varias vezes).
$cobaia = (curl.exe -s -X POST "$BaseUrl/api/v1/pets" -H "Content-Type: application/json" -d "@$petNovo" | ConvertFrom-Json).id
Write-Host "  pet de teste criado: id $cobaia"
curl.exe -s -o NUL -w "  antes:  GET /api/v1/pet/$cobaia -> HTTP %{http_code}`n" "$BaseUrl/api/v1/pet/$cobaia"
Write-Host "  chamando GET /api/v1/deletarPet?id=$cobaia ->"
curl.exe -s "$BaseUrl/api/v1/deletarPet?id=$cobaia"
Write-Host ""
curl.exe -s -o NUL -w "  depois: GET /api/v1/pet/$cobaia -> HTTP %{http_code}`n" "$BaseUrl/api/v1/pet/$cobaia"
Write-Host "  (esta URL funciona colada na barra do navegador. pense nisso.)"

Titulo "03 - GET /api/v1/pet/9 - compare esta rota com a do endpoint 06"
curl.exe -s -o NUL -w "  /api/v1/pet/9  -> HTTP %{http_code}`n" "$BaseUrl/api/v1/pet/9"
curl.exe -s -o NUL -w "  /api/v1/pets/9 -> HTTP %{http_code}`n" "$BaseUrl/api/v1/pets/9"

Titulo "04 - grafia das rotas"
curl.exe -s -o NUL -w "  /api/v1/banhosTosa   -> HTTP %{http_code}`n" "$BaseUrl/api/v1/banhosTosa?size=2"
curl.exe -s -o NUL -w "  /api/v1/banhostosa   -> HTTP %{http_code}`n" "$BaseUrl/api/v1/banhostosa?size=2"
curl.exe -s -o NUL -w "  /api/v1/tutores_vip  -> HTTP %{http_code}`n" "$BaseUrl/api/v1/tutores_vip?size=2"
curl.exe -s -o NUL -w "  /api/v1/tutores-vip  -> HTTP %{http_code}`n" "$BaseUrl/api/v1/tutores-vip?size=2"

Titulo "05 - POST /api/v1/pets - procure o status e o header Location"
curl.exe -s -D - -o NUL -X POST "$BaseUrl/api/v1/pets" -H "Content-Type: application/json" -d "@$petNovo" | Select-String -Pattern "^(HTTP|Location)"
Write-Host "  (existe alguma linha Location ai em cima?)"

Titulo "06 - GET /api/v1/pets/999999 - este pet nao existe"
curl.exe -s -i "$BaseUrl/api/v1/pets/999999" | Select-Object -First 8

Titulo "07 - GET /api/pets - compare a URI e os NOMES DOS CAMPOS"
Write-Host "  com versao no path (endpoint 01):"
$comVersao = curl.exe -s -X POST "$BaseUrl/api/v1/getPets"
Write-Host ("  " + $comVersao.Substring(0, [Math]::Min(150, $comVersao.Length)))
Write-Host "  sem versao no path:"
curl.exe -s "$BaseUrl/api/pets?size=1"
Write-Host ""

Titulo "08 - GET de um exame - quantos ids?"
curl.exe -s -o NUL -w "  ids reais    -> HTTP %{http_code}`n" "$BaseUrl/api/v1/petshops/1/clientes/5/pets/9/consultas/12/exames/6"
curl.exe -s -o NUL -w "  ids trocados -> HTTP %{http_code}`n" "$BaseUrl/api/v1/petshops/99/clientes/99/pets/99/consultas/12/exames/6"
Write-Host "  (os tres primeiros ids mudaram e a resposta e a mesma. eles servem para que?)"

Titulo "09 - GET /api/v1/consultas - meca a resposta"
curl.exe -s -o NUL -w "  %{size_download} bytes em %{time_total}s`n" "$BaseUrl/api/v1/consultas"
Write-Host "  compare com a API correta (se estiver rodando na 5301):"
curl.exe -s -o NUL -w "  %{size_download} bytes em %{time_total}s`n" "http://localhost:5301/api/v1/pedidos?page=1&size=20"

Titulo "10 - PUT /api/v1/pets/12/vacinas - tres vezes, corpo identico"
foreach ($i in 1..3) {
    $resposta = curl.exe -s -X PUT "$BaseUrl/api/v1/pets/12/vacinas" -H "Content-Type: application/json" -d "@$vacina"
    # ConvertFrom-Json devolve o array inteiro como UM objeto; o cast força a contagem certa.
    $quantidade = ([object[]]($resposta | ConvertFrom-Json)).Count
    Write-Host "  chamada $i -> o pet agora tem $quantidade vacina(s)"
}

Titulo "11 - sessao - dois logins seguidos"
Write-Host "  GET /meus-pets ANTES de qualquer login desta sessao:"
curl.exe -s -o NUL -w "  -> HTTP %{http_code}   (401 num servidor recem-iniciado)`n" "$BaseUrl/api/v1/meus-pets"
curl.exe -s -o NUL -X POST "$BaseUrl/api/v1/sessao" -H "Content-Type: application/json" -d "@$loginAna"
Write-Host "  login como Ana. /meus-pets responde:"
$respostaAna = curl.exe -s "$BaseUrl/api/v1/meus-pets"
Write-Host ("  " + $respostaAna.Substring(0, [Math]::Min(90, $respostaAna.Length)))
curl.exe -s -o NUL -X POST "$BaseUrl/api/v1/sessao" -H "Content-Type: application/json" -d "@$loginBrun"
Write-Host "  agora OUTRO usuario fez login. /meus-pets da Ana responde:"
$respostaDepois = curl.exe -s "$BaseUrl/api/v1/meus-pets"
Write-Host ("  " + $respostaDepois.Substring(0, [Math]::Min(90, $respostaDepois.Length)))
Write-Host "  (a Ana nao fez nada. por que a resposta dela mudou?)"

Titulo "12 - GET /api/v1/tabela-de-precos - leia os headers"
curl.exe -s -D - -o NUL "$BaseUrl/api/v1/tabela-de-precos" | Select-String -Pattern "^(HTTP|Cache-Control|ETag|Pragma)"
Write-Host "  (este dado e reajustado uma vez por ano.)"

Write-Host ""
Write-Host "=============================================================="
Write-Host "Agora preencha, para cada endpoint: o que observei / qual regra"
Write-Host "REST foi violada / como eu redesenharia."
Write-Host "Reinicie a API (Ctrl+C e dotnet run) para voltar ao estado inicial."
Write-Host "=============================================================="
