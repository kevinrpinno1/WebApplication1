using FluentValidation;
using WebApplication1.DTOs;

namespace WebApplication1.Validators
{
    /// <summary>
    /// Contains the shared validation rules for any DTO that implements IProductDto.
    /// </summary>
    public class ProductValidator : AbstractValidator<IProductDto>
    {
        public ProductValidator()
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
    public class CreateProductValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductValidator()
        {
            Include(new ProductValidator());

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative.");
        }
    }

    /// <summary>
    /// Validator for the UpdateProductDto.
    /// </summary>
    public class UpdateProductValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            Include(new ProductValidator());

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required for an update.");
        }
    }
}