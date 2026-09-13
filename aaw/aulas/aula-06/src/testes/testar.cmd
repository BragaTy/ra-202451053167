@echo off
chcp 65001 >nul
title Aula 06 - Testes reais com cURL
echo Mantenha Pagamento, Notificacao e Loja rodando.
echo.
echo TESTE SINCRONO
curl.exe -i -X POST "http://localhost:5090/api/pedidos/sincrono" -H "Content-Type: application/json" --data-binary "@%~dp0pedido-aprovado.json"
echo.
pause
echo.
echo TESTE ASSINCRONO
curl.exe -i -X POST "http://localhost:5090/api/pedidos/assincrono" -H "Content-Type: application/json" --data-binary "@%~dp0pedido-aprovado.json"
echo.
pause
echo.
echo PAGAMENTO REJEITADO NO SINCRONO
curl.exe -i -X POST "http://localhost:5090/api/pedidos/sincrono" -H "Content-Type: application/json" --data-binary "@%~dp0pedido-rejeitado.json"
echo.
echo PAGAMENTO REJEITADO NO ASSINCRONO
curl.exe -i -X POST "http://localhost:5090/api/pedidos/assincrono" -H "Content-Type: application/json" --data-binary "@%~dp0pedido-rejeitado.json"
echo.
pause
