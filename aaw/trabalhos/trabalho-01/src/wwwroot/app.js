// HelpDesk — página única que consome a API com fetch (sem recarregar).
// Navegação por hash: #/chamados, #/chamados/{id}, #/chamados/novo, #/clientes...
"use strict";

const API = "/api/v1";
const conteudo = document.getElementById("conteudo");

// ---------- Sessão (token JWT) ----------
const sessao = {
  ler() {
    try { return JSON.parse(localStorage.getItem("helpdesk.sessao")); } catch { return null; }
  },
  salvar(dados) {
    try { localStorage.setItem("helpdesk.sessao", JSON.stringify(dados)); } catch { /* sem storage */ }
    sessao.atual = dados;
  },
  limpar() {
    try { localStorage.removeItem("helpdesk.sessao"); } catch { /* sem storage */ }
    sessao.atual = null;
  },
  atual: null
};
sessao.atual = sessao.ler();
if (sessao.atual && new Date(sessao.atual.expiraEm) <= new Date()) sessao.limpar();

const ehAdmin = () => sessao.atual?.perfil === "admin";

// ---------- Utilitários ----------
// Escapa texto vindo da API antes de pôr no HTML (evita XSS).
const esc = (v) => String(v ?? "").replace(/[&<>"']/g, (c) =>
  ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));

const dataHora = (iso) => iso ? new Date(iso).toLocaleString("pt-BR") : "";
const rotuloStatus = { Aberto: "Aberto", EmAtendimento: "Em atendimento", Concluido: "Concluído" };
const rotuloPrioridade = { Baixa: "Baixa", Media: "Média", Alta: "Alta" };
const selo = (valor, rotulos) => `<span class="selo ${esc(valor)}">${esc(rotulos?.[valor] ?? valor)}</span>`;

function avisar(texto) {
  const el = document.getElementById("aviso");
  el.textContent = texto;
  el.hidden = false;
  clearTimeout(avisar.t);
  avisar.t = setTimeout(() => (el.hidden = true), 3500);
}

function ir(hash) { location.hash = hash; }

// Chamada à API. Erros chegam em Problem Details (RFC 9457).
async function api(metodo, caminho, corpo) {
  const headers = { Accept: "application/json" };
  if (corpo !== undefined) headers["Content-Type"] = "application/json";
  if (sessao.atual) headers.Authorization = `Bearer ${sessao.atual.token}`;

  const resp = await fetch(API + caminho, {
    method: metodo, headers, body: corpo !== undefined ? JSON.stringify(corpo) : undefined
  });

  if (resp.status === 401 && sessao.atual) {
    sessao.limpar();
    atualizarTopo();
    ir("#/login");
    throw new Error("Sua sessão expirou. Entre novamente.");
  }
  if (resp.status === 204) return null;

  const dados = await resp.json().catch(() => null);
  if (!resp.ok) {
    const validacao = dados?.errors ? Object.values(dados.errors).flat().join(" ") : "";
    throw new Error(validacao || dados?.detail || dados?.title || `Erro ${resp.status}`);
  }
  return dados;
}

function telaErro(erro) {
  conteudo.innerHTML = `<div class="cartao"><p class="erro">${esc(erro.message)}</p></div>`;
}

function lerFormulario(form) {
  return Object.fromEntries(new FormData(form).entries());
}

// Envia um formulário, mostrando o erro da API dentro dele.
function aoEnviar(form, acao) {
  form.addEventListener("submit", async (ev) => {
    ev.preventDefault();
    const botao = form.querySelector("[type=submit]");
    const caixaErro = form.querySelector(".erro");
    caixaErro.hidden = true;
    botao.disabled = true;
    try {
      await acao(lerFormulario(form));
    } catch (erro) {
      caixaErro.textContent = erro.message;
      caixaErro.hidden = false;
    } finally {
      botao.disabled = false;
    }
  });
}

// ---------- Topo ----------
function atualizarTopo() {
  const logado = !!sessao.atual;
  document.getElementById("menu").hidden = !logado;
  document.getElementById("sessao").hidden = !logado;
  if (logado) {
    document.getElementById("usuario-nome").textContent = sessao.atual.nome;
    document.getElementById("usuario-perfil").textContent = sessao.atual.perfil;
  }
  const raiz = location.hash.split("/")[1];
  document.querySelectorAll("#menu [data-rota]").forEach((a) =>
    a.classList.toggle("ativo", a.dataset.rota === raiz));
}

document.getElementById("sair").addEventListener("click", () => {
  sessao.limpar();
  atualizarTopo();
  ir("#/login");
});

