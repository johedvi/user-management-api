using FluentValidation;
using UserManagementApi.Models.User;

namespace UserManagementApi.Validators;

    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest> {
        public UpdateUserRequestValidator() {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
                .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            When(x => !string.IsNullOrEmpty(x.FirstName), () => {
                RuleFor(x => x.FirstName).MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.LastName), () => {
                RuleFor(x => x.LastName).MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
            });
        }
    }
