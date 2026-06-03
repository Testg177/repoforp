using BlazorApp.Application.DTOs;
using BlazorApp.Application.Validators;

namespace BlazorApp.UnitTests;

public class ValidatorTests
{
    private readonly CreateUserValidator _validator = new();

    [Fact]
    public async Task CreateUser_ValidData_PassesValidation()
    {
        var request = new CreateUserRequest(
            "jan.kowalski@firma.pl",
            "Jan",
            "Kowalski",
            "IT",
            "Developer",
            "BezpieczneHaslo123!",
            ["Employee"]);

        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreateUser_InvalidEmail_FailsValidation()
    {
        var request = new CreateUserRequest(
            "not-an-email",
            "Jan",
            "Kowalski",
            null, null,
            "BezpieczneHaslo123!",
            ["Employee"]);

        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task CreateUser_WeakPassword_FailsValidation()
    {
        var request = new CreateUserRequest(
            "jan@firma.pl",
            "Jan",
            "Kowalski",
            null, null,
            "password",   // brak znaku specjalnego
            ["Employee"]);

        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task CreateUser_NoRoles_FailsValidation()
    {
        var request = new CreateUserRequest(
            "jan@firma.pl",
            "Jan",
            "Kowalski",
            null, null,
            "BezpieczneHaslo123!",
            []);

        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }
}
