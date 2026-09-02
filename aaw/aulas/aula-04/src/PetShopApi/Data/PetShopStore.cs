using PetShopApi.Models;

namespace PetShopApi.Data;

/// <summary>
/// Banco em memória do PetHouse. O store em si está correto —
/// os problemas desta API estão no DESIGN dos endpoints, não aqui.
/// </summary>
public class PetShopStore
{
    private readonly object _trava = new();

    public List<Tutor> Tutores { get; } = new();
    public List<Pet> Pets { get; } = new();
    public List<BanhoTosa> BanhosETosas { get; } = new();
    public List<Consulta> Consultas { get; } = new();
    public List<Exame> Exames { get; } = new();
    public List<Vacina> Vacinas { get; } = new();
    public List<ItemDePreco> TabelaDePrecos { get; } = new();

    private int _proximoPetId = 1;
    private int _proximaVacinaId = 1;

    private static readonly string[] NomesDePet =
    {
        "Rex", "Mel", "Thor", "Luna", "Bidu", "Nina", "Bob", "Amora", "Fred", "Cacau",
        "Zeus", "Frida", "Simba", "Maya", "Pipoca", "Tobias", "Lola", "Bento", "Fiona", "Chico"
    };
    private static readonly string[] Especies = { "cachorro", "gato", "passaro", "roedor" };
    private static readonly string[] Racas =
    {
        "SRD", "Labrador", "Poodle", "Bulldog", "Siames", "Persa", "Golden", "Shih Tzu", "Calopsita", "Hamster"
    };
    private static readonly string[] NomesDeTutor =
    {
        "Ana Souza", "Bruno Lima", "Carla Dias", "Diego Alves", "Elisa Rocha", "Fabio Melo",
        "Gabi Torres", "Heitor Pinto", "Isis Nunes", "Joao Prado", "Kelly Ramos", "Lucas Vieira"
    };
    private static readonly string[] Veterinarios = { "Dra. Sofia", "Dr. Andre", "Dra. Renata", "Dr. Paulo" };
    private static readonly string[] Motivos = { "check-up", "vacinacao", "dermatite", "castracao", "dor abdominal" };
    private static readonly string[] Servicos = { "banho", "tosa higienica", "tosa completa", "hidratacao" };
    private static readonly string[] TiposDeExame = { "hemograma", "raio-x", "ultrassom", "urina" };

    public PetShopStore()
    {
        for (var t = 1; t <= 300; t++)
        {
            Tutores.Add(new Tutor
            {
                Id = t,
                Nome = $"{NomesDeTutor[t % NomesDeTutor.Length]} {t:D3}",
                Telefone = $"(31) 9{t:D4}-{(t * 7) % 10000:D4}",
                Vip = t % 9 == 0
            });
        }

        // 2.000 pets: é MUITO para uma resposta só — e é esse o ponto do endpoint sem paginação.
        for (var i = 0; i < 2000; i++)
        {
            Pets.Add(new Pet
            {
                Id = _proximoPetId++,
                Nome = $"{NomesDePet[i % NomesDePet.Length]} {i + 1:D4}",
                Especie = Especies[i % Especies.Length],
                Raca = Racas[i % Racas.Length],
                TutorId = (i % 300) + 1,
                PesoEmKg = Math.Round(1.5 + (i % 40) * 0.8, 1),
                Nascimento = new DateTime(2015, 1, 1).AddDays(i * 3)
            });
        }

        var proximaConsultaId = 1;
        var proximoExameId = 1;
        var proximoBanhoId = 1;

        // 6.000 consultas (3 por pet) e exames dentro delas.
        foreach (var pet in Pets)
        {
            for (var c = 0; c < 3; c++)
            {
                var consulta = new Consulta
                {
                    Id = proximaConsultaId++,
                    PetId = pet.Id,
                    Veterinario = Veterinarios[(pet.Id + c) % Veterinarios.Length],
                    Motivo = Motivos[(pet.Id + c) % Motivos.Length],
                    Data = new DateTime(2026, 1, 5).AddDays((pet.Id + c) % 200),
                    Valor = 120m + ((pet.Id + c) % 5) * 25m
                };
                Consultas.Add(consulta);

                if (consulta.Id % 2 == 0)
                {
                    Exames.Add(new Exame
                    {
                        Id = proximoExameId++,
                        ConsultaId = consulta.Id,
                        Tipo = TiposDeExame[consulta.Id % TiposDeExame.Length],
                        Resultado = consulta.Id % 3 == 0 ? "alterado" : "normal"
                    });
                }
            }

            if (pet.Id % 3 == 0)
            {
                BanhosETosas.Add(new BanhoTosa
                {
                    Id = proximoBanhoId++,
                    PetId = pet.Id,
                    Servico = Servicos[pet.Id % Servicos.Length],
                    Data = new DateTime(2026, 3, 1).AddDays(pet.Id % 120),
                    Valor = 45m + (pet.Id % 4) * 15m
                });
            }

            if (pet.Id % 4 == 0)
            {
                Vacinas.Add(new Vacina
                {
                    Id = _proximaVacinaId++,
                    PetId = pet.Id,
                    Nome = "V10",
                    Aplicada = new DateTime(2025, 6, 1).AddDays(pet.Id % 300)
                });
            }
        }

        TabelaDePrecos.AddRange(new[]
        {
            new ItemDePreco { Servico = "banho", Valor = 60m },
            new ItemDePreco { Servico = "tosa higienica", Valor = 70m },
            new ItemDePreco { Servico = "tosa completa", Valor = 95m },
            new ItemDePreco { Servico = "hidratacao", Valor = 45m },
            new ItemDePreco { Servico = "consulta", Valor = 140m },
            new ItemDePreco { Servico = "vacina V10", Valor = 110m }
        });
    }

    public Pet? BuscarPet(int id)
    {
        lock (_trava) return Pets.FirstOrDefault(p => p.Id == id);
    }

    public Pet CriarPet(Pet pet)
    {
        lock (_trava)
        {
            pet.Id = _proximoPetId++;
            Pets.Add(pet);
            return pet;
        }
    }

    public bool RemoverPet(int id)
    {
        lock (_trava)
        {
            var pet = Pets.FirstOrDefault(p => p.Id == id);
            if (pet is null) return false;

            Pets.Remove(pet);
            return true;
        }
    }

    public Vacina RegistrarVacina(int petId, string nome)
    {
        lock (_trava)
        {
            var vacina = new Vacina
            {
                Id = _proximaVacinaId++,
                PetId = petId,
                Nome = nome,
                Aplicada = DateTime.UtcNow
            };
            Vacinas.Add(vacina);
            return vacina;
        }
    }

    public List<Vacina> VacinasDoPet(int petId)
    {
        lock (_trava) return Vacinas.Where(v => v.PetId == petId).OrderBy(v => v.Id).ToList();
    }
}
