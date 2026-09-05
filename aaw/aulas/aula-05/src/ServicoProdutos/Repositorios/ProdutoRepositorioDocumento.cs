using System.Text.Json;
using ServicoProdutos.Models;

namespace ServicoProdutos.Repositorios;

/// <summary>
/// Implementação ORIENTADA A DOCUMENTOS — VOCÊ vai completá-la.
/// Cada produto é UM ARQUIVO JSON (um documento por agregado), como
/// num MongoDB simplificado: dados/produto-1.json, dados/produto-2.json...
/// Não há tabela, não há esquema imposto pelo "banco": o documento é o dado.
/// </summary>
public class ProdutoRepositorioDocumento : IProdutoRepositorio
{
    private readonly string _pasta;

    public ProdutoRepositorioDocumento(IConfiguration config)
    {
        _pasta = config["PastaDocumentos"] ?? "dados";
        Directory.CreateDirectory(_pasta);
    }

    private string CaminhoDoDocumento(int id) => Path.Combine(_pasta, $"produto-{id}.json");

    public List<Produto> ObterTodos()
    {
        // TODO 1: liste todos os documentos da coleção.
        //   a) Directory.EnumerateFiles(_pasta, "produto-*.json")
        //   b) para cada arquivo: File.ReadAllText + JsonSerializer.Deserialize<Produto>
        //   c) retorne a lista ordenada por Id
        // Pergunta para anotar: quem faz o trabalho aqui, o "banco" ou a aplicação?
        throw new NotImplementedException("TODO 1 — implementar leitura de todos os documentos");
    }

    public Produto? ObterPorId(int id)
    {
        // TODO 2: acesso DIRETO ao documento — a grande força do modelo:
        //   a) monte o caminho com CaminhoDoDocumento(id)
        //   b) se o arquivo não existe, retorne null
        //   c) senão, desserialize e retorne
        // Repare: nenhuma "consulta" — a chave leva direto ao documento.
        throw new NotImplementedException("TODO 2 — implementar leitura por id");
    }

    public List<Produto> ObterAbaixoDe(decimal precoMaximo)
    {
        // TODO 3: filtre produtos com Preco < precoMaximo.
        //   Sem índice e sem SQL, o único caminho é: carregar TODOS os
        //   documentos (reuse ObterTodos()) e filtrar em memória com Where.
        // Compare com o ProdutoRepositorioSql: onde o filtro aconteceu lá?
        // Anote essa diferença — ela cai na discussão final da aula.
        throw new NotImplementedException("TODO 3 — implementar filtro por preço");
    }

    public Produto Criar(Produto produto)
    {
        // TODO 4: gere o próximo Id e grave o documento:
        //   a) próximo Id = maior Id existente + 1 (use ObterTodos())
        //   b) serialize com JsonSerializer.Serialize(produto,
        //      new JsonSerializerOptions { WriteIndented = true })
        //   c) File.WriteAllText no caminho do documento
        //   d) retorne o produto criado
        // Abra a pasta dados/ e olhe o arquivo gerado: ISSO é o "registro".
        throw new NotImplementedException("TODO 4 — implementar gravação do documento");
    }
}
