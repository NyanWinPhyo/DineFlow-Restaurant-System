# DineFlow Restaurant System

DineFlow is a full-stack restaurant management web application built with ASP.NET Core MVC and SQL Server.

The system supports multiple restaurant roles — Admin, Manager, Chef, and Customer — and connects customer ordering, kitchen operations, menu management, ingredient inventory, stock tracking, sales reporting, and customer feedback into a single workflow.

DineFlow originally began as a rebuild of an earlier academic C# Windows Forms restaurant management project. It was later expanded independently into a more complete ASP.NET Core MVC application with a stronger focus on realistic restaurant operations and interconnected business logic.

---

## Tech Stack

- ASP.NET Core MVC
- C#
- .NET
- SQL Server
- ADO.NET
- Razor Views
- Bootstrap
- HTML / CSS / JavaScript
- Visual Studio

---

## Main Features

### Authentication and Role Management

- Login system
- Session-based authentication
- Role-based access control
- Automatic role-based redirection
- Access denied handling
- Separate interfaces for:
  - Admin
  - Manager
  - Chef
  - Customer

---

## Admin Module

### Dashboard

- Total users
- Total menu items
- Available menu items
- Total menu categories
- Total orders
- Completed paid sales total
- Active role display
- Low-stock ingredient count
- Low-stock ingredient alerts

### User Management

- View and search users
- Add users
- Edit users
- Optional password updates
- Activate / deactivate accounts
- Restricted permanent deletion
- Customer wallet management
- Admin profile management

### Order Management

- View all customer orders
- Search orders
- Filter by order status
- View detailed order information
- Update order status
- Cancel eligible orders
- Automatic customer wallet refund for cancelled paid orders

### Feedback Management

- View customer reviews
- Search reviews
- Filter by rating
- Filter reviewed / unreviewed feedback
- View review details
- Add or edit management responses
- Mark feedback as reviewed

---

## Manager Module

### Dashboard

- Total menu items
- Available menu items
- Total categories
- Pending orders
- Preparing orders
- Pending reviews
- Low-stock ingredient count
- Low-stock ingredient alerts

### Restaurant Operations

Managers can:

- Manage orders
- Update order statuses
- Manage menu items
- Manage menu categories
- Manage ingredients
- Restock ingredients
- Record stock adjustments
- Record ingredient waste
- Manage menu item recipes
- View stock transaction history
- Respond to customer feedback
- View sales reports

Business rules are shared with the Admin workflow where appropriate.

---

## Chef Module

### Kitchen Dashboard

- View pending and preparing orders
- Search kitchen orders
- Filter orders by status
- View kitchen-focused order details
- Move orders from Pending → Preparing → Completed

Completing an order automatically deducts the required ingredient quantities from inventory.

### Menu Management

- View menu items
- Add menu items
- Edit menu item details
- Upload / replace / remove food images
- Mark menu items as available or unavailable
- Menu price editing restricted for Chef accounts

### Inventory Visibility

Chefs can:

- View ingredient stock levels
- View low-stock alerts
- View stock transaction history

Inventory modification remains restricted to management roles.

### Reviews

- Read customer reviews
- Search and filter reviews

---

## Customer Module

### Menu

- Browse available menu items
- Menu items grouped by category
- Food image display
- Search menu items
- Hidden unavailable items
- Hidden items from inactive categories
- Ingredient-aware menu availability

A menu item is only shown to customers when:

- The menu item is active
- Its category is active
- A recipe has been assigned
- All required ingredients are active
- Sufficient ingredient stock exists

### Cart

- Session-based shopping cart
- Select quantity before adding to cart
- Increase / decrease quantity
- Remove individual items
- Clear cart
- Live stock-aware quantity validation
- Combined ingredient validation across the full cart

Exact internal stock quantities are not exposed to customers.

### Ordering

- Customer wallet balance
- Place orders using wallet funds
- Automatic wallet deduction
- Order confirmation
- Order history
- Order detail view
- Order status tracking

### Reviews

- Leave reviews for completed orders
- One review per completed order
- View management responses

---

## Menu Management

### Categories

- Add categories
- Edit categories
- Activate / deactivate categories
- Prevent invalid category deactivation
- Restricted permanent deletion for unused categories

### Menu Items

- Add menu items
- Edit menu items
- Search menu items
- Set availability
- Duplicate-name validation
- Upload food images
- Replace food images
- Remove food images
- Restricted permanent deletion for items with existing records

Supported image formats:

- JPG
- JPEG
- PNG
- WEBP

---

## Inventory and Recipe System

### Ingredients

Each ingredient stores:

- Ingredient name
- Unit
- Current stock
- Reorder level
- Cost per unit
- Active status

Supported example units include:

- g
- kg
- ml
- L
- pcs

### Restocking

Restocking:

- Increases ingredient stock
- Records unit cost
- Calculates total cost
- Creates a stock transaction
- Records the responsible user

### Stock Adjustment

Management users can record stock corrections.

Adjustments can:

- Increase stock
- Decrease stock
- Require a reason
- Prevent stock from falling below zero

### Waste Tracking

Ingredient waste can be recorded separately from normal adjustments.

Waste:

- Reduces stock
- Requires a reason
- Records the associated cost
- Creates a `Waste` stock transaction

### Menu Item Recipes

Menu items can be linked to multiple ingredients.

Each recipe defines the quantity required for one serving.

Example:

```text
Chicken Chop
├── Chicken Breast
├── Potato
├── Black Pepper Sauce
└── Cooking Oil