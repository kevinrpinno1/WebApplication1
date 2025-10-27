using AutoMapper;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Profiles
{
    /// <summary>
    /// AutoMapper profile for mapping between domain models and DTOs.
    /// Helps to keep mapping configurations organized and maintainable, as well as separating mapping logic from business logic.
    /// </summary>
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Customer Mappings
            CreateMap<Customer, GetCustomerDto>();
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();

            // Product Mappings
            CreateMap<Product, GetProductDto>();
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            // Order Mappings
            CreateMap<Order, GetOrderDto>()
                .ForMember(dest => dest.CustomerName, opt => 
                    opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
                .ForMember(dest => dest.Status, opt => 
                    opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Subtotal, opt => 
                    opt.MapFrom(src => src.OrderItems.Sum(item => item.Quantity * item.UnitPrice)))
                .ForMember(dest => dest.OrderDiscount, opt => 
                    opt.MapFrom(src => src.DiscountAmount))
                .ForMember(dest => dest.OrderTotal, opt => 
                    opt.MapFrom(src => src.OrderItems.Sum(item => 
                                item.Quantity * (item.UnitPrice - item.DiscountAmount)) - src.DiscountAmount));

            CreateMap<CreateOrderDto, Order>()
                .ForMember(dest => dest.Status, opt => 
                    opt.MapFrom(src => OrderStatus.Pending)); 

            // OrderItem Mappings
            CreateMap<OrderItem, GetOrderItemDto>()
                .ForMember(dest => dest.ProductName, opt => 
                    opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.LineTotal, opt => 
                    opt.MapFrom(src => (src.Quantity * src.UnitPrice) - src.DiscountAmount));

            CreateMap<CreateOrderItemDto, OrderItem>();
            CreateMap<UpdateOrderItemDto, OrderItem>();

        }
    }
}
