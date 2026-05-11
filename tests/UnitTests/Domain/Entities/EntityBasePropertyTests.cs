using Domain.Entities;
using Domain.Enums;
using EntityBase = Domain.Entities.Base.Entity;
using FsCheck;
using FsCheck.Xunit;
using UnitTests.Generators;

namespace UnitTests.Domain.Entities;

/// <summary>
/// Property-Based Tests for Entity (EntityBase).
/// Feature: registration-service-pbt-cleanup
/// </summary>
public class EntityBasePropertyTests
{
    #region Property 1: Entity initialization invariants

    /// <summary>
    /// For any newly created Entity instance, Id SHALL be non-empty and Ativo SHALL be true.
    /// **Validates: Requirements 2.1, 2.2**
    /// </summary>
    [Fact]
    public void Property1_EntityInitialization_IdNonEmpty_AtivoTrue()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            (Analise analise) =>
            {
                return analise.Id != Guid.Empty
                    && analise.Ativo == true;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 2: Entity lifecycle toggle

    /// <summary>
    /// Desativar() sets Ativo=false and updates DataAtualizacao.
    /// Ativar() sets Ativo=true and updates DataAtualizacao.
    /// Round-trip: Ativar() after Desativar() restores Ativo=true.
    /// **Validates: Requirements 2.3, 2.4**
    /// </summary>
    [Fact]
    public void Property2_EntityLifecycleToggle_DesativarAtivarRoundTrip()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            (Analise analise) =>
            {
                var beforeDesativar = DateTime.UtcNow;

                // Desativar
                analise.Desativar();

                var desativarOk = analise.Ativo == false
                    && analise.DataAtualizacao.HasValue
                    && analise.DataAtualizacao.Value >= beforeDesativar;

                var beforeAtivar = DateTime.UtcNow;

                // Ativar (round-trip)
                analise.Ativar();

                var ativarOk = analise.Ativo == true
                    && analise.DataAtualizacao.HasValue
                    && analise.DataAtualizacao.Value >= beforeAtivar;

                return desativarOk && ativarOk;
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion

    #region Property 3: Entity identity equality

    /// <summary>
    /// Two Entity instances with the same Id are equal; different Ids are never equal.
    /// **Validates: Requirements 2.5, 2.6**
    /// </summary>
    [Fact]
    public void Property3_EntityIdentityEquality_SameIdEqual_DifferentIdNotEqual()
    {
        var prop = Prop.ForAll(
            DomainGenerators.ValidAnalise(),
            DomainGenerators.ValidAnalise(),
            (Analise a, Analise b) =>
            {
                // Different instances with different Ids (generated independently) must not be equal
                if (a.Id != b.Id)
                {
                    var differentIdsNotEqual = !a.Equals(b) && a != b;

                    // Force same Id via reflection and verify equality
                    var idProperty = typeof(EntityBase)
                        .GetProperty("Id")!;
                    var backingField = typeof(EntityBase)
                        .GetField("<Id>k__BackingField",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    Analise c = new Analise(a.ClienteId, a.Nome, a.Status, new List<Diagrama>(), "desc");

                    if (backingField != null)
                    {
                        backingField.SetValue(c, a.Id);
                    }

                    var sameIdEqual = a.Equals(c) && a == c;

                    return differentIdsNotEqual && sameIdEqual;
                }

                // Rare collision: same Id generated — they must be equal
                return a.Equals(b);
            });

        prop.QuickCheckThrowOnFailure();
    }

    #endregion
}
