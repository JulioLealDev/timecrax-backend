# Database Seeding

Este projeto possui um sistema de seeding automático que popula o banco de dados com dados iniciais e de teste.

## O que é criado automaticamente?

### 1. Dados Estruturais (Sempre)
Executado sempre que a aplicação inicia:
- **16 Achievements**: Conquistas do jogo
- **4 Medals**: Medalhas baseadas em score (Aprendiz, Bacharel, Mestre, Doutor)

### 2. Dados de Teste (Apenas se o banco estiver vazio)
Executado automaticamente apenas se não houver usuários no banco:
- **75 usuários** com:
  - Nomes brasileiros variados
  - Scores entre 800 e 3200 pontos
  - 70% students, 30% teachers
  - Senha padrão: `Test123!`

- **20 temas históricos completos**:
  - Revolução Francesa, Segunda Guerra Mundial, etc.
  - Cada tema com 15 cartas de eventos
  - Todos marcados como `ReadyToPlay = true`

- **300 cartas de eventos** (15 por tema):
  - Cada carta com 4 tipos de quiz:
    - Image Quiz (4 opções de imagem)
    - Text Quiz (4 opções de texto)
    - True/False Quiz
    - Correlation Quiz (3 pares imagem-texto)
  - Anos variados (500 AC a 2000 DC)
  - Imagens via Picsum (placeholder)

## Como funciona?

O seeding é executado automaticamente quando a aplicação inicia através do `DbSeedHostedService`.

### Para resetar o banco e criar novos dados de teste:

1. **Apagar o banco de dados**:
```bash
# No diretório do projeto backend
dotnet ef database drop
```

2. **Recriar as migrations**:
```bash
dotnet ef database update
```

3. **Iniciar a aplicação**:
```bash
dotnet run
```

O seeder irá detectar que não há dados e criará tudo automaticamente.

## Usuários de Teste

Todos os usuários de teste possuem:
- **Email**: formato `nome.sobrenome{número}@test.com`
- **Senha**: `Test123!`
- **Exemplos**:
  - `joao.silva123@test.com`
  - `maria.santos456@test.com`

## Testando o Ranking

Com 75 usuários criados, você pode testar:
- ✅ Paginação (50 itens por página)
- ✅ Cores especiais (gold, silver, bronze)
- ✅ Ordenação por score
- ✅ Indicador do usuário logado

## Estrutura do Código

O código de seeding está em:
```
Timecrax.Api/
  Data/
    Seed/
      DbSeeder.cs              # Lógica de seeding
      DbSeederHostedService.cs # Execução automática na inicialização
```

## Customização

Para alterar a quantidade de dados de teste, edite em `DbSeeder.cs`:
```csharp
// Linha ~33
var users = CreateTestUsers(75);  // Altere o número aqui

// Linha ~39
var themes = CreateTestThemes(20, users);  // Altere o número aqui

// Linha ~47
var cards = CreateTestEventCards(theme, 15);  // Cartas por tema
```

## Ambiente de Produção

⚠️ **IMPORTANTE**: O seeding de dados de teste só ocorre se o banco estiver vazio. Em produção, apenas Achievements e Medals serão criados na primeira execução.

## Troubleshooting

### Erro: "Table already exists"
O banco já possui dados. Se quiser resetar:
```bash
dotnet ef database drop
dotnet ef database update
```

### Seeder não executou
Verifique os logs da aplicação. O seeder imprime mensagens de progresso:
```
Starting test data seed...
✓ Created 75 test users
✓ Created 20 test themes
✓ Created event cards with quizzes
Test data seed completed!
```

### Dados duplicados
O seeder é **idempotente**: só cria dados se eles não existirem. Se houver usuários no banco, não criará mais.
