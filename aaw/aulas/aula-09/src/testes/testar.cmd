@echo off
chcp 65001 >nul
title Aula 09 - 18 Testes de Autorizacao (RBAC, ABAC e PBAC)
echo ====================================================================
echo  AULA 09 - TESTES DE AUTORIZACAO (Substitui o Run Collection do Postman)
echo  Certifique-se de que a API esta rodando (cd src ^&^& dotnet run)
echo ====================================================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$base = 'http://localhost:5291';" ^
  "function Test-Endpoint($nome, $method, $path, $token, $body, $esperado) {" ^
  "  $headers = @('-s', '-o', 'NUL', '-w', '%%{http_code}', '-X', $method, \"$base$path\", '-H', 'Content-Type: application/json');" ^
  "  if ($token) { $headers += @('-H', \"Authorization: Bearer $token\") }" ^
  "  if ($body) { $headers += @('-d', $body) }" ^
  "  $status = & curl.exe @headers;" ^
  "  if ($status -eq $esperado) { Write-Host (\"{0,-45} -> {1} [PASSOU]\" -f $nome, $status) -ForegroundColor Green }" ^
  "  else { Write-Host (\"{0,-45} -> {1} (Esperado: {2}) [FALHOU]\" -f $nome, $status, $esperado) -ForegroundColor Red }" ^
  "}" ^
  "$ana = (curl.exe -s -X POST \"$base/api/auth/login\" -H 'Content-Type: application/json' -d '{\"email\":\"ana@biblioteca.com\",\"senha\":\"ana123\"}' | ConvertFrom-Json).token;" ^
  "$beto = (curl.exe -s -X POST \"$base/api/auth/login\" -H 'Content-Type: application/json' -d '{\"email\":\"biblio@biblioteca.com\",\"senha\":\"biblio123\"}' | ConvertFrom-Json).token;" ^
  "$carla = (curl.exe -s -X POST \"$base/api/auth/login\" -H 'Content-Type: application/json' -d '{\"email\":\"carla@biblioteca.com\",\"senha\":\"carla123\"}' | ConvertFrom-Json).token;" ^
  "$dora = (curl.exe -s -X POST \"$base/api/auth/login\" -H 'Content-Type: application/json' -d '{\"email\":\"admin@biblioteca.com\",\"senha\":\"admin123\"}' | ConvertFrom-Json).token;" ^
  "$livro = '{\"titulo\":\"Livro Teste\",\"autor\":\"Autor\",\"ano\":2026,\"categoria\":\"Comum\",\"disponivel\":true}';" ^
  "Test-Endpoint '01 Ana (Leitor) POST /api/livros' 'POST' '/api/livros' $ana $livro '403';" ^
  "Test-Endpoint '02 Beto (Biblio) POST /api/livros' 'POST' '/api/livros' $beto $livro '201';" ^
  "Test-Endpoint '03 Ana (Leitor) PUT /api/livros/1' 'PUT' '/api/livros/1' $ana $livro '403';" ^
  "Test-Endpoint '04 Beto DELETE /api/livros/3' 'DELETE' '/api/livros/3' $beto $null '403';" ^
  "Test-Endpoint '05 Dora (Admin) DELETE /api/livros/3' 'DELETE' '/api/livros/3' $dora $null '204';" ^
  "Test-Endpoint '06 GET /api/relatorios/leitores sem token' 'GET' '/api/relatorios/leitores' $null $null '401';" ^
  "Test-Endpoint '07 GET /api/relatorios/emprestimos sem token' 'GET' '/api/relatorios/emprestimos' $null $null '401';" ^
  "Test-Endpoint '08 Ana GET /api/relatorios/leitores' 'GET' '/api/relatorios/leitores' $ana $null '403';" ^
  "Test-Endpoint '09 Dora GET /api/relatorios/leitores' 'GET' '/api/relatorios/leitores' $dora $null '200';" ^
  "Test-Endpoint '10 Ana renova 101 (dela)' 'POST' '/api/emprestimos/101/renovar' $ana $null '200';" ^
  "Test-Endpoint '11 Ana renova 102 (do Bruno)' 'POST' '/api/emprestimos/102/renovar' $ana $null '403';" ^
  "Test-Endpoint '12 Beto renova 102' 'POST' '/api/emprestimos/102/renovar' $beto $null '200';" ^
  "Test-Endpoint '13 Ana empresta livro 1 (comum)' 'POST' '/api/emprestimos' $ana '{\"livroId\":1}' '201';" ^
  "Test-Endpoint '14 Ana empresta livro 4 (obra rara)' 'POST' '/api/emprestimos' $ana '{\"livroId\":4}' '403';" ^
  "Test-Endpoint '15 Carla (Pesquisador) empresta livro 4' 'POST' '/api/emprestimos' $carla '{\"livroId\":4}' '201';" ^
  "Test-Endpoint '16 Dora (Admin) empresta livro 1' 'POST' '/api/emprestimos' $dora '{\"livroId\":1}' '403';" ^
  "Test-Endpoint '17 GET /api/livros sem token' 'GET' '/api/livros' $null $null '200';" ^
  "Test-Endpoint '18 POST /api/emprestimos sem token' 'POST' '/api/emprestimos' $null '{\"livroId\":1}' '401';"

echo.
pause
