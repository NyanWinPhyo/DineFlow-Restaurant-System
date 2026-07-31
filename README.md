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
- Customer cart using session storage
- Add items to cart
- Update cart quantity
- Remove items from cart
- Clear cart
- Wallet balance display
- Place orders using customer wallet balance
- View order history
- View order details and order status

### Order Management
- Order placement from customer cart
- Wallet payment deduction during order placement
- Order and order item database records
- Order status tracking
- Admin order list
- Admin order detail view
- Admin order status updates
- Cancelled order wallet refund
- Search and filter orders by customer, order ID, and status

## Database

Current database tables:

- Users
- Customers
- MenuCategories
- MenuItems
- Orders
- OrderItems

## Planned Features

### Customer Ordering Improvements
- Improve customer layout/navigation
- Add cart item notes or special requests
- Add order cancellation request flow
- Improve order status badges

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

### Feedback and Reviews
- Customer reviews for completed orders
- One review per completed order
- Rating and comment system
- Admin/Manager feedback management
- Optional admin/manager response to feedback
- 
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
- Customer cart system
- Customer order placement
- Customer wallet payment deduction
- Customer order tracking
- Admin order management
- Admin order status updates and refund handling

Next development focus:

- Chef order dashboard
- Chef order status workflow
- Kitchen-focused order view
- Manager module foundation
- Sales report and dashboard analytics