// ---------- Login e cadastro ----------
function telaLogin() {
  conteudo.innerHTML = `
    <section class="cartao login">
      <h1>Entrar no HelpDesk</h1>
      <p class="dica">Acompanhe os chamados de suporte e os prazos de atendimento (SLA).</p>
      <div class="abas" role="tablist">
        <button type="button" class="botao" data-aba="entrar">Entrar</button>
        <button type="button" class="botao fantasma" data-aba="registrar">Criar conta</button>
      </div>
      <form id="form-entrar" class="formulario">
        <label>E-mail <input name="email" type="email" required autocomplete="username"></label>
        <label>Senha <input name="senha" type="password" required autocomplete="current-password"></label>
        <p class="erro" hidden></p>
        <button class="botao" type="submit">Entrar</button>
      </form>
      <form id="form-registrar" class="formulario" hidden>
        <label>Nome <input name="nome" required maxlength="100"></label>
        <label>E-mail <input name="email" type="email" required autocomplete="username"></label>
        <label>Senha (mínimo 8 caracteres) <input name="senha" type="password" minlength="8" required autocomplete="new-password"></label>
        <p class="erro" hidden></p>
        <button class="botao" type="submit">Criar conta</button>
      </form>
    </section>`;

  const formEntrar = document.getElementById("form-entrar");
  const formRegistrar = document.getElementById("form-registrar");

  conteudo.querySelectorAll("[data-aba]").forEach((b) => b.addEventListener("click", () => {
    const registrar = b.dataset.aba === "registrar";
    formEntrar.hidden = registrar;
    formRegistrar.hidden = !registrar;
    conteudo.querySelectorAll("[data-aba]").forEach((x) =>
      x.classList.toggle("fantasma", x !== b));
  }));

  const entrar = async ({ email, senha }) => {
    const r = await api("POST", "/auth/login", { email, senha });
    sessao.salvar(r);
    atualizarTopo();
    ir("#/chamados");
  };

  aoEnviar(formEntrar, entrar);
  aoEnviar(formRegistrar, async (dados) => {
    await api("POST", "/auth/registrar", dados);
    avisar("Conta criada. Entrando…");
    await entrar(dados);
  });
}

// ---------- Clientes ----------
async function buscarClientes() {
  const r = await api("GET", "/clientes?page=1&size=100");
  return r.itens;
}

async function telaClientes(page = 1) {
  const r = await api("GET", `/clientes?page=${page}&size=10`);
  const totalPaginas = Math.max(1, Math.ceil(r.total / r.size));
  conteudo.innerHTML = `
    <div class="cabecalho">
      <h1>Clientes</h1>
      ${ehAdmin() ? `<a class="botao" href="#/clientes/novo">Novo cliente</a>` : ""}
    </div>
    <section class="cartao">
      <div class="tabela-rolagem">
        <table>
          <thead><tr><th>Nome</th><th class="ocultar-celular">E-mail</th><th>Plano</th><th>Situação</th></tr></thead>
          <tbody>
            ${r.itens.length === 0 ? `<tr><td colspan="4" class="vazio">Nenhum cliente cadastrado.</td></tr>` :
              r.itens.map((c) => `
                <tr data-id="${esc(c.id)}">
                  <td>${esc(c.nome)}</td>
                  <td class="ocultar-celular">${esc(c.email)}</td>
                  <td>${esc(c.plano)}</td>
                  <td>${c.ativo ? selo("Ativo") : `<span class="selo inativo">Inativo</span>`}</td>
                </tr>`).join("")}
          </tbody>
        </table>
      </div>
      ${paginacao(page, totalPaginas, r.total, "#/clientes")}
    </section>`;
  ligarLinhas("#/clientes/");
}

