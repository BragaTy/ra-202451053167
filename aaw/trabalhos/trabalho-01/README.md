# PS5 Trophy Hunter

- **Aluno:** Matheus Braga de Sousa
- **RA:** 202451053167
- **Curso / Disciplina:** Arquitetura de Aplicações Web — 2026.2

---

## Sobre o Projeto

**PS5 Trophy Hunter** é uma aplicação web completa (REST API em .NET 10 + banco de dados NoSQL MongoDB + interface web assíncrona) desenvolvida para jogadores de PlayStation 5 planejarem e acompanharem suas jornadas em busca do troféu de **Platina**.

### Domínio e Modelagem NoSQL
- **`Jogos` (coleção `jogos`):** armazena o catálogo de jogos, dificuldade da platina (1 a 10), tempo estimado em horas e a **`ListaTrofeus` embutida** (documento embutido, pois os troféus oficiais de um jogo são fixos e lidos sempre juntos com o jogo).
- **`Jornadas` (coleção `jornadas`):** armazena o progresso individual de cada jogador (`PsnId`, códigos dos troféus conquistados, percentual de conclusão, horas jogadas, status e data da platina), referenciando o jogo por **`JogoId`** (referência `1-N`, evitando crescimento ilimitado do documento do jogo).
- **`Usuarios` (coleção `usuarios`):** credenciais com hash de senha e perfil de acesso (`admin` ou `usuario`) para autenticação JWT e autorização RBAC.

---

## Pré-requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`10.0.x`)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (ou uma instância local do **MongoDB** escutando na porta `27017`)

---

## Variáveis de Ambiente

A API **não possui strings de conexão nem segredos fixos no código**. Configure as variáveis abaixo no terminal (ou em `src/TrophyHunter.Api/appsettings.Development.json`, que está no `.gitignore`):

| Variável | Descrição | Exemplo |
| :--- | :--- | :--- |
| `MongoDb__ConnectionString` | URI de conexão do MongoDB | `mongodb://localhost:27017` |
| `MongoDb__Database` | Nome do banco NoSQL | `TrophyHunterDb` |
| `Jwt__Key` | Chave de assinatura do JWT (mín. 32 caracteres) | `<sua-chave-secreta-com-32-ou-mais-caracteres>` |
| `Jwt__Issuer` | Emissor do token JWT | `TrophyHunter.Api` |
| `Jwt__Audience` | Público do token JWT | `TrophyHunter.Web` |
| `Jwt__ExpiracaoMinutos` | Tempo de expiração do token em minutos | `120` |
| `Admin__PsnId` | PSN ID do administrador inicial (seed) | `AdminTrophy` |
| `Admin__Email` | E-mail do administrador inicial (seed) | `admin@trophyhunter.local` |
| `Admin__Senha` | Senha inicial do administrador (seed) | `<defina-sua-senha-local>` |

---

## Passo a Passo para Executar Localmente

### 1. Subir o banco de dados MongoDB
Na pasta `aaw/trabalhos/trabalho-01`, inicie o contêiner do MongoDB:

```bash
docker compose up -d
```

### 2. Definir as variáveis de ambiente (PowerShell)
```powershell
$env:MongoDb__ConnectionString = "mongodb://localhost:27017"
$env:MongoDb__Database = "TrophyHunterDb"
$env:Jwt__Key = "ChaveDeDesenvolvimentoLocalComMaisDe32Caracteres!"
$env:Admin__Email = "admin@trophyhunter.local"
$env:Admin__Senha = "Admin@123"
```

### 3. Executar a API e a Página Web
```bash
dotnet run --project src/TrophyHunter.Api
```

Ao iniciar pela primeira vez, a aplicação cadastra automaticamente 3 jogos de exemplo no MongoDB e cria o usuário administrador configurado nas variáveis de ambiente.

---

## Como Acessar

- **Página Web (navegação assíncrona):** [http://localhost:5050](http://localhost:5050)
- **Documentação OpenAPI / Swagger:** [http://localhost:5050/swagger](http://localhost:5050/swagger)

Para testar rotas protegidas no **Swagger**:
1. Execute `POST /api/auth/login` (ou crie uma conta em `POST /api/auth/registrar`);
2. Copie o valor do campo `token` retornado;
3. Clique no botão **Authorize** no topo do Swagger e cole o token.

---

## Como Executar os Testes Unitários

Na pasta `aaw/trabalhos/trabalho-01`, execute:

```bash
dotnet test
```

A suíte xUnit (`tests/TrophyHunter.Tests`) valida a camada de serviço (`JornadaService` e `JogoService`), cobrindo cenários de sucesso (cálculo de percentual de conclusão e concessão automática do troféu de Platina) e cenários de erro (bloqueio de jornada duplicada `409`, tentativa de platinar com troféus pendentes `400`, acesso negado a jornada de terceiros `403` e jogo inexistente `404`).

---

## Uso de IA

- **Ferramentas usadas:** assistente de IA integrado à IDE.
- **Onde ajudou:** apoio na estruturação inicial das camadas (repositórios do MongoDB, configuração do middleware de JWT/Swagger) e estilização CSS da página web assíncrona.
- **O que eu revisei/reescrevi:** revisei as regras de negócio no `JornadaService` (cálculo do percentual desconsiderando a própria platina e transição automática para o status `Platinado`), conferi o mapeamento de status codes no `TratamentoErrosMiddleware` (`400`, `401`, `403`, `404`, `409` e `500`) e validei os cenários de teste unitário no xUnit.
