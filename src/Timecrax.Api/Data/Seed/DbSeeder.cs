using Microsoft.EntityFrameworkCore;
using Timecrax.Api.Data;
using Timecrax.Api.Domain.Entities;
using Timecrax.Api.Domain.Enums;
using Timecrax.Api.Services;

namespace Timecrax.Api.Data.Seed;

public static class DbSeeder
{
    private static readonly Random Random = new();

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // 1) Achievements e Medals (sempre)
        await SeedAchievementsAsync(db, ct);
        await SeedMedalsAsync(db, ct);

        // 2) Test Data (apenas se não houver usuários)
        if (!await db.Users.AnyAsync(ct))
        {
            await SeedTestDataAsync(db, ct);
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedTestDataAsync(AppDbContext db, CancellationToken ct)
    {
        Console.WriteLine("Starting test data seed...");

        // Criar 75 usuários
        var users = CreateTestUsers(75);
        db.Users.AddRange(users);
        await db.SaveChangesAsync(ct);
        Console.WriteLine("✓ Created 75 test users");

        // Criar 20 temas completos (15 cartas cada)
        var themes = CreateTestThemes(20, users);
        db.Themes.AddRange(themes);
        await db.SaveChangesAsync(ct);
        Console.WriteLine("✓ Created 20 test themes");

        // Criar cartas para cada tema
        foreach (var theme in themes)
        {
            var cards = CreateTestEventCards(theme, 15);
            db.EventCards.AddRange(cards);
        }
        await db.SaveChangesAsync(ct);
        Console.WriteLine("✓ Created event cards with quizzes");
        Console.WriteLine("Test data seed completed!");
    }

    private static async Task SeedMedalsAsync(AppDbContext db, CancellationToken ct)
    {
        // Se já existe ao menos 1, não replique (idempotente)
        var any = await db.Medals.AnyAsync(ct);
        if (any) return;

        var now = DateTimeOffset.UtcNow;

        var list = new List<Medal>
        {
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz",
                Image = "http://localhost:5139/media/assets/medals/medal_01.png",
                MinScore = 0,
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Bacharel",
                Image = "http://localhost:5139/media/assets/medals/medal_02.png",
                MinScore = 1000,
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Mestre",
                Image = "http://localhost:5139/media/assets/medals/medal_03.png",
                MinScore = 2000,
                CreatedAt = now
            },
            new Medal
            {
                Id = Guid.NewGuid(),
                Name = "Doutor",
                Image = "http://localhost:5139/media/assets/medals/medal_04.png",
                MinScore = 3000,
                CreatedAt = now
            },
        };

        db.Medals.AddRange(list);
    }

    private static async Task SeedAchievementsAsync(AppDbContext db, CancellationToken ct)
    {
        // Se já existe ao menos 1, não replique (idempotente)
        var any = await db.Achievements.AnyAsync(ct);
        if (any) return;

        var now = DateTimeOffset.UtcNow;

        var list = new List<Achievement>
        {
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Primeira Vitória",
                Image = "http://localhost:5139/media/assets/achievements/ach_1.png",
                Description = "Complete um tema pela primeira vez.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Aprendiz Persistente",
                Image = "http://localhost:5139/media/assets/achievements/ach_2.png",
                Description = "Complete 5 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mestre do Passado",
                Image = "http://localhost:5139/media/assets/achievements/ach_3.png",
                Description = "Complete 10 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Expert da História",
                Image = "http://localhost:5139/media/assets/achievements/ach_4.png",
                Description = "Complete 20 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Senhor do Tempo",
                Image = "http://localhost:5139/media/assets/achievements/ach_5.png",
                Description = "Complete 50 temas diferentes.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Memória Afiada",
                Image = "http://localhost:5139/media/assets/achievements/ach_6.png",
                Description = "Acerte 3 quizzes seguidos.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Mente Magnífica",
                Image = "http://localhost:5139/media/assets/achievements/ach_7.png",
                Description = "Acerte 5 quizzes seguidos.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Enciclopédia Humana",
                Image = "http://localhost:5139/media/assets/achievements/ach_8.png",
                Description = "Acerte 10 quizzes seguidos.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Sorte de Principiante",
                Image = "http://localhost:5139/media/assets/achievements/ach_9.png",
                Description = "Complete 1 tema jogando sozinho.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Guerreiro Solitário",
                Image = "http://localhost:5139/media/assets/achievements/ach_10.png",
                Description = "Complete 5 temas jogando sozinho.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Exército de um homem só",
                Image = "http://localhost:5139/media/assets/achievements/ach_11.png",
                Description = "Complete 10 temas jogando sozinho.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Juntos venceremos",
                Image = "http://localhost:5139/media/assets/achievements/ach_12.png",
                Description = "Complete 1 tema jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "A união faz a força!",
                Image = "http://localhost:5139/media/assets/achievements/ach_13.png",
                Description = "Complete 5 temas jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Legião do saber",
                Image = "http://localhost:5139/media/assets/achievements/ach_14.png",
                Description = "Complete 10 temas jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Reis da História",
                Image = "http://localhost:5139/media/assets/achievements/ach_15.png",
                Description = "Complete 20 temas jogando em grupo.",
                CreatedAt = now
            },
            new Achievement
            {
                Id = Guid.NewGuid(),
                Name = "Ancioês do Tempo",
                Image = "http://localhost:5139/media/assets/achievements/ach_16.png",
                Description = "Complete 50 temas jogando em grupo.",
                CreatedAt = now
            }
        };

