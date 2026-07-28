# DineFlow Restaurant System

DineFlow is a restaurant management system built with ASP.NET Core MVC and SQL Server.  
The project is designed as a full-stack portfolio project that demonstrates role-based access, user management, menu management, ordering, reporting, inventory tracking, and restaurant finance flow.

## Tech Stack

- ASP.NET Core MVC
- C#
- SQL Server
- ADO.NET
- Bootstrap
- Visual Studio

## Current Features

### Authentication
- Login system
- Role-based redirection
- Session-based access control

### Admin Module
- Admin dashboard
- User management
- Search users
- Add new users
- Edit user details
- Optional password update during edit
- Activate and deactivate users
- Restricted hard delete for mistaken/test accounts
- Customer wallet handling
- Admin profile update

### Menu Module
- Menu category database table
- Menu item database table
- Menu item list page
- Search menu items by name, category, or description

## Planned Features

### Menu Management
- Add menu items
- Edit menu items
- Mark menu items as available/unavailable
- Manage menu categories

### Customer Module
- Browse available menu items
- Place orders
- View order history
- Wallet-based payment flow

### Chef Module
- View incoming orders
- Update order preparation status
- Manage kitchen availability

### Manager Module
- Manage menu items
- View sales reports
- Monitor restaurant operations

### Inventory System
- Ingredient tracking
- Ingredient stock levels
- Menu item ingredient requirements
- Automatic menu availability based on ingredient stock
- Low-stock warnings

### Restaurant Finance Tracker
- Restaurant wallet/balance tracking
- Order income tracking
- Ingredient restock cost tracking
- Inflow/outflow transaction history
- Net revenue calculation

## Database

Current database:

- Users
- Customers
- MenuCategories
- MenuItems

Future database tables may include:

- Orders
- OrderItems
- Feedback
- Ingredients
- InventoryStock
- MenuItemIngredients
- RestockRecords
- RestaurantTransactions

## Project Status

This project is currently under active development.

Completed so far:

- Initial ASP.NET Core MVC setup
- SQL Server database connection
- Authentication and role routing
- Admin user management
- Admin profile management
- Initial menu database and menu list page

Next development focus:

- Add menu item creation
- Edit menu items
- Menu availability control