async function telaCliente(id) {
  const [cliente, chamados] = await Promise.all([
    api("GET", `/clientes/${encodeURIComponent(id)}`),
    api("GET", `/clientes/${encodeURIComponent(id)}/chamados?page=1&size=50`)
  ]);
  conteudo.innerHTML = `
    <div class="cabecalho">
      <div><a class="voltar" href="#/clientes">← Clientes</a><h1>${esc(cliente.nome)}</h1></div>
      ${ehAdmin() ? `<div class="acoes">
        <a class="botao fantasma" href="#/clientes/${esc(cliente.id)}/editar">Editar</a>
        <button class="botao perigo" id="remover" type="button">Remover</button></div>` : ""}
    </div>
    <section class="cartao">
      <dl class="detalhes">
        <div><dt>E-mail</dt><dd>${esc(cliente.email)}</dd></div>
        <div><dt>Telefone</dt><dd>${esc(cliente.telefone)}</dd></div>
        <div><dt>Plano</dt><dd>${esc(cliente.plano)}${cliente.plano === "Completo" ? " (SLA pela metade)" : ""}</dd></div>
        <div><dt>Situação</dt><dd>${cliente.ativo ? "Ativo" : "Inativo"}</dd></div>
      </dl>
    </section>
    <section class="cartao">
      <div class="cabecalho"><h2>Chamados deste cliente (${chamados.total})</h2>
        ${cliente.ativo ? `<a class="botao fantasma" href="#/chamados/novo?cliente=${esc(cliente.id)}">Abrir chamado</a>` : ""}</div>
      ${tabelaChamados(chamados.itens, null)}
    </section>`;
  ligarLinhas("#/chamados/");

  document.getElementById("remover")?.addEventListener("click", async () => {
    if (!confirm(`Remover o cliente "${cliente.nome}"?`)) return;
    try {
      await api("DELETE", `/clientes/${encodeURIComponent(id)}`);
      avisar("Cliente removido.");
      ir("#/clientes");
    } catch (erro) { avisar(erro.message); }
  });
}

async function telaFormCliente(id) {
  const cliente = id ? await api("GET", `/clientes/${encodeURIComponent(id)}`) : null;
  conteudo.innerHTML = `
    <div class="cabecalho"><div>
      <a class="voltar" href="${id ? `#/clientes/${esc(id)}` : "#/clientes"}">← Voltar</a>
      <h1>${id ? "Editar cliente" : "Novo cliente"}</h1></div></div>
    <section class="cartao">
      <form class="formulario" id="form-cliente">
        <label>Nome <input name="nome" required maxlength="100" value="${esc(cliente?.nome)}"></label>
        <label>E-mail <input name="email" type="email" required value="${esc(cliente?.email)}"></label>
        <label>Telefone <input name="telefone" type="tel" required value="${esc(cliente?.telefone)}"></label>
        <label>Plano
          <select name="plano">
            <option value="Basico" ${cliente?.plano === "Basico" ? "selected" : ""}>Básico</option>
            <option value="Completo" ${cliente?.plano === "Completo" ? "selected" : ""}>Completo (SLA pela metade)</option>
          </select>
        </label>
        ${id ? `<label class="check"><input type="checkbox" name="ativo" ${cliente.ativo ? "checked" : ""}> Cliente ativo</label>` : ""}
        <p class="erro" hidden></p>
        <div class="acoes"><button class="botao" type="submit">Salvar</button></div>
      </form>
    </section>`;

  aoEnviar(document.getElementById("form-cliente"), async (dados) => {
    const corpo = { nome: dados.nome, email: dados.email, telefone: dados.telefone, plano: dados.plano };
    if (id) {
      corpo.ativo = dados.ativo === "on";
      await api("PUT", `/clientes/${encodeURIComponent(id)}`, corpo);
      avisar("Cliente atualizado.");
      ir(`#/clientes/${id}`);
    } else {
      const novo = await api("POST", "/clientes", corpo);
      avisar("Cliente cadastrado.");
      ir(`#/clientes/${novo.id}`);
    }
  });
}

// ---------- Chamados ----------
function tabelaChamados(itens, nomes) {
  if (itens.length === 0) return `<p class="vazio">Nenhum chamado encontrado.</p>`;
  return `<div class="tabela-rolagem"><table>
    <thead><tr><th>Assunto</th>${nomes ? `<th class="ocultar-celular">Cliente</th>` : ""}
      <th>Prioridade</th><th>Prazo</th><th>Status</th><th class="ocultar-celular">Abertura</th></tr></thead>
    <tbody>${itens.map((c) => `
      <tr data-id="${esc(c.id)}">
        <td>${esc(c.assunto)}</td>
        ${nomes ? `<td class="ocultar-celular">${esc(nomes.get(c.clienteId) ?? "—")}</td>` : ""}
        <td>${selo(c.prioridade, rotuloPrioridade)}</td>
        <td>${esc(c.prazoHoras)} h</td>
        <td>${selo(c.status, rotuloStatus)}</td>
        <td class="ocultar-celular">${esc(dataHora(c.dataAbertura))}</td>
      </tr>`).join("")}</tbody></table></div>`;
}

