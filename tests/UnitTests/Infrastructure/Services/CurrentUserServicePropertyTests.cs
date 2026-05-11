using FsCheck;
using FsCheck.Xunit;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using System.Security.Claims;

namespace UnitTests.Infrastructure.Services;

/// <summary>
/// Property-Based Tests for CurrentUserService.
///
/// **Validates: Requirements 15.1, 15.2, 15.3, 15.4**
/// </summary>
public class CurrentUserServicePropertyTests
{
    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static CurrentUserService CreateSut(ClaimsPrincipal? user)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();

        if (user is not null)
        {
            var httpContext = Substitute.For<HttpContext>();
            httpContext.User.Returns(user);
            accessor.HttpContext.Returns(httpContext);
        }
        else
        {
            accessor.HttpContext.Returns((HttpContext?)null);
        }

        return new CurrentUserService(accessor);
    }

    private static ClaimsPrincipal AuthenticatedPrincipal(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    // -------------------------------------------------------------------------
    // Property 12: CurrentUserService extracts claims correctly
    // -------------------------------------------------------------------------

    /// <summary>
    /// For any ClaimsPrincipal with NameIdentifier claim, UserId SHALL return that value.
    /// **Validates: Requirements 15.1**
    /// </summary>
    [Fact]
    public void Property12_UserId_ReturnsNameIdentifierClaimValue()
    {
        var prop = Prop.ForAll(
            Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary(),
            (Guid userId) =>
            {
                var principal = AuthenticatedPrincipal(
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

                var sut = CreateSut(principal);

                return sut.UserId == userId.ToString();
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// For any ClaimsPrincipal with Name claim, UserName SHALL return that value.
    /// **Validates: Requirements 15.2**
    /// </summary>
    [Fact]
    public void Property12_UserName_ReturnsNameClaimValue()
    {
        var prop = Prop.ForAll(
            Arb.Generate<NonEmptyString>().Select(s => s.Get).ToArbitrary(),
            (string name) =>
            {
                var principal = AuthenticatedPrincipal(
                    new Claim(ClaimTypes.Name, name));

                var sut = CreateSut(principal);

                return sut.UserName == name;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// For any ClaimsPrincipal with Email claim, Email SHALL return that value.
    /// **Validates: Requirements 15.3**
    /// </summary>
    [Fact]
    public void Property12_Email_ReturnsEmailClaimValue()
    {
        var prop = Prop.ForAll(
            Arb.Generate<NonEmptyString>().Select(s => s.Get).ToArbitrary(),
            (string email) =>
            {
                var principal = AuthenticatedPrincipal(
                    new Claim(ClaimTypes.Email, email));

                var sut = CreateSut(principal);

                return sut.Email == email;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// For any ClaimsPrincipal with all three claims, all properties return correct values.
    /// **Validates: Requirements 15.1, 15.2, 15.3**
    /// </summary>
    [Fact]
    public void Property12_AllClaims_AllPropertiesReturnCorrectValues()
    {
        var prop = Prop.ForAll(
            Arb.Generate<Guid>().Where(g => g != Guid.Empty).ToArbitrary(),
            Arb.Generate<NonEmptyString>().Select(s => s.Get).ToArbitrary(),
            Arb.Generate<NonEmptyString>().Select(s => s.Get).ToArbitrary(),
            (Guid userId, string name, string email) =>
            {
                var principal = AuthenticatedPrincipal(
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Email, email));

                var sut = CreateSut(principal);

                return sut.UserId == userId.ToString()
                    && sut.UserName == name
                    && sut.Email == email
                    && sut.IsAuthenticated;
            });

        prop.QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// When HttpContext is null (no authenticated user), IsAuthenticated SHALL be false
    /// and UserId SHALL be null.
    /// **Validates: Requirements 15.4**
    /// </summary>
    [Fact]
    public void Property12_NoHttpContext_IsAuthenticatedFalse_UserIdNull()
    {
        var sut = CreateSut(user: null);

        Assert.False(sut.IsAuthenticated);
        Assert.Null(sut.UserId);
        Assert.Null(sut.UserName);
        Assert.Null(sut.Email);
    }

    /// <summary>
    /// When ClaimsPrincipal has no authentication type (unauthenticated identity),
    /// IsAuthenticated SHALL be false.
    /// **Validates: Requirements 15.4**
    /// </summary>
    [Fact]
    public void Property12_UnauthenticatedPrincipal_IsAuthenticatedFalse()
    {
        // ClaimsIdentity without authenticationType = unauthenticated
        var identity = new ClaimsIdentity();
        var principal = new ClaimsPrincipal(identity);

        var sut = CreateSut(principal);

        Assert.False(sut.IsAuthenticated);
    }
}
