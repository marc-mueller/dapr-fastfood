# FrontendSelfServicePos Client App Requirements

## Introduction
The `FrontendSelfServicePos` is a web application that provides a self-service point-of-sales interface for customers of a fast food restaurant. This application is statically served from an ASP.NET Core app and communicates with APIs within the same domain. The app guides the customer through the process of placing their orders, adding products to the shopping cart, and completing payment.

## Features and Functionality
### General
- The frontend can be developed using any suitable UI library or framework.
- The application registers with a SignalR Hub to receive updates on the current active order.

### Screens and Behaviors

#### 1. Start Screen
- **Description**: The entry point for customers to begin their ordering process.
- **UI Components**:
    - A button labeled "Start Ordering".
- **Behavior**:
    - When the button is pressed, a new order is created using the `OrderController`'s `CreateOrder` API.
    - Upon successful order creation, the application transitions to the next screen.

#### 2. Add Products to Shopping Cart
- **Description**: Displays available products categorized, allowing customers to add them to their shopping cart.
- **UI Components**:
    - Product categories and product listings.
    - Shopping cart displayed on the right, showing products added to the cart.
    - Buttons next to each product to select the quantity and add to the cart.
    - A button labeled "Order" within the cart section.
    - Buttons to remove products from the cart.
- **Behavior**:
    - Fetch the list of products using the `ProductsController`'s `GetProducts` API.
    - Customers select the quantity and add products to their cart.
    - Cart updates in real-time to reflect added/removed products.
    - Pressing "Order" confirms the order using the `OrderController`'s `ConfirmOrder` API and transitions to the order confirmation screen.

#### 3. Order Confirmation Page
- **Description**: Displays the final order details and provides an option to make a payment.
- **UI Components**:
    - List of ordered items.
    - A button labeled "Pay".
- **Behavior**:
    - Review the list of items in the order.
    - Pressing "Pay" confirms the payment using the `OrderController`'s `ConfirmPayment` API.
    - If payment is successful, show an acknowledgement message.
    - Automatically redirect back to the start screen after 3 seconds.

### SignalR Integration
- **Purpose**: To provide real-time updates about the order status.
- **Behavior**:
    - The application registers for updates on the current active order through the `OrderUpdateHub`.
    - The backend pushes `OrderConfirmed` and `OrderPaid` status updates via SignalR events.

### API Controllers and Methods

#### OrderController
- **GetOrder(Guid id)**: Retrieves the details of a specific order.
- **CreateOrder(OrderDto orderDto)**: Initiates the creation of a new order.
- **AddItem(Guid orderid, OrderItemDto item)**: Adds an item to the specified order.
- **RemoveItem(Guid orderid, Guid itemId)**: Removes an item from the specified order.
- **ConfirmOrder(Guid orderid)**: Confirms the order.
- **ConfirmPayment(Guid orderid)**: Confirms the payment for the order.

#### ProductsController
- **GetProducts()**: Retrieves a list of available products.

### SignalR Hub (OrderUpdateHub)
- **SubscribeToOrder(string orderId)**: Subscribes the client to updates for a specific order.
- **UnsubscribeFromOrder(string orderId)**: Unsubscribes the client from updates for a specific order.
- **OnDisconnectedAsync(Exception? exception)**: Handles disconnection of a client, removing it from any subscribed order updates.

## Conclusion
This document outlines the requirements and behaviors for the `FrontendSelfServicePos` client app. It covers the start screen, product addition to the shopping cart, order confirmation, and integration with the SignalR hub for real-time updates. The detailed requirements should guide the development and ensure all necessary features are implemented correctly.