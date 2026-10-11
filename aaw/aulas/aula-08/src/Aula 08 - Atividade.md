# HANDOUT — AULA 08

## Kit dos Grupos — Hoje vocês ensinam
*AuthN & AuthZ: JWT, OAuth 2.0 e OpenID Connect — Arquitetura de Aplicações Web*

## 🎯 MISSÃO

Hoje a aula é de vocês. Cada grupo recebe UM tema, estuda com o texto-base da sua página e ensina esse tema para a turma. Juntos, os 7 temas cobrem toda a aula: de “quem é você?” até o botão “Entrar com Google”.

> **Nomes:** Matheus Braga de Sousa   **Turma:** ____________________   **Data:** 08/10/2026

---

## GRUPO 01 — AuthN × AuthZ — e o par 401 × 403

**Sua análise e respostas:**

1. Qual a diferença entre autenticação e autorização? Deem um exemplo fora da computação (show, aeroporto, academia…).
Autenticação é provar quem você é (identidade). Autorização é saber o que você pode fazer (permissão).
Exemplo: no show, autenticação é mostrar seu RG na entrada para provar sua idade e identidade; autorização é a pulseira VIP que te deixa entrar na área VIP ou só na pista comum.

2. Por que o nome “Unauthorized” do 401 confunde?
Porque fala "Unauthorized" (não autorizado), mas na verdade ele significa "não autenticado". A API não sabe quem você é porque falta token ou ele é inválido.

3. Quando a API deve responder 401 e quando deve responder 403?
401: quando a requisição não tem token, ou o token está inválido ou expirado.
403: quando o usuário está logado e o token é válido, mas ele não tem permissão para acessar aquele recurso.

4. Por que fazer login de novo não resolve um 403?
Porque o sistema já sabe quem você é. O problema é que o seu usuário não tem o papel/permissão necessário. Logar de novo com a mesma conta não vai mudar seu nível de acesso.

**Pergunta-desafio para a turma:**
- (a) GET /emprestimos sem token: **401 Unauthorized** (não se identificou).
- (b) Ana, role aluno, faz DELETE /livros/9, exclusivo de bibliotecários: **403 Forbidden** (está autenticada, mas não tem permissão).
- (c) token válido ontem, mas o exp já passou: **401 Unauthorized** (token expirado).
- (d) Beto, role bibliotecario, faz DELETE /livros/9: **204 No Content / Sucesso** (autenticado e autorizado).

---

## GRUPO 02 — Sessões × tokens: onde mora o estado?

**Sua análise e respostas:**

1. Onde mora o estado do login em cada modelo?
Na sessão, o estado fica guardado na memória do próprio servidor.
No token (JWT), o estado fica guardado dentro do próprio token com o cliente (navegador/app).

2. O que acontece com a sessão em memória quando há 3 servidores atrás de um balanceador?
O usuário faz login no servidor 1, mas a próxima requisição pode cair no servidor 2. Como o servidor 2 não tem a sessão guardada na memória dele, o usuário aparece deslogado.

3. Como o token resolve esse problema, e que preço se paga por isso?
O token resolve porque ele é autocontido. Qualquer um dos 3 servidores que tiver a mesma chave consegue validar o token sozinho, sem precisar olhar na memória.
O preço é que o token é maior e fica muito difícil revogar antes dele expirar.

4. Em que situação a sessão no servidor ainda é uma boa escolha?
Em sites tradicionais pequenos, rodando em um servidor só, onde é mais simples guardar na memória e fácil de invalidar a sessão na hora do logout.

**Pergunta-desafio para a turma:**
- Por que acontece? Porque a sessão ficou gravada na memória de uma máquina e a requisição seguinte foi enviada para outra.
- Duas formas de corrigir:
  1. Usar um banco centralizado de sessão compartilhado (como o Redis).
  2. Trocar sessões por tokens JWT (autocontidos).

---

## GRUPO 03 — Anatomia do JWT: header.payload.signature

**Sua análise e respostas:**

1. O que vai em cada uma das três partes do token?
- Header: tipo do token e algoritmo de assinatura usado (ex: HS256).
- Payload: os dados do usuário e do token chamados de claims (id, nome, role, expiração).
- Signature: a assinatura que junta o header, payload e a chave secreta para garantir que ninguém alterou nada.

2. O que significam sub, name, role, iat e exp?
- sub: id do usuário (subject).
- name: nome da pessoa.
- role: cargo ou papel do usuário (ex: Leitor ou Bibliotecario).
- iat: data/hora em que o token foi criado (issued at).
- exp: data/hora em que o token expira.

3. Por que o servidor precisa conferir exp, iss e aud, e não só a assinatura?
Porque a assinatura só diz que o token não foi adulterado. Conferir o exp evita aceitar token vencido; o iss confirma quem gerou o token; e o aud garante que o token foi feito para esta API e não para outro sistema.

4. Base64URL é criptografia?
Não, é apenas codificação. Qualquer um consegue colar em um site como o jwt.io e ler todo o conteúdo sem precisar de senha.

**Pergunta-desafio para a turma:**
- Cálculo: exp (1769907600) - iat (1769904000) = 3600 segundos.
- O token vale por **60 minutos (1 hora)**.

---

## GRUPO 04 — Assinado ≠ criptografado — e as boas práticas

**Sua análise e respostas:**

1. O que a assinatura garante, e o que ela NÃO garante?
Garante integridade (que os dados não foram modificados) e autenticidade (que quem gerou tinha a chave).
NÃO garante sigilo, pois o conteúdo pode ser lido por qualquer um.

