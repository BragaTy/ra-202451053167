#!/usr/bin/env bash
# ==========================================================================
# PetHouse API — os 12 endpoints a auditar (Aula 03.1)
# Rode com:  bash curl-petshop.sh        (Git Bash, WSL, macOS ou Linux)
# Antes:     cd ../PetShopApi && dotnet run
#
# Este script NÃO diz qual é o erro de cada endpoint — ele só faz as chamadas
# e mostra o que você precisa observar. O diagnóstico é seu.
# ==========================================================================
set -u
BASE="${1:-http://localhost:5302}"

titulo() { echo; echo "=============================================================="; echo "$1"; echo "=============================================================="; }

titulo "0. A API está no ar?"
curl -s -o /dev/null -w "  GET /api/v1/pet/1 -> HTTP %{http_code}\n" "$BASE/api/v1/pet/1" \
  || { echo "  A API nao respondeu. Rode 'dotnet run' em src/PetShopApi."; exit 1; }

titulo "01 · POST /api/v1/getPets — observe a URI e o método"
curl -s -o /dev/null -w "  POST -> HTTP %{http_code}\n" -X POST "$BASE/api/v1/getPets"
curl -s -o /dev/null -w "  o mesmo caminho com GET -> HTTP %{http_code}\n" -X GET "$BASE/api/v1/getPets"

titulo "02 · GET /api/v1/deletarPet?id=N — observe o que um GET faz"
# Cria um pet só para esta demonstração (assim o script pode rodar várias vezes).
COBAIA=$(curl -s -X POST "$BASE/api/v1/pets" -H "Content-Type: application/json" \
         -d '{"nome":"Cobaia","especie":"gato","raca":"SRD","tutorId":1,"pesoEmKg":3.2}' \
         | sed -E 's/.*"id":([0-9]+).*/\1/')
echo "  pet de teste criado: id $COBAIA"
curl -s -o /dev/null -w "  antes:  GET /api/v1/pet/$COBAIA -> HTTP %{http_code}\n" "$BASE/api/v1/pet/$COBAIA"
echo -n "  chamando GET /api/v1/deletarPet?id=$COBAIA -> "; curl -s "$BASE/api/v1/deletarPet?id=$COBAIA"; echo
curl -s -o /dev/null -w "  depois: GET /api/v1/pet/$COBAIA -> HTTP %{http_code}\n" "$BASE/api/v1/pet/$COBAIA"
echo "  (esta URL funciona colada na barra do navegador. pense nisso.)"

titulo "03 · GET /api/v1/pet/9 — compare esta rota com a do endpoint 06"
curl -s -o /dev/null -w "  /api/v1/pet/9  -> HTTP %{http_code}\n" "$BASE/api/v1/pet/9"
curl -s -o /dev/null -w "  /api/v1/pets/9 -> HTTP %{http_code}\n" "$BASE/api/v1/pets/9"

titulo "04 · grafia das rotas"
curl -s -o /dev/null -w "  /api/v1/banhosTosa   -> HTTP %{http_code}\n" "$BASE/api/v1/banhosTosa?size=2"
curl -s -o /dev/null -w "  /api/v1/banhostosa   -> HTTP %{http_code}\n" "$BASE/api/v1/banhostosa?size=2"
curl -s -o /dev/null -w "  /api/v1/tutores_vip  -> HTTP %{http_code}\n" "$BASE/api/v1/tutores_vip?size=2"
curl -s -o /dev/null -w "  /api/v1/tutores-vip  -> HTTP %{http_code}\n" "$BASE/api/v1/tutores-vip?size=2"

titulo "05 · POST /api/v1/pets — procure o status e o header Location"
curl -s -D - -o /dev/null -X POST "$BASE/api/v1/pets" \
     -H "Content-Type: application/json" \
     -d '{"nome":"Bidu","especie":"cachorro","raca":"SRD","tutorId":1,"pesoEmKg":8.5}' \
  | grep -Ei "^(HTTP|Location)"
echo "  (existe alguma linha Location aí em cima?)"

titulo "06 · GET /api/v1/pets/999999 — este pet não existe"
curl -s -i "$BASE/api/v1/pets/999999" | head -8

titulo "07 · GET /api/pets — compare a URI e os NOMES DOS CAMPOS"
echo "  com versao no path:"; curl -s "$BASE/api/v1/getPets" -X POST | head -c 150; echo
echo "  sem versao no path:"; curl -s "$BASE/api/pets?size=1"; echo

titulo "08 · GET de um exame — quantos ids?"
curl -s -o /dev/null -w "  ids reais    -> HTTP %{http_code}\n" "$BASE/api/v1/petshops/1/clientes/5/pets/9/consultas/12/exames/6"
curl -s -o /dev/null -w "  ids trocados -> HTTP %{http_code}\n" "$BASE/api/v1/petshops/99/clientes/99/pets/99/consultas/12/exames/6"
echo "  (os tres primeiros ids mudaram e a resposta e a mesma. eles servem para que?)"

titulo "09 · GET /api/v1/consultas — meça a resposta"
curl -s -o /dev/null -w "  %{size_download} bytes em %{time_total}s\n" "$BASE/api/v1/consultas"
echo "  compare com a API correta (se estiver rodando na 5301):"
curl -s -o /dev/null -w "  %{size_download} bytes em %{time_total}s\n" "http://localhost:5301/api/v1/pedidos?page=1&size=20" 2>/dev/null

titulo "10 · PUT /api/v1/pets/12/vacinas — três vezes, corpo idêntico"
for i in 1 2 3; do
  QTD=$(curl -s -X PUT "$BASE/api/v1/pets/12/vacinas" -H "Content-Type: application/json" -d '{"nome":"V10"}' \
        | grep -o '"id"' | wc -l)
  echo "  chamada $i -> o pet agora tem $QTD vacina(s)"
done

titulo "11 · sessão — dois logins seguidos"
echo "  GET /meus-pets ANTES de qualquer login desta sessao:"
curl -s -o /dev/null -w "  -> HTTP %{http_code}   (401 num servidor recem-iniciado)\n" "$BASE/api/v1/meus-pets"
curl -s -o /dev/null -X POST "$BASE/api/v1/sessao" -H "Content-Type: application/json" -d '{"usuario":"Ana","senha":"123"}'
echo "  login como Ana. /meus-pets responde:"
curl -s "$BASE/api/v1/meus-pets" | head -c 90; echo
curl -s -o /dev/null -X POST "$BASE/api/v1/sessao" -H "Content-Type: application/json" -d '{"usuario":"Bruno","senha":"123"}'
echo "  agora OUTRO usuario fez login. /meus-pets da Ana responde:"
curl -s "$BASE/api/v1/meus-pets" | head -c 90; echo
echo "  (a Ana nao fez nada. por que a resposta dela mudou?)"

titulo "12 · GET /api/v1/tabela-de-precos — leia os headers"
curl -s -D - -o /dev/null "$BASE/api/v1/tabela-de-precos" | grep -Ei "^(HTTP|Cache-Control|ETag|Pragma)"
echo "  (este dado e reajustado uma vez por ano.)"

echo
echo "=============================================================="
echo "Agora preencha, para cada endpoint: o que observei / qual regra"
echo "REST foi violada / como eu redesenharia."
echo "Reinicie a API (Ctrl+C e dotnet run) para voltar ao estado inicial."
echo "=============================================================="
