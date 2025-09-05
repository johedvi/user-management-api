using FluentValidation;
using UserManagementApi.Models.User;

namespace UserManagementApi.Validators; 
    public class AddUserRequestValidator : AbstractValidator<AddUserRequest> {
        public AddUserRequestValidator() {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .Length(3, 50).WithMessage("Username must be between 3 and 50 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(100).WithMessage("Email cannot exceed 100 characters");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(128).WithMessage("Password cannot exceed 128 characters")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
                .WithMessage("Password must contain at least 8 characters with uppercase, lowercase, number, and special character");

            When(x => !string.IsNullOrEmpty(x.FirstName), () => {
                RuleFor(x => x.FirstName).MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
            });

            When(x => !string.IsNullOrEmpty(x.LastName), () => {
                RuleFor(x => x.LastName).MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
            });
        }
    }
