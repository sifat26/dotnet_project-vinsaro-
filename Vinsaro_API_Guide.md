# Vinsaro API - Postman Testing Guide

This guide walks you through the complete flow to test the Vinsaro e-commerce backend using Postman.

## 1. Setup & Authentication Flow

Before doing anything, you need to create an Admin account and log in to get your JWT Bearer token.

### A. Initialize the First Admin
- **Endpoint:** `POST /api/v1/Auth/init-admin`
- **Body:**
```json
{
  "masterKey": "vinsaro-super-secret-master-key-2024",
  "email": "admin@vinsaro.com",
  "password": "Password123!",
  "fullName": "System Admin",
  "phoneNumber": "01711000000"
}
```

### B. Login (Get Token)
- **Endpoint:** `POST /api/v1/Auth/login`
- **Body:**
```json
{
  "email": "admin@vinsaro.com",
  "password": "Password123!"
}
```
**Postman Script:** Add this to the **Tests** tab of your Login request to automatically save the token:
```javascript
var jsonData = pm.response.json();
if(jsonData.token) {
    pm.environment.set("jwt_token", jsonData.token);
}
```
*Note: For all subsequent requests, go to the **Authorization** tab, select **Bearer Token**, and use `{{jwt_token}}`.*

---

## 2. Catalog Setup (Admin Only)

### A. Create a Category
- **Endpoint:** `POST /api/v1/Categories`
- **Body:**
```json
{
  "name": "Men's Shirts",
  "description": "High quality shirts for men"
}
```
*(Save the `id` from the response as `categoryId`)*

### B. Create a Product
- **Endpoint:** `POST /api/v1/Products`
- **Body:**
```json
{
  "categoryId": "{{categoryId}}",
  "name": "Premium Cotton Oxford Shirt",
  "description": "Classic oxford shirt perfect for any occasion.",
  "brand": "Vinsaro Originals"
}
```
*(Save the `id` from the response as `productId`)*

### C. Add a Variant (SKU, Price, Stock)
- **Endpoint:** `POST /api/v1/Products/{{productId}}/variants`
- **Body:**
```json
{
  "sku": "OXF-BLU-L",
  "price": 1250.00,
  "stockQuantity": 50,
  "color": "Blue",
  "size": "L"
}
```
*(Save the `id` from the response as `variantId`)*

---

## 3. Customer Shopping Flow

*(You can test this as the Admin, or register a new regular customer via `POST /api/v1/Auth/register` and log in with them).*

### A. Add Item to Cart
- **Endpoint:** `POST /api/v1/Cart/items`
- **Body:**
```json
{
  "productVariantId": "{{variantId}}",
  "quantity": 2
}
```

### B. Create Shipping Address
- **Endpoint:** `POST /api/v1/Addresses`
- **Body:**
```json
{
  "fullName": "John Doe",
  "phoneNumber": "01711223344",
  "addressLine1": "House 12, Road 5",
  "addressLine2": "Block A, Bashundhara R/A",
  "area": "Bashundhara",
  "district": "Dhaka",
  "division": "Dhaka",
  "postalCode": "1229",
  "isDefault": true
}
```
*(Save the `id` from the response as `addressId`)*

---

## 4. Checkout & Orders

### A. Process Checkout
- **Endpoint:** `POST /api/v1/Checkout`
- **Body:**
```json
{
  "shippingAddressId": "{{addressId}}",
  "customerNotes": "Please deliver in the evening.",
  "idempotencyKey": "unique-checkout-id-123"
}
```
*(Save the `orderId` from the response)*

### B. View My Orders
- **Endpoint:** `GET /api/v1/Orders`
- **Query Params:** `?page=1&pageSize=10`

---

## 5. Operations

### A. Process Payment (Cash on Delivery)
- **Endpoint:** `POST /api/v1/Payments`
- **Body:**
```json
{
  "orderId": "{{orderId}}",
  "paymentMethod": 2
}
```
*(PaymentMethod Enums: `0` = Digital, `1` = BankTransfer, `2` = CashOnDelivery)*

### B. Update Order Status (Admin Only)
- **Endpoint:** `PUT /api/v1/Orders/{{orderId}}/status`
- **Body:** `3`
*(OrderStatus Enums: `0`=Pending, `1`=Processing, `2`=Shipped, `3`=Delivered, `4`=Cancelled)*

### C. Leave a Product Review
- **Endpoint:** `POST /api/v1/Reviews`
- **Body:**
```json
{
  "orderItemId": "REPLACE_WITH_ORDER_ITEM_ID_FROM_GET_ORDERS",
  "rating": 5,
  "comment": "Amazing quality, highly recommended!"
}
```

### D. View Sales Report (Admin Only)
- **Endpoint:** `GET /api/v1/Reports/sales`
- **Query Params:** `?startDate=2024-01-01&endDate=2024-12-31`
