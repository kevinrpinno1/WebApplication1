# WebApplication1

This project is a demonstration RESTful API built with ASP.NET Core 9. It provides a simple e-commerce backend for managing customers, products, and orders, with a focus on clean architecture, modern .NET practices, and robust security.

## Core Features

*   **Authentication:**
    *   User registration and login functionality using ASP.NET Core Identity.
    *   Secure JWT (JSON Web Token) based authentication for all protected endpoints.
    *   Anonymous access is available for token acquisition and user registration.

*   **Customer Management:**
    *   Full CRUD (Create, Read, Update, Delete) operations for customer records.
    *   Endpoints to retrieve all customers, a single customer by ID, or search by name.

*   **Product Management:**
    *   Full CRUD operations for products, including name, price, and stock quantity.
    *   Endpoints to retrieve all products, a single product by ID, or search by name.

*   **Order Management:**
    *   A rich service layer for handling complex order logic.
    *   Create new orders with multiple line items.
    *   Add, update, or remove items from existing orders.
    *   Update the status of an order (e.g., Pending, Shipped, Completed).
    *   Business logic to prevent deletion of products or customers that are part of existing orders.

## Technical Implementation

*   **Framework:** .NET 9 / ASP.NET Core
*   **Architecture:** Controller-based REST API with a dedicated service layer for business logic (`OrderService`).
*   **Database:** SQL Server accessed via Entity Framework Core, using a code-first approach with migrations.
*   **Authentication:** ASP.NET Core Identity with JWT Bearer tokens.
*   **Validation:** FluentValidation for validating incoming DTOs, with centralized exception handling.
*   **Testing:** Includes a unit test project (`xUnit`) demonstrating how to test service logic in isolation using in-memory databases and mocking (`Moq`).
*   **API Design:**
    *   Uses `async/await` for all I/O-bound operations.
    *   Global exception handling middleware to provide structured error responses.
    *   AutoMapper for clean and efficient mapping between DTOs and data models.
    *   JSON responses are serialized to `camelCase` using `System.Text.Json`.

## Postman Testing:

*  A Postman collection is included in the `Postman` folder to facilitate testing of all API endpoints. 
*  Import the collection into Postman to quickly get started with testing the API.
*  The collection includes preconfigured requests for user registration, login, and all CRUD operations for customers, products, and orders.
*  All requests have included payloads to demonstrate the expected input format, some values may need to be adjusted based on your testing scenario (e.g., IDs).
*  It also includes dedicated testing for Customers, Products, and most Orders endpoints to ensure comprehensive coverage of the API functionality.
*  The JWT token acquisition request is set up to allow easy authentication for protected endpoints, once called it is auto saved to used in subsequent requests.
*  A fun note: look at the Visualizer tab in the Token response to see a custom HTML visualization of the token details!