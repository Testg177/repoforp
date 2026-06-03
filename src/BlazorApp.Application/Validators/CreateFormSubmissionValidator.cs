using BlazorApp.Application.DTOs;
using FluentValidation;

namespace BlazorApp.Application.Validators;

public class CreateFormSubmissionValidator : AbstractValidator<CreateFormSubmissionRequest>
{
    public CreateFormSubmissionValidator()
    {
        RuleFor(x => x.FormDefinitionId).GreaterThan(0);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow.AddMinutes(-5));
        RuleFor(x => x.FieldValues).NotNull();
    }
}

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches(@"[^a-zA-Z0-9]").WithMessage("Hasło musi zawierać znak specjalny.");
        RuleFor(x => x.Roles).NotEmpty().WithMessage("Użytkownik musi mieć co najmniej jedną rolę.");
    }
}
