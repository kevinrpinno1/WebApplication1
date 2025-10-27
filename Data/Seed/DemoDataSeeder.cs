using Bogus;
using Data;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data.Seed
{
    public static class DemoDataSeeder
    {
        public static async Task SeedDemoDataAsync(ApplicationDbContext context)
        {
            // Use a single Randomizer for deterministic "randomness" if needed.
            // This means the "random" data will be the same every time you run the seeder.
            Randomizer.Seed = new Random(8675309);

            // 1. Seed Products if the table is empty
            if (!await context.Products.AnyAsync())
            {
                var products = GenerateProducts(20); // Generate 20 products
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }

            // 2. Seed Customers if the table is empty
            if (!await context.Customers.AnyAsync())
            {
                var customers = GenerateCustomers(10); // Generate 10 customers
                await context.Customers.AddRangeAsync(customers);
                await context.SaveChangesAsync();
            }

            // 3. Seed Orders if the table is empty
            if (!await context.Orders.AnyAsync())
            {
                var customers = await context.Customers.ToListAsync();
                var products = await context.Products.ToListAsync();
                var orders = GenerateOrdersForCustomers(customers, products);
                await context.Orders.AddRangeAsync(orders);
                await context.SaveChangesAsync();
            }
        }

        private static List<Product> GenerateProducts(int count)
        {
            var productFaker = new Faker<Product>()
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Price, f => Math.Round(f.Random.Decimal(5, 250), 2))
                .RuleFor(p => p.StockQuantity, f => f.Random.Int(0, 100));

            return productFaker.Generate(count);
        }

        private static List<Customer> GenerateCustomers(int count)
        {
            var customerFaker = new Faker<Customer>()
                .RuleFor(c => c.Name, f => f.Name.FullName())
                .RuleFor(c => c.Address, f => f.Address.StreetAddress())
                .RuleFor(c => c.PhoneNumber, f => f.Phone.PhoneNumber());

            return customerFaker.Generate(count);
        }

        private static List<Order> GenerateOrdersForCustomers(List<Customer> customers, List<Product> products)
        {
            var orders = new List<Order>();
            var random = new Random();

            foreach (var customer in customers)
            {
                var numberOfOrders = random.Next(1, 4); // Each customer will have 1 to 3 orders
                for (int i = 0; i < numberOfOrders; i++)
                {
                    var order = new Order
                    {
                        CustomerId = customer.CustomerId,
                        Status = (OrderStatus)random.Next(0, 4), // Random status
                        DiscountAmount = random.Next(0, 5) == 0 ? random.Next(5, 20) : 0, // 1 in 5 chance of an order-level discount
                        OrderItems = new List<OrderItem>()
                    };

                    var numberOfItems = random.Next(1, 6); // Each order will have 1 to 5 items

                    // Keep track of products already added to this specific order to avoid duplicates.
                    var productIdsInThisOrder = new HashSet<int>();

                    for (int j = 0; j < numberOfItems; j++)
                    {
                        Product product;
                        // Keep picking a random product until we find one that isn't already in this order.
                        do
                        {
                            product = products[random.Next(products.Count)];
                        } while (productIdsInThisOrder.Contains(product.ProductId));

                        productIdsInThisOrder.Add(product.ProductId);

                        var orderItem = new OrderItem
                        {
                            ProductId = product.ProductId,
                            Quantity = random.Next(1, 4),
                            UnitPrice = product.Price, // Snapshot the price
                            DiscountAmount = random.Next(0, 10) == 0 ? random.Next(1, 5) : 0 // 1 in 10 chance of an item-level discount
                        };
                        order.OrderItems.Add(orderItem);
                    }
                    orders.Add(order);
                }
            }
            return orders;
        }
    }
}