2. Qual a diferença entre HS256 e RS256? Quando cada um faz sentido?
HS256 é simétrico: a mesma chave secreta assina e valida. Faz sentido quando a mesma API cria e consome o token.
RS256 é assimétrico: chave privada assina e chave pública valida. Faz sentido quando um servidor central (ex: Google) emite tokens e várias APIs diferentes precisam validar.

3. Por que usar exp curto + refresh token em vez de um token que vale um mês?
Porque se o token for roubado, o invasor só consegue usar por poucos minutos. O refresh token permite renovar com segurança sem ter que pedir a senha do usuário toda hora.

4. O que nunca deve ir no payload? Por quê?
Senhas, CPF, cartão de crédito ou dados sensíveis. Porque o payload não é criptografado e fica visível para qualquer pessoa.

**Pergunta-desafio para a turma:**
- Consegue LER: tudo o que estiver no payload (nome, id, perfil, etc.).
- Consegue FAZER: qualquer requisição fingindo ser a Ana durante os 40 minutos restantes.
- NÃO consegue fazer: alterar o conteúdo do token (como mudar o papel para admin), pois isso invalidaria a assinatura.
- Como o servidor se protege: usar tempo de expiração curto (minutos) e manter uma lista de tokens revogados se necessário.

---

## GRUPO 05 — OAuth 2.0: os quatro papéis

**Sua análise e respostas:**

1. No caso NovoApp + Google Agenda, quem é cada um dos 4 papéis?
- Resource Owner: você (dono da agenda).
- Client: o aplicativo NovoApp.
- Authorization Server: o servidor de autenticação do Google.
- Resource Server: a API do Google Agenda onde estão os eventos.

2. O que são escopo e consentimento? Por que eles importam?
Escopo é a permissão solicitada (ex: só ler eventos da agenda).
Consentimento é a tela onde o dono autoriza esse acesso.
Importam para dar apenas o acesso mínimo necessário, sem abrir a conta inteira.

3. Que problemas de “dar a senha para o app” o OAuth resolve?
O app não vê a sua senha real, tem acesso apenas ao que você autorizou e você pode cancelar o acesso a qualquer momento sem precisar trocar a senha da sua conta.

4. Expliquem com a analogia da “chave de manobrista”.
A chave de manobrista só liga o carro para manobrar, mas não abre o porta-malas. O OAuth dá uma chave com poder limitado em vez de entregar a chave mestra com a senha.

**Pergunta-desafio para a turma:**
- Resource Owner: você.
- Client: o aplicativo de impressão.
- Authorization Server: Google Accounts.
- Resource Server: API do Google Fotos.
- Escopo recomendado: apenas leitura de fotos (`photoslibrary.readonly`).

---

## GRUPO 06 — O fluxo por trás do botão: authorization code + PKCE

**Sua análise e respostas:**

1. Qual a sequência dos passos, e em cada um, quem fala com quem?
1. Usuário clica no botão "Entrar com Google" no app.
2. App redireciona o navegador para o Google.
3. Usuário faz login e aceita a permissão no Google.
4. Google redireciona o navegador de volta para o app com um código temporário.
5. Servidor do app fala direto com o Google (back-channel) e troca o código pelo token.
6. App recebe o token e cria a sessão do usuário.

2. Por que o Google devolve um código temporário em vez de entregar o token direto no navegador?
Porque o navegador é inseguro (histórico, extensões, logs). O código temporário sozinho não tem utilidade para quem interceptar, pois só o servidor do app consegue trocá-lo pelo token verdadeiro.

3. Que ataque o PKCE impede? E o state?
O PKCE impede que um aplicativo malicioso intercepte o código e pegue o token.
O state impede ataques de CSRF (falsificação de solicitação entre sites).

4. Quando se usa client credentials em vez de authorization code?
Quando a comunicação é direta entre dois serviços ou APIs, sem nenhum usuário humano participando do login.

**Pergunta-desafio para a turma:**
Em qual passo o NovoApp viu a senha do Google?
- **Em nenhum passo.** O usuário digita a senha diretamente na página do Google.

---

## GRUPO 07 — OpenID Connect: id_token × access_token

**Sua análise e respostas:**

1. Por que o OAuth 2.0 sozinho não é um protocolo de autenticação?
Porque ele foi feito apenas para autorização (acesso a recursos). Ele não garante e nem informa de forma padronizada quem é o usuário logado.

2. Qual a diferença entre id_token e access_token: para quem é cada um e para que serve?
- id_token: é para o aplicativo (client) saber QUEM é o usuário (contém nome, e-mail, etc.).
- access_token: é para a API (resource server) saber se o portador tem permissão para acessar os dados.

3. O que o app precisa conferir no id_token antes de confiar nele?
Precisa conferir a assinatura com a chave pública do provedor, a data de expiração (exp), o emissor (iss), se o destinatário é ele mesmo (aud) e o nonce.

4. O que é SSO? Deem exemplos que vocês usam.
É o Single Sign-On, onde um único login permite acessar vários serviços diferentes.
Exemplos: "Entrar com Google", conta Gov.br, login único da universidade.

**Pergunta-desafio para a turma:**
O OAuth 2.0, sozinho, prova quem você é? Um app que recebeu só um access token pode afirmar “o usuário logado é a Ana”?
- **Não.** O access token serve apenas para acessar recursos, não prova a identidade do usuário. Para provar identidade é necessário o id_token do OpenID Connect.
