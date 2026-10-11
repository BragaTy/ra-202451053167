@echo off
chcp 65001 >nul
title Aula 08 - Testes do ciclo com cURL (sem Postman)
echo ====================================================================
echo  AULA 08 - TESTES COM CURL (Substitui o Postman)
echo  Certifique-se de que a API esta rodando (cd src ^&^& dotnet run)
echo ====================================================================
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$base = 'http://localhost:5290';" ^
  "Write-Host '1. POST /api/livros sem token' -ForegroundColor Yellow;" ^
  "$r1 = curl.exe -s -o /dev/null -w '%%{http_code}' -X POST \"$base/api/livros\" -H 'Content-Type: application/json' -d '{\"titulo\":\"Livro Teste\",\"autor\":\"Autor\",\"ano\":2026,\"disponivel\":true}';" ^
  "Write-Host \"   Status: $r1 (Esperado: 401)`n\" -ForegroundColor Green;" ^
  "Write-Host '2. Login como Leitor' -ForegroundColor Yellow;" ^
  "$loginLeitor = curl.exe -s -X POST \"$base/api/auth/login\" -H 'Content-Type: application/json' -d '{\"email\":\"leitor@biblioteca.com\",\"senha\":\"leitor123\"}' | ConvertFrom-Json;" ^
  "$tokenLeitor = $loginLeitor.token;" ^
  "Write-Host \"   Status: 200 (Token emitido para $($loginLeitor.nome), Role: $($loginLeitor.role))`n\" -ForegroundColor Green;" ^
  "Write-Host '3. POST /api/livros com token de Leitor' -ForegroundColor Yellow;" ^
  "$r3 = curl.exe -s -w '%%{http_code}' -X POST \"$base/api/livros\" -H 'Content-Type: application/json' -H \"Authorization: Bearer $tokenLeitor\" -d '{\"titulo\":\"Clean Code\",\"autor\":\"Robert Martin\",\"ano\":2008,\"disponivel\":true}';" ^
  "$status3 = $r3.Substring($r3.Length - 3);" ^
  "Write-Host \"   Status: $status3 (Esperado: 201 - Livro criado)`n\" -ForegroundColor Green;" ^
  "Write-Host '4. DELETE /api/livros/1 com token de Leitor' -ForegroundColor Yellow;" ^
  "$r4 = curl.exe -s -o /dev/null -w '%%{http_code}' -X DELETE \"$base/api/livros/1\" -H \"Authorization: Bearer $tokenLeitor\";" ^
  "Write-Host \"   Status: $r4 (Esperado: 403 - Leitor nao pode deletar)`n\" -ForegroundColor Green;" ^
  "Write-Host '5. Login como Bibliotecario' -ForegroundColor Yellow;" ^
  "$loginBiblio = curl.exe -s -X POST \"$base/api/auth/login\" -H 'Content-Type: application/json' -d '{\"email\":\"biblio@biblioteca.com\",\"senha\":\"biblio123\"}' | ConvertFrom-Json;" ^
  "$tokenBiblio = $loginBiblio.token;" ^
  "Write-Host \"   Status: 200 (Token emitido para $($loginBiblio.nome), Role: $($loginBiblio.role))`n\" -ForegroundColor Green;" ^
  "Write-Host '6. DELETE /api/livros/1 com token de Bibliotecario' -ForegroundColor Yellow;" ^
  "$r6 = curl.exe -s -o /dev/null -w '%%{http_code}' -X DELETE \"$base/api/livros/1\" -H \"Authorization: Bearer $tokenBiblio\";" ^
  "Write-Host \"   Status: $r6 (Esperado: 204 - Deletado com sucesso)`n\" -ForegroundColor Green;" ^
  "Write-Host '7. POST com token expirado' -ForegroundColor Yellow;" ^
  "$loginExp = curl.exe -s -X POST \"$base/api/auth/login-expirado\" -H 'Content-Type: application/json' -d '{\"email\":\"leitor@biblioteca.com\",\"senha\":\"leitor123\"}' | ConvertFrom-Json;" ^
  "$tokenExp = $loginExp.token;" ^
  "$r7 = curl.exe -s -o /dev/null -w '%%{http_code}' -X POST \"$base/api/livros\" -H 'Content-Type: application/json' -H \"Authorization: Bearer $tokenExp\" -d '{\"titulo\":\"Livro Expirado\",\"autor\":\"Autor\",\"ano\":2026,\"disponivel\":true}';" ^
  "Write-Host \"   Status: $r7 (Esperado: 401 - Token expirado)`n\" -ForegroundColor Green;"

echo.
pause
