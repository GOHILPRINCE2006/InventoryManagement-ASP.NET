# Inventory Management System

A web-based inventory management application built with **ASP.NET Core MVC**, **C#**, and **Entity Framework Core**. It is designed to manage products, categories, suppliers, customers, purchases, sales, and stock movements through a role-based interface.

> **Project:** Academic Web Application Development (WAD) project. Update the configuration and setup details below to match the final source code in your repository.

---

## Table of Contents

- [Overview](#overview)
- [Objectives](#objectives)
- [Features](#features)
- [User Roles](#user-roles)
- [Technology Stack](#technology-stack)
- [System Workflow](#system-workflow)
- [Database Design](#database-design)
- [Business Rules](#business-rules)
- [Project Structure](#project-structure)
- [Getting Started](#getting-started)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [Security](#security)
- [Future Enhancements](#future-enhancements)
- [License](#license)

## Overview

The Inventory Management System provides a centralized platform for maintaining product information and tracking goods moving into and out of stock.

Purchases increase product quantities, while sales decrease them. Before completing a sale, the application checks whether sufficient stock is available. Stock movements are recorded in a transaction history so that changes can be traced to the related purchase or sales invoice.

The application uses role-based access control to separate administrative responsibilities from day-to-day inventory operations.

## Objectives

- Centralize product, category, supplier, and customer information.
- Manage purchase and sales transactions.
- Automatically update stock after purchases and sales.
- Prevent sales that exceed available stock.
- Maintain a history of stock movements.
- Identify products at or below their minimum stock level.
- Provide separate access for Admin, Manager, and Employee users.
- Maintain consistency when a transaction contains multiple products.

## Features

### Authentication and User Management
- Registration where enabled, login, and logout.
- Cookie-based authentication.
- Password hashing rather than plaintext password storage.
- User profile management.
- Admin user management and role assignment.

### Product and Master Data Management
- Manage products, categories, suppliers, and customers.
- Store product SKU, selling price, cost price, quantity, and minimum stock level.
- Search products by name or SKU.
- Filter products by category.
- Deactivate products while retaining historical transaction references.

### Purchase Management
- Create purchases for a selected supplier.
- Add multiple products to one purchase.
- Store quantities and unit prices.
- Increase stock after a valid purchase.
- Record stock-in (`IN`) movements.

### Sales Management and Invoices
- Create sales for a selected customer.
- Add multiple products to one sale.
- Check stock availability before completing a sale.
- Reject a sale if any requested quantity is unavailable.
- Reduce stock after a valid sale.
- Record stock-out (`OUT`) movements.
- View the invoice for a completed sale.

### Inventory Monitoring
- Maintain current stock quantities.
- Display low-stock products.
- Keep a stock movement history with references, users, quantities, and dates.
- Provide dashboard and inventory information according to user permissions.

> The exact dashboard cards, report filters, invoice print options, and export formats depend on the current implementation. Do not assume PDF/Excel export unless it is available in the application.

## User Roles

| Role | Responsibilities |
|---|---|
| **Admin** | Manage users and roles, view system-level dashboard information, and monitor inventory and reports. |
| **Manager** | Manage products and master data, process purchases and sales, and view permitted inventory information. |
| **Employee** | Perform permitted purchase and sales operations, search/view products, and access sales invoices. |

Authorization must be enforced on the server. Hiding a navigation link alone does not protect an action.

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 8) |
| Language | C# |
| Architecture | Model–View–Controller (MVC) |
| ORM / Data access | Entity Framework Core |
| Database approach | Code-First with EF Core migrations |
| Database engine | Confirm the configured provider in the project (SQLite or SQL Server) |
| Frontend | Razor Views, HTML5, CSS3, Bootstrap 5 |
| Client-side scripting | JavaScript; jQuery where used |
| Authentication | Cookie-based authentication |
| Authorization | Role-Based Access Control (RBAC) |

## System Workflow

### Purchase Workflow — Stock In

1. An authorized user opens the purchase form.
2. The user selects a supplier and adds one or more product items.
3. The application validates the supplier, products, quantities, and prices.
4. The purchase header and detail items are saved.
5. The quantity of each purchased product is increased.
6. A stock transaction of type `IN` is recorded for each purchase line.
7. The operation is committed after all required steps succeed.

**Example:** If a product has 12 units in stock and a purchase adds 8 units, the updated stock becomes 20 units.

### Sales Workflow — Stock Out

1. An authorized user opens the sales form.
2. The user selects a customer and adds one or more product items.
3. The application validates the submitted details.
4. The system checks stock availability for every sale item.
5. If any item has insufficient stock, the complete sale is rejected.
6. If all items are available, the sale and its detail items are saved.
7. Product quantities are reduced and `OUT` stock transaction records are created.
8. The completed sale invoice can be displayed.

**Example:** If 20 units are available and 6 units are sold, the updated stock becomes 14 units. If only 4 units are available and 6 are requested, the sale must be rejected.

### Low-Stock Rule

An active product is considered low stock when:

```text
Quantity <= MinStockLevel
```

For example, if the minimum stock level is 10, a product with 10 or fewer units qualifies for the low-stock alert.

## Database Design

| Entity | Purpose |
|---|---|
| `Roles` | Stores role names such as Admin, Manager, and Employee. |
| `ApplicationUsers` | Stores user account and profile information. |
| `Categories` | Groups products into categories. |
| `Suppliers` | Stores supplier details. |
| `Customers` | Stores customer details. |
| `Products` | Stores product information and current stock quantity. |
| `Purchases` | Stores purchase transaction header information. |
| `PurchaseItems` | Stores individual product lines in a purchase. |
| `Sales` | Stores sales transaction and invoice header information. |
| `SaleItems` | Stores individual product lines in a sale. |
| `StockTransactions` | Stores the history of stock movements. |

### Important Relationships

- One role can be assigned to many users.
- One category can contain many products.
- One supplier can be associated with multiple products and purchases.
- One customer can have multiple sales.
- One purchase contains multiple purchase items.
- One sale contains multiple sale items.
- One product can appear in many purchase items, sale items, and stock transactions.
- A user can be associated with the purchases, sales, and stock movements they create.

`Products.Quantity` stores the current stock balance. `StockTransactions` stores the history of changes to that balance.

- `IN` — stock received, normally through a purchase.
- `OUT` — stock issued, normally through a sale.
- `ADJUSTMENT` — manual stock correction, only if supported by the implementation.

## Business Rules

1. Product SKU must be unique.
2. Purchase and invoice numbers must be unique.
3. Transaction quantities must be valid positive values.
4. A sale must not reduce stock below zero.
5. All sale items must pass stock validation before the sale is completed.
6. Purchase/sale headers, detail items, stock changes, and stock logs should be handled as one database transaction.
7. If a required operation fails, the transaction should roll back rather than leave partial changes.
8. Low-stock alerts apply to active products where `Quantity <= MinStockLevel`.
9. Deactivating a product should preserve historical transaction references.
10. Role permissions must be checked by protected server-side actions.

## Project Structure

The exact folder names may differ in your repository. A typical ASP.NET Core MVC structure is:

```text
InventoryManagementSystem/
├── Controllers/       # MVC controllers and request handling
├── Models/            # Entity and domain models
├── ViewModels/        # Data passed between views and controllers
├── Views/             # Razor UI pages
├── Data/              # DbContext and database configuration
├── Migrations/        # Entity Framework Core migrations
├── wwwroot/
│   ├── css/            # Stylesheets
│   ├── js/             # JavaScript files
│   └── images/         # Application images
├── Properties/
├── appsettings.json   # Application configuration
├── Program.cs         # Application startup and middleware
└── README.md
```

Adjust this section to match the actual repository structure.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 or a compatible IDE such as Visual Studio Code
- The database provider configured by the project (SQLite or SQL Server)
- Git

Check your installed .NET SDK:

```bash
dotnet --version
```

### 1. Clone the Repository

```bash
git clone https://github.com/GOHILPRINCE2006/InventoryManagement-ASP.NET.git
cd InventoryManagement-ASP.NET
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure the Database

Open `appsettings.json` and check the connection string and database provider used by the project. Configure the connection string for your local environment.

Do not commit passwords, production connection strings, or other secrets to source control.

### 4. Apply Database Migrations

If the project uses Entity Framework Core migrations, run:

```bash
dotnet ef database update
```

If the EF Core command-line tool is not installed:

```bash
dotnet tool install --global dotnet-ef
```

Run migrations only after confirming the correct database provider, connection string, and startup project. Follow the repository's actual setup if it differs.

## Running the Application

Run the application from the project directory:

```bash
dotnet run
```

Open the local URL printed in the terminal. The exact URL and port depend on the application's launch settings.

## Testing

The following areas should be verified before considering the application ready:

| Area | Example verification |
|---|---|
| Authentication | Valid and invalid login; logout behavior. |
| Authorization | Employee cannot access Admin-only actions. |
| Product management | Required-field validation and duplicate SKU rejection. |
| Purchases | Purchase increases stock and creates an `IN` record. |
| Sales | Sale decreases stock and creates an `OUT` record. |
| Insufficient stock | Sale is rejected and stock remains unchanged. |
| Multi-item transactions | A failed line does not leave partial stock changes. |
| Low-stock alerts | A product at the minimum threshold is included. |
| Product deactivation | Historical transaction information remains accessible. |
| Invoices | Invoice details match the saved sale. |

Only mark tests as passed after executing them and confirming the result.

## Security

- Store password hashes, never plaintext passwords.
- Keep secrets and private connection strings out of source control.
- Enforce authorization on the server for every protected operation.
- Validate user input on the server.
- Use anti-forgery protection for state-changing form submissions where applicable.
- Use database constraints and foreign keys to protect data integrity.
- Avoid exposing sensitive implementation details in error messages.

## Future Enhancements

Potential future improvements include:

- Barcode and QR-code scanning for product identification.
- Supplier reorder suggestions based on stock thresholds.
- More detailed inventory and sales analytics.
- Email or in-app low-stock notifications.
- Additional report export options, if required.
- Stronger concurrency controls for simultaneous sales involving the same stock.

These are proposed enhancements, not existing features unless they have been implemented.

## Contributing

For an academic project, coordinate contributions with the project team.

1. Create a branch for your change.
2. Make the required updates.
3. Test the affected functionality.
4. Commit your changes with a clear message.
5. Open a pull request for review if the repository uses pull requests.
