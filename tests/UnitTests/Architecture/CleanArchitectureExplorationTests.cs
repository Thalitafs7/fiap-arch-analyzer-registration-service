using System.Reflection;
using Domain.Entities;
using Domain.Entities.Base;

namespace UnitTests.Architecture;

/// <summary>
/// Bug Condition Exploration Tests - Clean Architecture Violations Detection.
/// These tests encode the EXPECTED (fixed) behavior.
/// On UNFIXED code, they MUST FAIL — failure confirms the bugs exist.
/// 
/// **Validates: Requirements 1.1, 1.2, 1.5, 1.6, 1.7, 1.12, 1.13, 1.17, 1.18**
/// </summary>
public class CleanArchitectureExplorationTests
{
    #region Property 1: Domain Encapsulation - Entities must NOT have public setters

    /// <summary>
    /// Bug Condition: Analise.Nome has public setter allowing unvalidated mutation.
    /// Expected (after fix): Nome has private set, changes via AlterarNome() method.
    /// Counterexample on unfixed code: Analise.Nome.SetMethod.IsPublic == true
    /// </summary>
    [Fact]
    public void Analise_Nome_Should_Not_Have_Public_Setter()
    {
        // Arrange
        var property = typeof(Analise).GetProperty("Nome");

        // Act
        var setMethod = property!.SetMethod;

        // Assert - Expected behavior: setter is NOT public
        setMethod.Should().NotBeNull("property should have a setter");
        setMethod!.IsPublic.Should().BeFalse(
            "Analise.Nome should have private set to enforce encapsulation. " +
            "State changes must go through validated business methods.");
    }

    /// <summary>
    /// Bug Condition: Analise.Status has public setter with string type.
    /// Expected (after fix): Status uses enum with private set, changes via AtualizarStatus().
    /// Counterexample on unfixed code: Analise.Status.SetMethod.IsPublic == true
    /// </summary>
    [Fact]
    public void Analise_Status_Should_Not_Have_Public_Setter()
    {
        // Arrange
        var property = typeof(Analise).GetProperty("Status");

        // Act
        var setMethod = property!.SetMethod;

        // Assert
        setMethod.Should().NotBeNull("property should have a setter");
        setMethod!.IsPublic.Should().BeFalse(
            "Analise.Status should have private set to enforce domain invariants. " +
            "Status transitions must be validated via AtualizarStatus() method.");
    }

    /// <summary>
    /// Bug Condition: Entity.Ativo has public setter bypassing Desativar()/Ativar() methods.
    /// Expected (after fix): Ativo has protected set, forcing use of business methods.
    /// Counterexample on unfixed code: Entity.Ativo.SetMethod.IsPublic == true
    /// </summary>
    [Fact]
    public void Entity_Ativo_Should_Not_Have_Public_Setter()
    {
        // Arrange
        var property = typeof(Entity).GetProperty("Ativo");

        // Act
        var setMethod = property!.SetMethod;

        // Assert
        setMethod.Should().NotBeNull("property should have a setter");
        setMethod!.IsPublic.Should().BeFalse(
            "Entity.Ativo should have protected set. " +
            "Activation/deactivation must go through Desativar()/Ativar() methods.");
    }

    /// <summary>
    /// Bug Condition: Entity.DataAtualizacao has public setter.
    /// Expected (after fix): DataAtualizacao has protected set, managed internally.
    /// </summary>
    [Fact]
    public void Entity_DataAtualizacao_Should_Not_Have_Public_Setter()
    {
        // Arrange
        var property = typeof(Entity).GetProperty("DataAtualizacao");

        // Act
        var setMethod = property!.SetMethod;

        // Assert
        setMethod.Should().NotBeNull("property should have a setter");
        setMethod!.IsPublic.Should().BeFalse(
            "Entity.DataAtualizacao should have protected set. " +
            "Timestamp updates must be managed internally via AtualizarDataModificacao().");
    }

    #endregion

    #region Property 2: Layer Separation - Application must NOT reference ASP.NET Core HTTP