async function telaChamados(params) {
  const page = Number(params.get("page") || 1);
  const status = params.get("status") || "";
  const prioridade = params.get("prioridade") || "";
  const filtro = new URLSearchParams({ page, size: 10 });
  if (status) filtro.set("status", status);
  if (prioridade) filtro.set("prioridade", prioridade);

  const [r, clientes] = await Promise.all([api("GET", `/chamados?${filtro}`), buscarClientes()]);
  const nomes = new Map(clientes.map((c) => [c.id, c.nome]));
  const totalPaginas = Math.max(1, Math.ceil(r.total / r.size));
  const opcoes = (lista, rotulos, atual) => lista.map((v) =>
    `<option value="${v}" ${v === atual ? "selected" : ""}>${rotulos[v]}</option>`).join("");

  conteudo.innerHTML = `
    <div class="cabecalho">
      <h1>Chamados</h1>
      <a class="botao" href="#/chamados/novo">Abrir chamado</a>
    </div>
    <section class="cartao">
      <div class="filtros">
        <select id="f-status" aria-label="Filtrar por status"><option value="">Todos os status</option>
          ${opcoes(["Aberto", "EmAtendimento", "Concluido"], rotuloStatus, status)}</select>
        <select id="f-prioridade" aria-label="Filtrar por prioridade"><option value="">Todas as prioridades</option>
          ${opcoes(["Alta", "Media", "Baixa"], rotuloPrioridade, prioridade)}</select>
      </div>
      ${tabelaChamados(r.itens, nomes)}
      ${paginacao(page, totalPaginas, r.total, `#/chamados?${new URLSearchParams({ status, prioridade })}`)}
    </section>`;

  const aplicarFiltro = () => {
    const p = new URLSearchParams();
    const s = document.getElementById("f-status").value;
    const pr = document.getElementById("f-prioridade").value;
    if (s) p.set("status", s);
    if (pr) p.set("prioridade", pr);
    ir(`#/chamados?${p}`);
  };
  document.getElementById("f-status").addEventListener("change", aplicarFiltro);
  document.getElementById("f-prioridade").addEventListener("change", aplicarFiltro);
  ligarLinhas("#/chamados/");
}

async function telaChamado(id) {
  const chamado = await api("GET", `/chamados/${encodeURIComponent(id)}`);
  const cliente = await api("GET", `/clientes/${encodeURIComponent(chamado.clienteId)}`).catch(() => null);
  const prazoFinal = new Date(new Date(chamado.dataAbertura).getTime() + chamado.prazoHoras * 3600000);
  conteudo.innerHTML = `
    <div class="cabecalho">
      <div><a class="voltar" href="#/chamados">← Chamados</a><h1>${esc(chamado.assunto)}</h1></div>
      <div class="acoes">
        <a class="botao fantasma" href="#/chamados/${esc(chamado.id)}/editar">Editar</a>
        ${ehAdmin() ? `<button class="botao perigo" id="remover" type="button">Remover</button>` : ""}
      </div>
    </div>
    <section class="cartao">
      <dl class="detalhes">
        <div><dt>Status</dt><dd>${selo(chamado.status, rotuloStatus)}</dd></div>
        <div><dt>Prioridade</dt><dd>${selo(chamado.prioridade, rotuloPrioridade)}</dd></div>
        <div><dt>Prazo (SLA)</dt><dd>${esc(chamado.prazoHoras)} horas · até ${esc(dataHora(prazoFinal))}</dd></div>
        <div><dt>Cliente</dt><dd>${cliente ? `<a href="#/clientes/${esc(cliente.id)}">${esc(cliente.nome)}</a> (${esc(cliente.plano)})` : "—"}</dd></div>
        <div><dt>Abertura</dt><dd>${esc(dataHora(chamado.dataAbertura))}</dd></div>
      </dl>
    </section>
    <section class="cartao"><h2>Descrição</h2><p class="descricao">${esc(chamado.descricao)}</p></section>`;

  document.getElementById("remover")?.addEventListener("click", async () => {
    if (!confirm("Remover este chamado?")) return;
    try {
      await api("DELETE", `/chamados/${encodeURIComponent(id)}`);
      avisar("Chamado removido.");
      ir("#/chamados");
    } catch (erro) { avisar(erro.message); }
  });
}

