# Guia de Migração - MS Ordens

## Resumo das Mudanças

A estrutura do projeto foi completamente reorganizada de:
```
src/
├── API/
├── Core/           → Removido
└── Infraestrutura/ → Removido
```

Para:
```
src/
├── API/            → Atualizado (usa novas camadas)
├── Domain/         → NOVO
├── Application/    → NOVO
└── Infrastructure/ → NOVO
```

## Passos para Migração

### 1. Remover Pastas Antigas

Após confirmar que tudo compila, remova as pastas antigas:

```powershell
# Executar no diretório raiz do projeto
Remove-Item -Recurse -Force .\src\Core
Remove-Item -Recurse -Force .\src\Infraestrutura
Remove-Item -Recurse -Force .\src\Ordens.Domain      # pasta vazia criada anteriormente
Remove-Item -Recurse -Force .\src\Ordens.Application # pasta vazia criada anteriormente
Remove-Item -Recurse -Force .\src\Ordens.Infrastructure # pasta vazia criada anteriormente
Remove-Item -Recurse -Force .\src\Ordens.API         # pasta vazia criada anteriormente
```

### 2. Verificar Compilação

```powershell
cd c:\Users\user\source\repos\ms-ordens
dotnet build -v d
```

### 3. Atualizar Testes

Os projetos de teste precisam ser atualizados para referenciar as novas camadas:

**tests/UnitTests/UnitTests.csproj:**
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\Domain\Domain.csproj" />
  <ProjectReference Include="..\..\src\Application\Application.csproj" />
  <ProjectReference Include="..\..\src\Infrastructure\Infrastructure.csproj" />
</ItemGroup>
```

**tests/BDD.Tests/BDD.Tests.csproj:**
```xml
<ItemGroup>
  <ProjectReference Include="..\..\src\API\API.csproj" />
</ItemGroup>
```

### 4. Atualizar Namespaces nos Testes

Os namespaces mudaram de:
- `Core.*` → `Domain.*` ou `Application.*`
- `Infraestrutura.*` → `Infrastructure.*`

Use busca e substituição:
```
Core.Entidades       → Domain.Entities
Core.Enumeradores    → Domain.Enums
Core.DTOs            → Application.DTOs
Core.UseCases        → Application.Commands / Application.Queries
Core.Interfaces      → Domain.Interfaces / Application.Common.Interfaces
Infraestrutura.*     → Infrastructure.*
```

### 5. Executar Testes

```powershell
dotnet test --logger "console;verbosity=detailed"
```

## Mapeamento de Arquivos

| Arquivo Antigo | Novo Local |
|----------------|------------|
| `Core/Entidades/*` | `Domain/Entities/*` |
| `Core/Enumeradores/*` | `Domain/Enums/*` |
| `Core/Interfaces/Repositories/*` | `Domain/Interfaces/*` |
| `Core/Interfaces/Services/*` | `Application/Common/Interfaces/*` |
| `Core/DTOs/*` | `Application/DTOs/*` (ou `API/DTOs/*`) |
| `Core/UseCases/*/Commands/*` | `Application/Commands/*` |
| `Core/UseCases/*/Queries/*` | `Application/Queries/*` |
| `Core/Eventos/*` | `Application/IntegrationEvents/*` |
| `Core/Validators/*` | `Application/Commands/*/Validator.cs` |
| `Infraestrutura/Dados/*` | `Infrastructure/Persistence/*` |
| `Infraestrutura/Mensageria/*` | `Infrastructure/Messaging/*` |
| `Infraestrutura/Servicos/*` | `Infrastructure/ExternalServices/*` |

## Banco de Dados

**Não há mudanças no banco de dados.** As configurações do EF Core foram preservadas.

## Configurações

O `appsettings.json` permanece inalterado.

## Rollback

Se precisar fazer rollback, restaure os arquivos do git:
```powershell
git checkout HEAD -- src/
```