        db.Achievements.AddRange(list);
    }

    // ========== TEST DATA HELPERS ==========

    private static readonly string[] FirstNames =
    {
        "João", "Maria", "Pedro", "Ana", "Lucas", "Julia", "Gabriel", "Beatriz",
        "Rafael", "Larissa", "Felipe", "Camila", "Bruno", "Amanda", "Thiago", "Carolina",
        "Diego", "Fernanda", "Matheus", "Juliana", "Rodrigo", "Mariana", "Vitor", "Patricia",
        "Carlos", "Aline", "Daniel", "Leticia", "Fernando", "Gabriela", "Gustavo", "Bruna",
        "Marcelo", "Renata", "Leonardo", "Tatiane", "Ricardo", "Vanessa", "André", "Cristina",
        "Paulo", "Sandra", "Roberto", "Monica", "José", "Claudia", "Antonio", "Silvia",
        "Marcos", "Luciana", "Eduardo", "Adriana", "Alexandre", "Rosangela", "Fabio", "Regina"
    };

    private static readonly string[] LastNames =
    {
        "Silva", "Santos", "Oliveira", "Souza", "Rodrigues", "Ferreira", "Alves", "Pereira",
        "Lima", "Gomes", "Costa", "Ribeiro", "Martins", "Carvalho", "Rocha", "Almeida",
        "Nascimento", "Barbosa", "Cardoso", "Dias", "Fernandes", "Araujo", "Castro", "Cavalcante"
    };

    private static readonly string[] ThemeNames =
    {
        "Revolução Francesa",
        "Segunda Guerra Mundial",
        "Renascimento Italiano",
        "Império Romano",
        "Descobrimento do Brasil",
        "Revolução Industrial",
        "Guerra Fria",
        "Idade Média",
        "Civilização Egípcia",
        "Grécia Antiga",
        "Independência Americana",
        "Era Napoleônica",
        "Primeira Guerra Mundial",
        "Revolução Russa",
        "Expansão Marítima",
        "Reforma Protestante",
        "Iluminismo",
        "Absolutismo Europeu",
        "Feudalismo",
        "Civilizações Pré-Colombianas"
    };

    private static readonly string[] EventTitles =
    {
        "Batalha Decisiva", "Tratado de Paz", "Descoberta Importante", "Revolução Popular",
        "Coroação Imperial", "Queda do Império", "Invenção Revolucionária", "Conquista Territorial",
        "Reforma Política", "Construção Monumental", "Declaração Histórica", "Aliança Estratégica",
        "Revolução Cultural", "Expedição Marítima", "Fundação da Cidade"
    };

    private static List<User> CreateTestUsers(int count)
    {
        var users = new List<User>();
        var usedEmails = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            string email;
            string firstName = FirstNames[Random.Next(FirstNames.Length)];
            string lastName = LastNames[Random.Next(LastNames.Length)];

            do
            {
                email = $"{firstName.ToLower()}.{lastName.ToLower()}{Random.Next(1, 999)}@test.com";
            } while (usedEmails.Contains(email));

            usedEmails.Add(email);

            var role = Random.Next(10) < 3 ? "teacher" : "student"; // 30% teachers, 70% students

            users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                PasswordHash = PasswordService.Hash("Test123!"),
                Role = role,
                Score = Random.Next(800, 3201), // Entre 800 e 3200
                SchoolName = role == "teacher" ? $"Escola {Random.Next(1, 21)}" : null,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(1, 365)),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(0, 30))
            });
        }

        return users;
    }

    private static List<Theme> CreateTestThemes(int count, List<User> users)
    {
        var themes = new List<Theme>();
        var teachers = users.Where(u => u.Role == "teacher").ToList();

        // Se não houver teachers, usar alguns users aleatórios
        if (!teachers.Any())
        {
            teachers = users.Take(5).ToList();
        }

        for (int i = 0; i < count; i++)
        {
            var creator = teachers[Random.Next(teachers.Count)];

            themes.Add(new Theme
            {
                Id = Guid.NewGuid(),
                Name = ThemeNames[i],
                Image = $"https://picsum.photos/seed/theme{i}/400/300",
                ReadyToPlay = true, // Todos prontos para jogar
                CreatorUserId = creator.Id,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(30, 180)),
                UpdatedAt = DateTimeOffset.UtcNow.AddDays(-Random.Next(0, 30))
            });
        }

        return themes;
    }

    private static List<EventCard> CreateTestEventCards(Theme theme, int count)
    {
        var cards = new List<EventCard>();

        for (int i = 0; i < count; i++)
        {
            var year = Random.Next(500, 2000);
            var era = year < 1000 ? Era.AC : Era.DC;

            var card = new EventCard
            {
                Id = Guid.NewGuid(),
                ThemeId = theme.Id,
                Title = $"{EventTitles[Random.Next(EventTitles.Length)]} {i + 1}",
                Image = $"https://picsum.photos/seed/card{theme.Id}{i}/600/400",
                Year = year < 1000 ? 1000 - year : year,
                Era = era,
                OrderIndex = i,
                CreatedAt = theme.CreatedAt.AddHours(i),

                // Image Quiz
                ImageQuiz = new ImageQuiz
                {
                    Id = Guid.NewGuid(),
                    Question = "Qual imagem representa melhor este evento histórico?",
                    Image1 = $"https://picsum.photos/seed/img1{theme.Id}{i}/300/200",
                    Image2 = $"https://picsum.photos/seed/img2{theme.Id}{i}/300/200",
                    Image3 = $"https://picsum.photos/seed/img3{theme.Id}{i}/300/200",
                    Image4 = $"https://picsum.photos/seed/img4{theme.Id}{i}/300/200",
                    CorrectImageIndex = (short)Random.Next(0, 4)
                },

                // Text Quiz
                TextQuiz = new TextQuiz
                {
                    Id = Guid.NewGuid(),
                    Question = "Qual foi a principal consequência deste evento?",
                    Text1 = "Mudança política significativa",
                    Text2 = "Revolução tecnológica",
                    Text3 = "Expansão territorial",
                    Text4 = "Reforma social",
                    CorrectTextIndex = (short)Random.Next(0, 4)
                },

                // True or False Quiz
                TrueOrFalseQuiz = new TrueOrFalseQuiz
                {
                    Id = Guid.NewGuid(),
                    Text = "Este evento marcou o início de uma nova era histórica.",
                    IsTrue = Random.Next(2) == 1
                },

                // Correlation Quiz
                CorrelationQuiz = new CorrelationQuiz
                {
                    Id = Guid.NewGuid(),
                    Image1 = $"https://picsum.photos/seed/cor1{theme.Id}{i}/250/200",
                    Image2 = $"https://picsum.photos/seed/cor2{theme.Id}{i}/250/200",
                    Image3 = $"https://picsum.photos/seed/cor3{theme.Id}{i}/250/200",
                    Text1 = "Líder histórico do período",
                    Text2 = "Local onde ocorreu o evento",
                    Text3 = "Documento importante relacionado"
                }
            };

            cards.Add(card);
        }

        return cards;
    }
}
