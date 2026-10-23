# Proposta — Trabalho Semestral · Arquitetura de Aplicações Web 2026.2

- **Aluno:** Matheus Braga de Sousa · **Curso/Turma:** Arquitetura de Aplicações Web (2026.2)
- **Repositório:** https://github.com/BragaTy/ra-202451053167.git

---

## 1. Domínio e problema

A aplicação é um rastreador e guia de platinas para jogadores de PlayStation 5 (PS5 Trophy Hunter). O problema resolvido é a falta de um local simples e direto onde o jogador consiga planejar a busca pelo troféu de platina, sabendo com antecedência o tempo estimado, o nível de dificuldade do jogo e marcando os troféus conquistados passo a passo. O sistema atende jogadores e caçadores de troféus que querem organizar suas metas de jogos de forma prática e visual.

---

## 2. Entidades

### Entidade A: `Jogos`
Representa o jogo cadastrado no catálogo oficial da plataforma.
- `Id`: identificador único no banco (ObjectId);
- `Titulo`: nome do jogo (ex.: "God of War Ragnarök", "Spider-Man 2");
- `Genero`: categoria do jogo (Ação, RPG, Aventura);
- `DificuldadePlatina`: número de 1 a 10 indicando a dificuldade para platinar;
- `TempoEstimadoHoras`: quantidade média de horas necessárias para a platina;
- `TotalTrofeus`: total de troféus existentes no jogo;
- `ListaTrofeus`: lista de troféus oficiais do jogo, contendo:
  - `CodigoTrofeu`: identificador do troféu;
  - `Nome`: título do troféu;
  - `Descricao`: objetivo para desbloquear;
  - `Tipo`: "Bronze", "Prata", "Ouro" ou "Platina";
  - `TrofeuOculto`: booleano indicando se é troféu secreto de história;
- `Ativo`: booleano que indica se o jogo está ativo para novas jornadas.

### Entidade B: `Jornadas`
Representa o progresso individual de um jogador na tentativa de platinar um determinado jogo.
- `Id`: identificador único da jornada;
- `JogoId`: referência ao identificador do jogo correspondente;
- `PsnId`: apelido/ID da conta do jogador na PlayStation Network;
- `TrofeusConquistados`: lista com os códigos dos troféus que o jogador já concluiu;
- `PercentualConclusao`: porcentagem calculada do progresso atual (0 a 100%);
- `HorasJogadas`: quantidade de horas que o jogador já dedicou até o momento;
- `Status`: situação da jornada ("EmAndamento", "Platinado", "Pausado");
- `DataPlatina`: data e hora em que a platina foi alcançada (preenchida automaticamente).

### Relacionamento e Modelagem NoSQL:
O relacionamento entre `Jogos` e `Jornadas` é de **1 para N** (um jogo pode ter várias jornadas de jogadores diferentes).

- **Documento Embutido:** A lista de troféus oficiais (`ListaTrofeus`) fica **embutida** dentro do documento de `Jogos`. Justificativa: a lista de troféus de um jogo é fixa, não sofre alterações frequentes e precisa ser carregada de forma rápida e completa sempre que o jogo for exibido na tela, sem necessidade de consultas adicionais.
- **Referência:** As `Jornadas` são armazenadas em uma coleção **separada**, guardando apenas o `JogoId` como referência. Justificativa: um jogo pode ter milhares de jogadores acompanhando seu progresso. Se colocássemos todas as jornadas dentro do documento do jogo, o registro cresceria sem controle, ultrapassando os limites do MongoDB e causando lentidão de escrita.

---

## 3. Endpoints previstos

### Entidade `Jogos`
- `GET /api/jogos`: lista todos os jogos com resumo de tempo e dificuldade.
- `GET /api/jogos/{id}`: busca os detalhes completos de um jogo, incluindo sua lista de troféus.
- `POST /api/jogos`: cadastra um novo jogo e seus troféus (restrito a administradores).
- `PUT /api/jogos/{id}`: atualiza dados cadastrais de um jogo existente (restrito a administradores).
- `DELETE /api/jogos/{id}`: remove ou desativa um jogo do catálogo (restrito a administradores).

