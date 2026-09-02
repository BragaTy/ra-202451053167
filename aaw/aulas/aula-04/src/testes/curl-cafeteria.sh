#!/usr/bin/env bash
# ==========================================================================
# Café Newton API — roteiro de verificação em cURL (Aula 03.1)
# Rode com:  bash curl-cafeteria.sh      (Git Bash, WSL, macOS ou Linux)
# Antes:     cd ../CafeteriaApi && dotnet run
# ==========================================================================
set -u
BASE="${1:-http://localhost:5301}"

titulo() { echo; echo "=============================================================="; echo "$1"; echo "=============================================================="; }

titulo "0. A API está no ar?"
curl -s -o /dev/null -w "  GET /api/v1/cafes -> HTTP %{http_code}\n" "$BASE/api/v1/cafes?size=1" \
  || { echo "  A API nao respondeu. Rode 'dotnet run' em src/CafeteriaApi."; exit 1; }

titulo "1. 200 — coleção paginada (envelope page/size/total/items)"
curl -s "$BASE/api/v1/cafes?page=1&size=3"; echo

titulo "2. 200 — filtro e ordenação como query string do recurso"
curl -s "$BASE/api/v1/cafes?torra=escura&sort=-preco&size=3"; echo

titulo "3. 200 — um café, com ETag e Cache-Control"
curl -s -D - -o /dev/null "$BASE/api/v1/cafes/1" | grep -Ei "^(HTTP|ETag|Cache-Control)"

titulo "4. 304 — Not Modified (o cliente já tem esta versão)"
ETAG=$(curl -s -D - -o /dev/null "$BASE/api/v1/cafes/1" | grep -i '^etag' | tr -d '\r' | cut -d' ' -f2)
echo "  ETag recebido: $ETAG"
curl -s -o /dev/null -w "  reenviando em If-None-Match -> HTTP %{http_code} (sem corpo)\n" \
     -H "If-None-Match: $ETAG" "$BASE/api/v1/cafes/1"

titulo "5. 404 — recurso inexistente, em application/problem+json"
curl -s -i "$BASE/api/v1/cafes/99999" | head -12

titulo "6. 201 + Location — criação"
CABECALHOS=$(curl -s -D - -o /dev/null -X POST "$BASE/api/v1/cafes" \
     -H "Content-Type: application/json" \
     -d '{"nome":"Espresso da Aula","origem":"Sul de Minas","torra":"media","preco":9.5,"disponivel":true}')
echo "$CABECALHOS" | grep -Ei "^(HTTP|Location)"
# O cliente NÃO precisa adivinhar o id: ele vem no header Location.
CRIADO=$(echo "$CABECALHOS" | grep -i '^location' | tr -d '\r' | sed -E 's#.*/##')
echo "  id extraído do Location: $CRIADO"

titulo "7. 400 — corpo inválido (falta o campo nome)"
curl -s -X POST "$BASE/api/v1/cafes" \
     -H "Content-Type: application/json" \
     -d '{"origem":"Sul de Minas","torra":"media","preco":9.5}'; echo

titulo "8. 400 — o servidor define o teto de size (máx. 100)"
curl -s -o /dev/null -w "  GET /api/v1/cafes?size=100000 -> HTTP %{http_code}\n" "$BASE/api/v1/cafes?size=100000"

titulo "9. PUT é idempotente — duas chamadas idênticas, mesmo estado"
for i in 1 2; do
  curl -s -o /dev/null -w "  chamada $i -> HTTP %{http_code}\n" -X PUT "$BASE/api/v1/cafes/2" \
       -H "Content-Type: application/json" \
       -d '{"nome":"Coado Reserva","origem":"Cerrado Mineiro","torra":"clara","preco":11.0,"disponivel":true}'
done
curl -s "$BASE/api/v1/cafes/2"; echo

titulo "10. 204 e depois 404 — DELETE (no café criado no passo 6)"
curl -s -o /dev/null -w "  1a chamada -> HTTP %{http_code}   (204 No Content)\n" -X DELETE "$BASE/api/v1/cafes/$CRIADO"
curl -s -o /dev/null -w "  2a chamada -> HTTP %{http_code}   (o recurso nao existe mais)\n" -X DELETE "$BASE/api/v1/cafes/$CRIADO"

titulo "11. Pedidos e itens — aninhamento de 1 nível"
PEDIDO=$(curl -s -X POST "$BASE/api/v1/pedidos" -H "Content-Type: application/json" \
         -d '{"cliente":"Turma de ADS"}' | sed -E 's/.*"id":([0-9]+).*/\1/')
echo "  pedido criado: $PEDIDO"
curl -s -D - -o /dev/null -X POST "$BASE/api/v1/pedidos/$PEDIDO/itens" \
     -H "Content-Type: application/json" -d '{"cafeId":5,"quantidade":2}' | grep -Ei "^(HTTP|Location)"
echo "  itens do pedido:"; curl -s "$BASE/api/v1/pedidos/$PEDIDO/itens"; echo
echo "  pedido com os totais recalculados:"; curl -s "$BASE/api/v1/pedidos/$PEDIDO"; echo
curl -s -o /dev/null -w "  PATCH status=entregue -> HTTP %{http_code}\n" -X PATCH "$BASE/api/v1/pedidos/$PEDIDO" \
     -H "Content-Type: application/json" -d '{"status":"entregue"}'

echo
echo "Fim. Reinicie a API (Ctrl+C e dotnet run) para voltar ao estado inicial."
