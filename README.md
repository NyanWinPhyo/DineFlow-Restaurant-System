# DineFlow Restaurant System

DineFlow is a restaurant management system built with ASP.NET Core MVC and SQL Server.  
The project is designed as a full-stack portfolio project that demonstrates role-based access, user management, menu management, customer ordering, inventory tracking, reservations, and restaurant finance flow.

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
- Access denied handling for unauthorized roles

### Admin Module
- Admin dashboard
- Dynamic dashboard statistics
  - Total users
  - Total menu items
  - Available menu items
  - Total menu categories
- User management
- Search users
- Add new users
- Edit user details
- Optional password update during edit
- Activate and deactivate users
- Restricted hard delete for mistaken/test accounts
- Customer wallet handling
- Admin profile update

### Menu Management
- Menu category management
- Add menu categories
- Edit menu categories
- Activate and deactivate categories
- Restricted hard delete for unused categories
- Menu item management
- Add menu items
- Edit menu items
- Mark menu items as available/unavailable
- Restricted hard delete for unused menu items
- Search menu items
- Food image upload, replacement, and removal for menu items

### Customer Module
- Customer menu browsing page
- Menu items displayed by category
- Food image display
- Search available menu items
- Hidden unavailable menu items
- Hidden items from inactive categories

## Database

Current database tables:

- Users
- Customers
- MenuCategories
- MenuItems

## Planned Features

### Customer Ordering
- Add to cart
- Update cart quantity
- Place orders
- View order history
- Wallet-based payment flow

### Chef Module
- View incoming orders
- Update preparation status
- Mark menu items available/unavailable
- View kitchen workload

### Manager Module
- Manage menu items and categories
- View order status
- View sales reports
- Monitor restaurant operations

### Sales and Reports
- Total sales report
- Order history report
- Menu item performance report
- Customer activity report

### Inventory System
- Ingredient tracking
- Ingredient stock levels
- Menu item ingredient requirements
- Automatic menu availability based on ingredient stock
- Low-stock warnings
- Ingredient restock records

### Restaurant Finance Tracker
- Restaurant wallet/balance tracking
- Order income tracking
- Ingredient restock cost tracking
- Inflow/outflow transaction history
- Net revenue calculation

### Reservation System
- Customer reservation requests
- Admin/Manager reservation management
- Table assignment
- Reservation status tracking

## Project Status

This project is currently under active development.

Completed so far:

- Initial ASP.NET Core MVC setup
- SQL Server database connection
- Authentication and role routing
- Admin user management
- Admin profile management
- Menu item and category management
- Food image upload for menu items
- Customer menu browsing page

Next development focus:

- Customer cart system
- Order placement
- Order database tables
- Chef order dashboard