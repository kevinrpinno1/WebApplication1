using FluentValidation;
using WebApplication1.DTOs;

namespace WebApplication1.Validators
{
    public class CustomerValidator : AbstractValidator<ICustomerDto>
    {
        public CustomerValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Customer name is required.")
                .MinimumLength(2).WithMessage("Customer name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Customer name cannot exceed 100 characters.");

            RuleFor(x => x.Address)
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");

            const string phoneRegex = @"^(?:\+?1[\s.-]?)?(?:\([2-9]\d{2}\)|[2-9]\d{2})[\s.-]?\d{3}[\s.-]?\d{4}$";

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50).WithMessage("Phone number cannot exceed 50 characters.")
                .Matches(phoneRegex)
                .WithMessage("Invalid phone number format.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
        }
    }
    public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
    {
        public CreateCustomerDtoValidator()
        {
            Include(new CustomerValidator());
        }
    }
    public class UpdateCustomerDtoValidator : AbstractValidator<UpdateCustomerDto>
    {
        public UpdateCustomerDtoValidator()
        {
            Include(new CustomerValidator());
        }
    }
}