### Entidade `Jornadas`
- `GET /api/jornadas`: lista as jornadas existentes (com opção de filtro por jogador ou jogo).
- `GET /api/jornadas/{id}`: exibe o detalhamento de uma jornada e a lista de troféus já conquistados.
- `POST /api/jornadas`: inicia o acompanhamento de um jogo por um jogador.
- `PATCH /api/jornadas/{id}/trofeus`: adiciona um troféu conquistado à jornada, recalculando o percentual.
- `DELETE /api/jornadas/{id}`: exclui uma jornada de progresso.

---

## 4. Regra de negócio

A camada de serviço (`JornadaService`) aplicará duas regras centrais de negócio:

1. **Cálculo de Progresso e Conquista Automática da Platina:**
   Ao registrar um novo troféu na jornada via `PATCH`, o serviço recalcula automaticamente o `PercentualConclusao` dividindo os troféus conquistados pelo total de troféus base do jogo. Quando o jogador conquistar todos os troféus regulares (Bronze, Prata e Ouro), a regra de negócio concede automaticamente o troféu de "Platina", altera o `Status` para `"Platinado"` e registra a data atual em `DataPlatina`.

2. **Validação de Bloqueio de Trapaça:**
   O sistema impede que uma requisição force manualmente a alteração do `Status` para `"Platinado"` se o jogador ainda não tiver conquistado 100% dos troféus obrigatórios do jogo.

---

## 5. Casos de erro

1. **Erro de Regra de Negócio / Conflito (`409 Conflict` ou `400 Bad Request`):**
   - Caso um jogador tente iniciar uma nova jornada para um jogo que ele já possui com status `"EmAndamento"`, a API rejeita a criação devolvendo status `409 Conflict` com a mensagem: *"Já existe uma jornada em andamento para este jogo e jogador."*
2. **Erro de Recurso Não Encontrado (`404 Not Found`):**
   - Caso seja enviada uma requisição com um identificador de jogo ou jornada inexistente no banco de dados, a API devolve status `404 Not Found` com a mensagem: *"Registro não encontrado."*

---

## 6. Stack escolhida

- **Backend:** .NET 10 (C#) utilizando ASP.NET Core Web API em arquitetura em camadas (Controllers, Services, Repositories).
- **Banco de Dados:** MongoDB (NoSQL) rodando em contêiner Docker local, com conexão lida estritamente via variável de ambiente.
- **Frontend:** Página web desenvolvida em HTML5, CSS3 moderno e JavaScript puro consumindo a API de forma assíncrona (`fetch`), sem recarregar o navegador.

---

## 7. Segurança e testes

- **Autenticação:** JWT (JSON Web Token) com endpoints de registro (`POST /api/auth/registrar`) e login (`POST /api/auth/login`), com tempo de expiração configurado e envio via cabeçalho `Authorization: Bearer <token>`.
- **Autorização (RBAC):** dois perfis de acesso definidos no payload do token:
  - `usuario`: pode visualizar jogos, abrir suas jornadas e marcar troféus obtidos;
  - `admin`: perfil com permissão exclusiva para cadastrar, editar e excluir jogos do catálogo oficial.
- **Testes Unitários:** suíte de testes com xUnit focada na camada de serviço (`JornadaService`), cobrindo:
  - Cenário de sucesso 1: cálculo correto da porcentagem de progresso ao marcar troféus;
  - Cenário de sucesso 2: transição automática de status para "Platinado" ao completar todos os troféus;
  - Cenário de erro 1: rejeição de jornada duplicada para o mesmo jogo e jogador (`409`);
  - Cenário de erro 2: rejeição ao tentar concluir platina com troféus pendentes (`400`).
  - Executáveis por meio de comando único documentado: `dotnet test`.
