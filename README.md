# 💰 FinanceTracker

Sistema completo de controle financeiro pessoal desenvolvido com .NET 9 e Angular, seguindo as melhores práticas de arquitetura limpa e Domain-Driven Design (DDD).

## 📋 Índice

- [Sobre o Projeto](#sobre-o-projeto)
- [Tecnologias](#tecnologias)
- [Arquitetura](#arquitetura)
- [Funcionalidades](#funcionalidades)
- [Pré-requisitos](#pré-requisitos)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Execução](#execução)
- [API Endpoints](#api-endpoints)
- [Testes](#testes)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Contribuição](#contribuição)
- [Licença](#licença)

## 📖 Sobre o Projeto

O FinanceTracker é um sistema web moderno para gerenciamento de finanças pessoais que permite:

- 📊 Controle completo de receitas e despesas
- 📈 Dashboard com análises e gráficos
- 🏷️ Categorização inteligente de transações
- 📱 Interface responsiva e intuitiva
- 🔍 Relatórios detalhados e filtros avançados
- 💹 Projeções e tendências financeiras

## 🚀 Tecnologias

### Backend (.NET 9)
- **ASP.NET Core 9** - Framework web
- **Entity Framework Core** - ORM
- **PostgreSQL** - Banco de dados
- **Mapster** - Mapeamento de objetos
- **FluentValidation** - Validações
- **Swagger/OpenAPI** - Documentação da API
- **xUnit** - Testes unitários
- **Moq** - Mocking para testes

### Frontend (Angular - Planejado)
- **Angular 18+** - Framework frontend
- **TypeScript** - Linguagem
- **Angular Material** - UI Components
- **Chart.js** - Gráficos e visualizações
- **RxJS** - Programação reativa

### DevOps & Ferramentas
- **Docker** - Containerização
- **GitHub Actions** - CI/CD
- **SonarQube** - Análise de código
- **Serilog** - Logging estruturado

## 🏗️ Arquitetura

O projeto segue uma arquitetura em camadas baseada em DDD (Domain-Driven Design):

```
┌─────────────────┐
│   Presentation  │  ← Controllers, DTOs
├─────────────────┤
│   Application   │  ← Services, Interfaces, Mappings
├─────────────────┤
│   Domain        │  ← Entities, Value Objects, Rules
├─────────────────┤
│ Infrastructure  │  ← Repositories, DbContext, External
└─────────────────┘
```

### Princípios Aplicados:
- **Domain-Driven Design (DDD)**
- **Clean Architecture**
- **SOLID Principles**
- **Repository + Unit of Work Pattern**
- **Dependency Injection**
- **Test-Driven Development (TDD)**

## ✨ Funcionalidades

### 🏷️ Gerenciamento de Categorias
- Categorias pré-definidas (Alimentação, Saúde, Transporte, etc.)
- Criação de categorias personalizadas
- Diferenciação entre receitas e despesas
- Validação de integridade referencial

### 💸 Controle de Transações
- Registro de receitas e despesas
- Categorização automática
- Filtros avançados (data, categoria, valor, descrição)
- Paginação e busca textual
- Validações de negócio robustas

### 📊 Dashboard e Relatórios
- Resumo financeiro mensal e anual
- Gráficos de tendências
- Top categorias por gastos
- Comparações período a período
- Projeções futuras baseadas em histórico
- Transações recentes

### 🔍 Recursos Avançados
- Balanço automático (receitas - despesas)
- Estatísticas por categoria
- Filtros temporais flexíveis
- Exportação de dados (planejado)
- Múltiplas moedas (planejado)

## 📋 Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/get-started) (opcional)
- [Node.js 18+](https://nodejs.org/) (para o frontend)
- [Angular CLI](https://angular.io/cli) (para o frontend)

## 🛠️ Instalação

### 1. Clone o repositório
```bash
git clone https://github.com/seu-usuario/FinanceTracker.git
cd FinanceTracker
```

### 2. Configurar PostgreSQL

#### Opção A: Docker (Recomendado)
```bash
docker run --name finance-postgres \
  -e POSTGRES_DB=FinanceTracker \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=123456 \
  -p 5432:5432 \
  -d postgres:15
```

#### Opção B: Instalação Local
- Instale PostgreSQL
- Crie banco de dados: `FinanceTracker`
- Configure usuário e senha

### 3. Restaurar pacotes .NET
```bash
dotnet restore
```

### 4. Configurar connection string
Edite `src/FinanceTracker.API/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FinanceTracker;Username=postgres;Password=123456"
  }
}
```

### 5. Executar migrations
```bash
cd src
dotnet ef database update -p FinanceTracker.Infrastructure -s FinanceTracker.API
```

## 🚀 Execução

### Backend (API)
```bash
cd src/FinanceTracker.API
dotnet run
```

A API estará disponível em:
- HTTPS: `https://localhost:7123`
- HTTP: `http://localhost:5123`
- Swagger: `https://localhost:7123/swagger`

### Frontend (Planejado)
```bash
cd frontend
npm install
ng serve
```

## 📚 API Endpoints

### Categories
```http
GET    /api/categories              # Listar todas
GET    /api/categories/{id}         # Buscar por ID
GET    /api/categories/summaries    # Resumos (dropdowns)
GET    /api/categories/expenses     # Categorias de despesa
GET    /api/categories/incomes      # Categorias de receita
POST   /api/categories              # Criar categoria
PUT    /api/categories/{id}         # Atualizar categoria
DELETE /api/categories/{id}         # Excluir categoria
GET    /api/categories/stats        # Estatísticas
```

### Transactions (Planejado)
```http
GET    /api/transactions            # Listar com paginação
GET    /api/transactions/{id}       # Buscar por ID
POST   /api/transactions            # Criar transação
PUT    /api/transactions/{id}       # Atualizar transação
DELETE /api/transactions/{id}       # Excluir transação
GET    /api/transactions/search     # Busca textual
GET    /api/transactions/stats      # Estatísticas
```

### Dashboard (Planejado)
```http
GET    /api/dashboard               # Dashboard completo
GET    /api/dashboard/summary       # Resumo financeiro
GET    /api/dashboard/trends        # Tendências mensais
GET    /api/dashboard/projections   # Projeções futuras
```

### Exemplos de Request/Response

#### Criar Categoria
```http
POST /api/categories
Content-Type: application/json

{
  "name": "Supermercado",
  "categoryType": "Food"
}
```

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Supermercado",
  "categoryType": "Food",
  "displayName": "Alimentação",
  "isExpenseCategory": true,
  "isIncomeCategory": false,
  "transactionType": "Expense",
  "createdAt": "2024-03-15T10:30:00Z"
}
```

## 🧪 Testes

### Executar todos os testes
```bash
dotnet test
```

### Testes por projeto
```bash
# Testes de domínio
dotnet test tests/FinanceTracker.Domain.Tests

# Testes de aplicação
dotnet test tests/FinanceTracker.Application.Tests

# Testes de API
dotnet test tests/FinanceTracker.API.Tests
```

### Coverage Report
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Tipos de Testes
- **Unitários**: Value Objects, Entities, Services
- **Integração**: Repositories, Database
- **API**: Controllers, Endpoints (planejado)
- **E2E**: Fluxos completos (planejado)

## 📁 Estrutura do Projeto

```
FinanceTracker/
├── 📁 src/
│   ├── 🎯 FinanceTracker.Domain/           # Regras de negócio
│   │   ├── Entities/                       # Entidades (Category, Transaction)
│   │   ├── ValueObjects/                   # Value Objects (Money, Enums)
│   │   ├── Interfaces/                     # Contratos do domínio
│   │   └── Exceptions/                     # Exceções de domínio
│   │
│   ├── 🔧 FinanceTracker.Application/      # Casos de uso
│   │   ├── DTOs/                          # Data Transfer Objects
│   │   ├── Services/                      # Serviços de aplicação
│   │   ├── Mappings/                      # Mapeamentos (Mapster)
│   │   └── Validators/                    # Validações (FluentValidation)
│   │
│   ├── 🗄️ FinanceTracker.Infrastructure/   # Acesso a dados
│   │   ├── Data/                          # DbContext, Configurations
│   │   ├── Repositories/                  # Implementações dos repositórios
│   │   └── DependencyInjection.cs        # Configuração de DI
│   │
│   └── 🌐 FinanceTracker.API/              # Camada de apresentação
│       ├── Controllers/                   # Controllers da API
│       ├── Middlewares/                   # Middlewares customizados
│       └── Program.cs                     # Configuração da aplicação
│
├── 📁 tests/                              # Testes
│   ├── FinanceTracker.Domain.Tests/      # Testes de domínio
│   ├── FinanceTracker.Application.Tests/ # Testes de aplicação
│   └── FinanceTracker.API.Tests/         # Testes de API
│
├── 📁 frontend/ (Planejado)               # Aplicação Angular
├── 📁 docs/                              # Documentação adicional
├── 🐳 docker-compose.yml                 # Orquestração Docker
├── 📋 FinanceTracker.sln                 # Solution .NET
└── 📖 README.md                          # Este arquivo
```

## 🔧 Configurações

### Environment Variables
```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Database=FinanceTracker;Username=postgres;Password=123456"

# Logging
Logging__LogLevel__Default="Information"
Logging__LogLevel__Microsoft="Warning"

# CORS (Development)
CORS__AllowedOrigins="http://localhost:4200"
```

### Docker Compose (Planejado)
```yaml
version: '3.8'
services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_DB: FinanceTracker
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: 123456
    ports:
      - "5432:5432"
  
  api:
    build: .
    ports:
      - "5000:5000"
    depends_on:
      - postgres
  
  frontend:
    build: ./frontend
    ports:
      - "4200:4200"
    depends_on:
      - api
```

## 📊 Monitoramento

### Health Checks
- Endpoint: `/health`
- Verifica conectividade com PostgreSQL
- Status da aplicação

### Logging
- **Serilog** para logging estruturado
- Logs em console (desenvolvimento)
- Logs em arquivo (produção)
- Integração com observability tools

### Métricas (Planejado)
- Prometheus + Grafana
- Application Insights
- Performance counters

## 🚦 Roadmap

### ✅ Concluído
- [x] Arquitetura base do projeto
- [x] Domain Layer completa
- [x] Application Services
- [x] Infrastructure com PostgreSQL
- [x] Categories API
- [x] Testes unitários (Domain + Application)
- [x] Documentação Swagger

### 🔄 Em Progresso
- [ ] Transactions Controller
- [ ] Dashboard Controller
- [ ] Middleware de tratamento de erros
- [ ] Testes de integração

### 📅 Próximas Funcionalidades
- [ ] Frontend Angular completo
- [ ] Autenticação e autorização
- [ ] Exportação de relatórios (PDF/Excel)
- [ ] Notificações e alertas
- [ ] Metas e orçamentos
- [ ] Suporte a múltiplas moedas
- [ ] Importação de extratos bancários
- [ ] App mobile (React Native/Flutter)
- [ ] API versioning
- [ ] Cache distribuído (Redis)
- [ ] Message queues (RabbitMQ)