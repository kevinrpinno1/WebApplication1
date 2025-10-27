using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(CreateOrderDto dto, CancellationToken ct);
        Task<Order> AddItemToOrderAsync(Guid orderId, CreateOrderItemDto itemDto, CancellationToken ct);
        Task<Order> UpdateOrderItemAsync(Guid orderId, Guid orderItemId, UpdateOrderItemDto itemDto, CancellationToken ct);
        Task RemoveOrderItemAsync(Guid orderId, Guid orderItemId, CancellationToken ct);
        Task DeleteOrderAsync(Guid id, CancellationToken ct);
        Task UpdateOrderStatusAsync(Guid id, OrderStatus newStatus, CancellationToken ct);
    }
}