namespace WebApplication1.Configuration
{
    public static class LoggingMessages
    {
        // --- Order Service Messages ---
        public const string CreateOrder = "Creating order for customer {CustomerId}.";
        public const string OrderCreated = "Order {OrderId} created successfully for customer {CustomerId}.";
        public const string AddItemToOrder = "Adding product {ProductId} to order {OrderId}.";
        public const string OrderItemAdded = "Item with product ID {ProductId} added to order {OrderId}.";
        public const string UpdateOrderItem = "Updating item {OrderItemId} in order {OrderId}.";
        public const string OrderItemUpdated = "Item {OrderItemId} in order {OrderId} was updated successfully.";
        public const string RemoveOrderItem = "Removing item {OrderItemId} from order {OrderId}.";
        public const string OrderItemRemoved = "Item {OrderItemId} was removed from order {OrderId}.";
        public const string DeleteOrder = "Deleting order {OrderId}.";
        public const string OrderDeleted = "Order {OrderId} was deleted successfully.";
        public const string UpdateOrderStatus = "Updating status for order {OrderId} to {Status}.";
        public const string OrderStatusUpdated = "Status for order {OrderId} was updated to {Status}.";

        // --- General Warning/Error Messages ---
        public const string OrderNotFound = "Attempted operation on non-existent order with ID {OrderId}.";
        public const string OrderItemNotFound = "Attempted operation on non-existent order item with ID {OrderItemId} in order {OrderId}.";
    }
}
