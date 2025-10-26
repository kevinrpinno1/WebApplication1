using FluentValidation;
using WebApplication1.DTOs;

namespace WebApplication1.Validators
{
    /// <summary>
    /// Validator for the CreateOrderItemDto.
    /// </summary>
    public class CreateOrderItemDtoValidator : AbstractValidator<CreateOrderItemDto>
    {
        public CreateOrderItemDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
            
            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.")
                .When(x => x.DiscountAmount.HasValue);
        }
    }

    public class UpdateOrderItemDtoValidator : AbstractValidator<UpdateOrderItemDto>
    {
        public UpdateOrderItemDtoValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.");
        }
    }

    /// <summary>
    /// Validator for the CreateOrderDto.
    /// </summary>
    public class CreateOrderDtoValidator : AbstractValidator<CreateOrderDto>
    {
        public CreateOrderDtoValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("Order must contain at least one item.");

            // Apply the OrderItem validator to each item in the list.
            RuleForEach(x => x.OrderItems)
                .SetValidator(new CreateOrderItemDtoValidator());

            RuleFor(x => x.DiscountAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.")
                .When(x => x.DiscountAmount.HasValue);
        }
    }

    public class UpdateOrderDtoValidator : AbstractValidator<UpdateOrderDto>
    {
        public UpdateOrderDtoValidator()
        {
            RuleFor(x => x.OrderItems)
                .NotEmpty().WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.OrderItems)
                .SetValidator(new CreateOrderItemDtoValidator());
        }
    }
}