# DineFlow Restaurant System

DineFlow is a restaurant management system built with ASP.NET Core MVC and SQL Server.  
The project is designed as a full-stack portfolio project that demonstrates role-based access, user management, menu management, customer ordering, kitchen order handling, customer feedback, and restaurant operation tracking.

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

### Admin Order Management
- View all customer orders
- Search orders by order ID, customer name, or login ID
- Filter orders by status
- View order details
- Update order status
- Cancel orders with automatic customer wallet refund

### Admin Feedback Management
- View all customer reviews
- Search reviews by order ID, customer, or comment
- Filter reviews by rating
- Filter reviews by reviewed/unreviewed status
- View review details
- Add management response
- Mark feedback as reviewed

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
- Duplicate menu item validation
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
- Leave reviews for completed orders
- One review per completed order

### Chef Module
- Kitchen dashboard
- View pending and preparing orders
- Search and filter kitchen orders
- View kitchen-focused order details
- Mark orders as preparing
- Mark orders as completed
- Read customer reviews
- Search/filter reviews
- Manage menu items
- Add menu items
- Edit menu item details
- Upload, change, and remove food images
- Mark menu items available/unavailable
- Price editing restricted from chef edit form

### Manager Module
- Manager dashboard foundation
- Dynamic operation statistics
  - Total menu items
  - Available menu items
  - Total categories
  - Pending orders
  - Preparing orders
  - Pending reviews

## Database

Current database tables:

- Users
- Customers
- MenuCategories
- MenuItems
- Orders
- OrderItems
- Feedback

### Sales Reports
- Admin sales report
- Manager sales report
- Date range filtering
- Total sales from completed paid orders
- Completed order count
- Cancelled/refunded order tracking
- Total items sold
- Average order value
- Top-selling menu items
- Recent order report


## Planned Features

### Manager Module Expansion
- Manager order management
- Manager menu management
- Manager feedback handling
- Manager sales/report dashboard

### Sales and Reports
- Total sales report
- Order history report
- Menu item performance report
- Customer activity report
- Revenue summary dashboard

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

### Review System Improvements
- Customer-visible management responses
- Admin/Manager review response history
- Review analytics by rating
- Possible menu-item-specific feedback

### UI Improvements
- Shared layouts for Customer, Chef, and Manager pages
- Better navigation
- Improved status badge colors
- Dashboard charts
- Responsive layout polish

## Project Status

This project is currently under active development.

Completed so far:

- Initial ASP.NET Core MVC setup
- SQL Server database connection
- Authentication and role routing
- Admin user management
- Admin profile management
- Admin order management
- Admin feedback management
- Menu item and category management
- Food image upload for menu items
- Customer menu browsing page
- Customer cart system
- Customer order placement
- Customer wallet payment deduction
- Customer order tracking
- Customer review system
- Chef kitchen order dashboard
- Chef read-only review page
- Chef menu item management
- Manager dashboard foundation

Next development focus:

- Manager order management
- Manager menu and feedback management
- Sales report and dashboard analytics
- Customer-facing review response display