async function telaFormChamado(id, params) {
  const chamado = id ? await api("GET", `/chamados/${encodeURIComponent(id)}`) : null;
  const clientes = id ? [] : (await buscarClientes()).filter((c) => c.ativo);
  const clienteInicial = params.get("cliente") || "";

  conteudo.innerHTML = `
    <div class="cabecalho"><div>
      <a class="voltar" href="${id ? `#/chamados/${esc(id)}` : "#/chamados"}">← Voltar</a>
      <h1>${id ? "Editar chamado" : "Abrir chamado"}</h1></div></div>
    <section class="cartao">
      <form class="formulario" id="form-chamado">
        ${id ? "" : `
          <label>Cliente
            <select name="clienteId" required>
              <option value="">Selecione…</option>
              ${clientes.map((c) => `<option value="${esc(c.id)}" ${c.id === clienteInicial ? "selected" : ""}>${esc(c.nome)} (${esc(c.plano)})</option>`).join("")}
            </select>
          </label>
          ${clientes.length === 0 ? `<p class="dica">Não há clientes ativos. Um admin precisa cadastrar um cliente antes.</p>` : ""}`}
        <label>Assunto <input name="assunto" required maxlength="120" value="${esc(chamado?.assunto)}"></label>
        <label>Descrição <textarea name="descricao" required>${esc(chamado?.descricao)}</textarea></label>
        ${id ? `
          <label>Status
            <select name="status">
              ${["Aberto", "EmAtendimento", "Concluido"].map((s) =>
                `<option value="${s}" ${chamado.status === s ? "selected" : ""}>${rotuloStatus[s]}</option>`).join("")}
            </select>
          </label>
          <p class="dica">Prioridade ${esc(rotuloPrioridade[chamado.prioridade])} · prazo de ${esc(chamado.prazoHoras)} h. Chamado concluído não pode ser reaberto.</p>` : `
          <label>Prioridade
            <select name="prioridade">
              <option value="Alta">Alta (8 h)</option>
              <option value="Media" selected>Média (24 h)</option>
              <option value="Baixa">Baixa (48 h)</option>
            </select>
          </label>
          <p class="dica">O prazo cai pela metade para clientes do plano Completo.</p>`}
        <p class="erro" hidden></p>
        <div class="acoes"><button class="botao" type="submit">${id ? "Salvar" : "Abrir chamado"}</button></div>
      </form>
    </section>`;

  aoEnviar(document.getElementById("form-chamado"), async (dados) => {
    if (id) {
      await api("PUT", `/chamados/${encodeURIComponent(id)}`,
        { assunto: dados.assunto, descricao: dados.descricao, status: dados.status });
      avisar("Chamado atualizado.");
      ir(`#/chamados/${id}`);
    } else {
      const novo = await api("POST", "/chamados", dados);
      avisar(`Chamado aberto com prazo de ${novo.prazoHoras} h.`);
      ir(`#/chamados/${novo.id}`);
    }
  });
}

// ---------- Componentes ----------
function paginacao(page, totalPaginas, total, base) {
  const sep = base.includes("?") ? "&" : "?";
  return `<div class="paginacao">
    <span>${total} registro(s) · página ${page} de ${totalPaginas}</span>
    ${page > 1 ? `<a class="botao fantasma" href="${base}${sep}page=${page - 1}">Anterior</a>` : ""}
    ${page < totalPaginas ? `<a class="botao fantasma" href="${base}${sep}page=${page + 1}">Próxima</a>` : ""}
  </div>`;
}

function ligarLinhas(prefixo) {
  conteudo.querySelectorAll("tbody tr[data-id]").forEach((tr) =>
    tr.addEventListener("click", () => ir(prefixo + tr.dataset.id)));
}

// ---------- Roteador ----------
async function rotear() {
  const [caminho, query = ""] = location.hash.slice(1).split("?");
  const partes = caminho.split("/").filter(Boolean);
  const params = new URLSearchParams(query);

  if (!sessao.atual) {
    atualizarTopo();
    if (partes[0] !== "login") return ir("#/login");
    return telaLogin();
  }
  if (partes.length === 0 || partes[0] === "login") return ir("#/chamados");

  atualizarTopo();
  conteudo.innerHTML = `<p class="carregando">Carregando…</p>`;
  window.scrollTo(0, 0);

  try {
    const [area, id, acao] = partes;
    if (area === "clientes") {
      if (!id) return await telaClientes(Number(params.get("page") || 1));
      if (id === "novo") return await telaFormCliente(null);
      if (acao === "editar") return await telaFormCliente(id);
      return await telaCliente(id);
    }
    if (area === "chamados") {
      if (!id) return await telaChamados(params);
      if (id === "novo") return await telaFormChamado(null, params);
      if (acao === "editar") return await telaFormChamado(id, params);
      return await telaChamado(id);
    }
    ir("#/chamados");
  } catch (erro) {
    telaErro(erro);
  }
}

window.addEventListener("hashchange", rotear);
rotear();
