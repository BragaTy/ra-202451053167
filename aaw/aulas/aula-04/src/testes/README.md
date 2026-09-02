# testes/ — como consumir as duas APIs

Os mesmos casos, em quatro formatos. Escolha o que você já usa.
O passo a passo ilustrado está no **PDF da aula** ("Guia de Consumo de APIs REST").

| Arquivo | Ferramenta | Como usar |
|---|---|---|
| `CafeteriaApi.postman_collection.json` | Postman | *Import* → arraste o arquivo → a variável `baseUrl` já vem preenchida |
| `PetShopApi.postman_collection.json` | Postman | idem (porta 5302) |
| `cafeteria.http` / `petshop.http` | VS Code (extensão **REST Client**), Visual Studio 2022, Rider | abra o arquivo e clique em *Send Request* |
| `curl-cafeteria.sh` / `curl-petshop.sh` | Git Bash, WSL, macOS, Linux | `bash curl-cafeteria.sh` |
| `curl-cafeteria.ps1` / `curl-petshop.ps1` | PowerShell (Windows) | `powershell -ExecutionPolicy Bypass -File .\curl-cafeteria.ps1` |

Antes de qualquer coisa, suba as APIs:

```bash
cd ../CafeteriaApi && dotnet run      # porta 5301
cd ../PetShopApi   && dotnet run      # porta 5302
```

## Duas armadilhas de Windows

1. **`curl` no PowerShell 5.1 não é o cURL.** É um alias para `Invoke-WebRequest`,
   que não entende `-X`, `-H` nem `-d`. Use sempre **`curl.exe`** (é por isso que
   os scripts `.ps1` fazem assim).
2. **Aspas.** No PowerShell, o JSON vai entre aspas simples com as aspas duplas
   escapadas (`-d '{\"nome\":\"Bidu\"}'`); no Git Bash, aspas simples bastam
   (`-d '{"nome":"Bidu"}'`).

## Se preferir nem instalar nada

Cada API tem **Swagger UI** embutido: `http://localhost:5301/swagger` e
`http://localhost:5302/swagger`. Clique em *Try it out* → *Execute* e leia
o status code, o corpo e os *response headers*. O Swagger ainda mostra o
comando `curl` equivalente de cada chamada — é uma boa forma de aprender a
sintaxe do cURL sem decorar nada.
