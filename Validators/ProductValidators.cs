using FluentValidation;
using WebApplication1.DTOs;

namespace WebApplication1.Validators
{
    /// <summary>
    /// Contains the shared validation rules for any DTO that implements IProductDto.
    /// </summary>
    public class ProductInterfaceValidator : AbstractValidator<IProductDto>
    {
        public ProductInterfaceValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(2).WithMessage("Product name must be at least 2 characters long.")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");
        }
    }

    /// <summary>
    /// Validator for the CreateProductDto.
    /// </summary>
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            Include(new ProductInterfaceValidator());

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.");
        }
    }

    /// <summary>
    /// Validator for the UpdateProductDto.
    /// </summary>
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            Include(new ProductInterfaceValidator());

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required for an update.");
        }
    }
}