    /// <summary>
    /// Bug Condition: Application assembly references Microsoft.AspNetCore.Http.
    /// Expected (after fix): Application uses own FileData abstraction, zero HTTP references.
    /// Counterexample on unfixed code: Application assembly has reference to Microsoft.AspNetCore.Http.Features
    /// </summary>
    [Fact]
    public void Application_Assembly_Should_Not_Reference_AspNetCore_Http()
    {
        // Arrange
        var applicationAssembly = typeof(Application.Commands.CriarAnalise.CriarAnaliseCommand).Assembly;

        // Act
        var referencedAssemblies = applicationAssembly.GetReferencedAssemblies();
        var aspNetCoreHttpReferences = referencedAssemblies
            .Where(a => a.Name != null && a.Name.Contains("Microsoft.AspNetCore.Http", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Assert - Expected behavior: NO references to ASP.NET Core HTTP
        aspNetCoreHttpReferences.Should().BeEmpty(
            "Application layer must not reference Microsoft.AspNetCore.Http. " +
            "Use domain abstractions (FileData) instead of IFormFile to maintain layer separation.");
    }

    /// <summary>
    /// Bug Condition: CriarAnaliseCommand uses IFormFile from Microsoft.AspNetCore.Http.
    /// Expected (after fix): Command uses FileData record.
    /// </summary>
    [Fact]
    public void CriarAnaliseCommand_Should_Not_Use_IFormFile()
    {
        // Arrange
        var commandType = typeof(Application.Commands.CriarAnalise.CriarAnaliseCommand);

        // Act
        var properties = commandType.GetProperties();
        var usesIFormFile = properties.Any(p =>
            p.PropertyType.FullName != null &&
            p.PropertyType.FullName.Contains("IFormFile", StringComparison.OrdinalIgnoreCase));

        // Also check constructor parameters (records use constructor)
        var constructors = commandType.GetConstructors();
        var ctorUsesIFormFile = constructors.Any(c =>
            c.GetParameters().Any(param =>
                param.ParameterType.FullName != null &&
                param.ParameterType.FullName.Contains("IFormFile", StringComparison.OrdinalIgnoreCase)));

        // Assert
        usesIFormFile.Should().BeFalse(
            "CriarAnaliseCommand should not use IFormFile. Use FileData abstraction instead.");
        ctorUsesIFormFile.Should().BeFalse(
            "CriarAnaliseCommand constructor should not reference IFormFile.");
    }

    #endregion

    #region Property 3: CQRS Compliance - Queries must NOT perform mutations

    /// <summary>
    /// Bug Condition: UpdateAnaliseServicoHandler in Queries namespace calls _analiseRepository.Atualizar().
    /// Expected (after fix): No handler in Queries namespace calls mutation methods.
    /// Counterexample on unfixed code: UpdateAnaliseServicoHandler calls Atualizar() in Queries namespace.
    /// </summary>
    [Fact]
    public void Queries_Namespace_Should_Not_Contain_Mutation_Handlers()
    {
        // Arrange
        var applicationAssembly = typeof(Application.Commands.CriarAnalise.CriarAnaliseCommand).Assembly;

        // Act - Find all types in Queries namespace
        var queryTypes = applicationAssembly.GetTypes()
            .Where(t => t.Namespace != null && t.Namespace.Contains(".Queries.", StringComparison.Ordinal))
            .Where(t => t.Name.EndsWith("Handler", StringComparison.Ordinal))
            .ToList();

        // Check if any handler in Queries namespace has methods that call repository mutation methods
        var mutationMethods = new[] { "Atualizar", "Adicionar", "Remover", "Deletar" };
        var violatingHandlers = new List<string>();

        foreach (var handlerType in queryTypes)
        {
            var methods = handlerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in methods)
            {
                try
                {
                    var methodBody = method.GetMethodBody();
                    if (methodBody == null) continue;

                    // Check fields for repository references that have mutation methods
                    var fields = handlerType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var repositoryFields = fields.Where(f =>
                        f.FieldType.GetMethods().Any(m => mutationMethods.Contains(m.Name)));

                    if (repositoryFields.Any())
                    {
                        violatingHandlers.Add(handlerType.FullName!);
                        break;
                    }
                }
                catch
                {
                    // Skip methods that can't be inspected
                }
            }
        }

        // Assert - Expected behavior: No handlers in Queries namespace use mutation repositories
        violatingHandlers.Should().BeEmpty(
            "Handlers in Queries namespace must not call repository mutation methods (Atualizar, Adicionar, Remover, Deletar). " +
            "Mutations belong in Commands namespace per CQRS principle.");
    }

    /// <summary>
    /// Alternative check: No type named "Update*" should exist in Queries namespace.
    /// </summary>
    [Fact]
    public void Queries_Namespace_Should_Not_Contain_Update_Types()
    {
        // Arrange
        var applicationAssembly = typeof(Application.Commands.CriarAnalise.CriarAnaliseCommand).Assembly;

        // Act
        var updateTypesInQueries = applicationAssembly.GetTypes()
            .Where(t => t.Namespace != null && t.Namespace.Contains(".Queries.", StringComparison.Ordinal))
            .Where(t => t.Name.StartsWith("Update", StringComparison.OrdinalIgnoreCase))
            .Select(t => t.FullName)
            .ToList();

        // Assert
        updateTypesInQueries.Should().BeEmpty(
            "Types named 'Update*' should not exist in Queries namespace. " +
            "Update operations are mutations and belong in Commands namespace.");
    }

    #endregion

    #region Property 4: Dead Code - No unused private methods in Infrastructure

    /// <summary>
    /// Bug Condition: GetRetryPolicy and GetCircuitBreakerPolicy are dead code (never called).
    /// Expected (after fix): These methods are removed or integrated.
    /// Counterexample on unfixed code: Methods exist but have zero callers.
    /// </summary>
    [Fact]
    public void DependencyInjection_Should_Not_Have_Unused_Policy_Methods()
    {
        // Arrange
        var infrastructureAssembly = typeof(Infrastructure.DependencyInjection).Assembly;
        var diType = typeof(Infrastructure.DependencyInjection);

        // Act - Check if dead code methods still exist
        var getRetryPolicy = diType.GetMethod("GetRetryPolicy",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        var getCircuitBreakerPolicy = diType.GetMethod("GetCircuitBreakerPolicy",
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

        // Assert - Expected behavior: These methods should NOT exist (removed as dead code)
        getRetryPolicy.Should().BeNull(
            "GetRetryPolicy is dead code with zero callers and should be removed.");
        getCircuitBreakerPolicy.Should().BeNull(
            "GetCircuitBreakerPolicy is dead code with zero callers and should be removed.");
    }

    #endregion
}
