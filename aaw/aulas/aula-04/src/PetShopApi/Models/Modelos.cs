using System.ComponentModel.DataAnnotations;

namespace PetShopApi.Models;

/// <summary>Um pet cadastrado no PetHouse.</summary>
public class Pet
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>cachorro | gato | passaro | roedor</summary>
    public string Especie { get; set; } = "cachorro";

    public string Raca { get; set; } = string.Empty;

    public int TutorId { get; set; }

    public double PesoEmKg { get; set; }

    public DateTime Nascimento { get; set; }
}

/// <summary>O dono do pet.</summary>
public class Tutor
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public bool Vip { get; set; }
}

/// <summary>Um atendimento de banho e/ou tosa.</summary>
public class BanhoTosa
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string Servico { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
}

/// <summary>Uma consulta veterinária.</summary>
public class Consulta
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string Veterinario { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;
    public DateTime Data { get; set; }
    public decimal Valor { get; set; }
}

/// <summary>Um exame pedido dentro de uma consulta.</summary>
public class Exame
{
    public int Id { get; set; }
    public int ConsultaId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Resultado { get; set; } = string.Empty;
}

/// <summary>Uma vacina aplicada no pet.</summary>
public class Vacina
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public DateTime Aplicada { get; set; }
}

/// <summary>Uma linha da tabela de preços do PetHouse (muda uma vez por ano).</summary>
public class ItemDePreco
{
    public string Servico { get; set; } = string.Empty;
    public decimal Valor { get; set; }
}

/// <summary>Corpo do "login" do PetHouse.</summary>
public class Credenciais
{
    public string Usuario { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}
