# E-commerce Platform - API Specifications

## API Gateway Routes Overview

All external API requests go through the YARP API Gateway at `https://localhost:5000`. The gateway handles authentication, routing, rate limiting, and cross-cutting concerns.

### Base API Structure
```
https://localhost:5000/api/{service}/{resource}

Gateway Routes:
├── /api/auth/*           → Auth Service (Port 5001)
├── /api/customers/*      → Customer Service (Port 5002) 
├── /api/products/*       → Catalog Service (Port 5003)
├── /api/categories/*     → Catalog Service (Port 5003)
├── /api/cart/*           → Cart Service (Port 5004)
├── /api/orders/*         → Order Service (Port 5005)
├── /api/payments/*       → Payment Service (Port 5006)
├── /api/inventory/*      → Inventory Service (Port 5007)
├── /api/search/*         → Search Service (Port 5008)
├── /api/notifications/* → Notification Service (Port 5009)
└── /api/admin/*          → Admin routes (various services)
```

## 1. Authentication API

### Base URL: `/api/auth`
**Service**: Auth Service (Enhanced)
**Port**: 5001

#### POST /api/auth/register
Register a new customer account.

**Request:**
```json
{
  "email": "customer@example.com",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+1234567890",
  "acceptsMarketing": true
}
```

**Response: 201 Created**
```json
{
  "success": true,
  "message": "Registration successful. Please check your email for verification.",
  "data": {
    "customerId": "123e4567-e89b-12d3-a456-426614174000",
    "email": "customer@example.com",
    "emailVerificationRequired": true
  }
}
```

#### POST /api/auth/login
Authenticate customer and return tokens.

**Request:**
```json
{
  "email": "customer@example.com",
  "password": "SecurePass123!"
}
```

**Response: 200 OK**
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "def50200e54d...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "customer": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "customer@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "role": "Customer"
    }
  }
}
```

#### POST /api/auth/refresh
Refresh access token using refresh token.

#### POST /api/auth/logout
Invalidate tokens and end session.

#### POST /api/auth/forgot-password
Request password reset.

#### POST /api/auth/reset-password
Reset password using token.

#### GET /api/auth/verify-email/{token}
Verify email address.

---

## 2. Catalog API

### Base URL: `/api/products`, `/api/categories`
**Service**: Catalog Service (Enhanced Base Service)
**Port**: 5003

#### GET /api/products
Get paginated list of products with filtering.

**Query Parameters:**
- `page` (int, default: 1): Page number
- `limit` (int, default: 20, max: 100): Items per page
- `category` (string): Filter by category slug
- `brand` (string): Filter by brand slug
- `minPrice` (decimal): Minimum price filter
- `maxPrice` (decimal): Maximum price filter
- `rating` (int): Minimum rating filter
- `search` (string): Search term
- `sort` (string): Sort order (name, price, rating, created)
- `order` (string): asc/desc

**Response: 200 OK**
```json
{
  "success": true,
  "data": {
    "products": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174001",
        "name": "Gaming Laptop",
        "slug": "gaming-laptop",
        "description": "High-performance gaming laptop",
        "shortDescription": "Perfect for gaming and productivity",
        "sku": "LAPTOP-GAMING-001",
        "price": 1299.99,
        "comparePrice": 1499.99,
        "currency": "USD",
        "category": {
          "id": "123e4567-e89b-12d3-a456-426614174002",
          "name": "Laptops",
          "slug": "laptops"
        },
        "brand": {
          "id": "123e4567-e89b-12d3-a456-426614174003", 
          "name": "TechBrand",
          "slug": "techbrand"
        },
        "images": [
          {
            "url": "https://cdn.example.com/laptop1.jpg",
            "altText": "Gaming laptop front view",
            "isPrimary": true
          }
        ],
        "variants": [
          {
            "id": "123e4567-e89b-12d3-a456-426614174004",
            "sku": "LAPTOP-GAMING-001-BLACK-512GB",
            "name": "Black, 512GB",
            "price": 1299.99,
            "attributes": {
              "color": "Black",
              "storage": "512GB"
            },
            "inventory": {
              "available": 15,
              "reserved": 3
            }
          }
        ],
        "rating": 4.5,
        "reviewCount": 127,
        "status": "active",
        "createdAt": "2024-01-15T10:30:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 20,
      "total": 150,
      "pages": 8,
      "hasNext": true,
      "hasPrev": false
    },
    "filters": {
      "categories": ["laptops", "accessories"],
      "brands": ["techbrand", "gamercorp"],
      "priceRange": { "min": 299.99, "max": 2999.99 },
      "attributes": {
        "color": ["Black", "White", "Silver"],
        "storage": ["256GB", "512GB", "1TB"]
      }
    }
  }
}
```

#### GET /api/products/{id}
Get detailed product information.

**Response: 200 OK**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174001",
    "name": "Gaming Laptop",
    "slug": "gaming-laptop",
    "description": "Detailed product description...",
    "shortDescription": "Perfect for gaming and productivity",
    "sku": "LAPTOP-GAMING-001",
    "price": 1299.99,
    "comparePrice": 1499.99,
    "category": {
      "id": "123e4567-e89b-12d3-a456-426614174002",
      "name": "Laptops",
      "slug": "laptops",
      "breadcrumb": ["Electronics", "Computers", "Laptops"]
    },
    "brand": {
      "id": "123e4567-e89b-12d3-a456-426614174003",
      "name": "TechBrand",
      "slug": "techbrand",
      "description": "Leading technology brand"
    },
    "images": [
      {
        "url": "https://cdn.example.com/laptop1-large.jpg",
        "altText": "Gaming laptop front view",
        "isPrimary": true
      }
    ],
    "variants": [...],
    "specifications": {
      "processor": "Intel Core i7-12700H",
      "memory": "16GB DDR5",
      "graphics": "NVIDIA RTX 4060",
      "display": "15.6\" FHD 144Hz"
    },
    "dimensions": {
      "length": 35.9,
      "width": 25.5,
      "height": 2.5,
      "weight": 2.3
    },
    "seo": {
      "title": "Gaming Laptop - High Performance",
      "description": "Best gaming laptop for professionals",
      "keywords": "gaming, laptop, high-performance"
    },
    "relatedProducts": [...],
    "crossSellProducts": [...],
    "reviews": {
      "average": 4.5,
      "count": 127,
      "distribution": {
        "5": 82,
        "4": 28,
        "3": 12,
        "2": 3,
        "1": 2
      }
    },
    "status": "active",
    "createdAt": "2024-01-15T10:30:00Z",
    "updatedAt": "2024-01-20T14:22:00Z"
  }
}
```

#### GET /api/categories
Get category hierarchy.

**Response: 200 OK**
```json
{
  "success": true,
  "data": {
    "categories": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174002",
        "name": "Electronics",
        "slug": "electronics",
        "description": "Electronic devices and accessories",
        "displayOrder": 1,
        "productCount": 245,
        "children": [
          {
            "id": "123e4567-e89b-12d3-a456-426614174003",
            "name": "Computers",
            "slug": "computers",
            "productCount": 89,
            "children": [
              {
                "id": "123e4567-e89b-12d3-a456-426614174004",
                "name": "Laptops",
                "slug": "laptops",
                "productCount": 34,
                "children": []
              }
            ]
          }
        ]
      }
    ]
  }
}
```

#### Admin Endpoints

#### POST /api/admin/products
Create new product (Admin only).

#### PUT /api/admin/products/{id}
Update product (Admin only).

#### DELETE /api/admin/products/{id}
Delete/Archive product (Admin only).

---

## 3. Cart API

### Base URL: `/api/cart`
**Service**: Cart Service (New)
**Port**: 5004
**Authentication**: Required (or guest session)

#### GET /api/cart
Get current customer's cart.

**Response: 200 OK**
```json
{
  "success": true,
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174005",
    "customerId": "123e4567-e89b-12d3-a456-426614174000",
    "sessionId": "session_abc123",
    "items": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174006",
        "productVariant": {
          "id": "123e4567-e89b-12d3-a456-426614174004",
          "sku": "LAPTOP-GAMING-001-BLACK-512GB",
          "product": {
            "id": "123e4567-e89b-12d3-a456-426614174001",
            "name": "Gaming Laptop",
            "slug": "gaming-laptop"
          },
          "name": "Black, 512GB",
          "attributes": {
            "color": "Black",
            "storage": "512GB"
          },
          "image": "https://cdn.example.com/laptop1-thumb.jpg"
        },
        "quantity": 1,
        "unitPrice": 1299.99,
        "totalPrice": 1299.99,
        "addedAt": "2024-01-20T10:30:00Z"
      }
    ],
    "totals": {
      "subtotal": 1299.99,
      "itemCount": 1,
      "discountAmount": 0.00,
      "estimatedTax": 103.99,
      "estimatedShipping": 15.00,
      "estimatedTotal": 1418.98
    },
    "appliedDiscounts": [],
    "createdAt": "2024-01-20T10:30:00Z",
    "updatedAt": "2024-01-20T10:30:00Z"
  }
}
```

#### POST /api/cart/items
Add item to cart.

**Request:**
```json
{
  "productVariantId": "123e4567-e89b-12d3-a456-426614174004",
  "quantity": 2
}
```

**Response: 201 Created**
```json
{
  "success": true,
  "message": "Item added to cart",
  "data": {
    "itemId": "123e4567-e89b-12d3-a456-426614174006",
    "cart": {
      // Full cart object
    }
  }
}
```

#### PUT /api/cart/items/{itemId}
Update cart item quantity.

**Request:**
```json
{
  "quantity": 3
}
```

#### DELETE /api/cart/items/{itemId}
Remove item from cart.

#### POST /api/cart/discount
Apply discount code.

**Request:**
```json
{
  "code": "SAVE10"
}
```

#### DELETE /api/cart/discount/{code}
Remove discount code.

#### POST /api/cart/merge
Merge guest cart with customer cart (used after login).

---

## 4. Order API

### Base URL: `/api/orders`
**Service**: Order Service (New)
**Port**: 5005
**Authentication**: Required

#### POST /api/orders
Create order from cart.

**Request:**
```json
{
  "shippingAddress": {
    "firstName": "John",
    "lastName": "Doe",
    "streetLine1": "123 Main Street",
    "streetLine2": "Apt 4B",
    "city": "New York",
    "stateProvince": "NY",
    "postalCode": "10001",
    "country": "US",
    "phone": "+1234567890"
  },
  "billingAddress": {
    // Same structure as shipping, or null to use shipping
  },
  "shippingMethod": {
    "id": "standard",
    "name": "Standard Shipping",
    "cost": 15.00,
    "estimatedDays": 5
  },
  "paymentMethod": {
    "type": "card",
    "token": "pm_1234567890" // Stripe payment method
  },
  "notes": "Please leave at door"
}
```

**Response: 201 Created**
```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "orderId": "123e4567-e89b-12d3-a456-426614174007",
    "orderNumber": "ORD-2024-001234",
    "status": "pending",
    "paymentRequired": true,
    "paymentUrl": "https://checkout.stripe.com/pay/cs_test_...",
    "totalAmount": 1418.98
  }
}
```

#### GET /api/orders
Get customer's order history.

**Query Parameters:**
- `page` (int, default: 1): Page number
- `limit` (int, default: 10): Items per page
- `status` (string): Filter by order status

**Response: 200 OK**
```json
{
  "success": true,
  "data": {
    "orders": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174007",
        "orderNumber": "ORD-2024-001234",
        "status": "delivered",
        "items": [
          {
            "productName": "Gaming Laptop",
            "variantName": "Black, 512GB",
            "quantity": 1,
            "unitPrice": 1299.99,
            "totalPrice": 1299.99
          }
        ],
        "totals": {
          "subtotal": 1299.99,
          "taxAmount": 103.99,
          "shippingAmount": 15.00,
          "totalAmount": 1418.98
        },
        "shippingInfo": {
          "method": "Standard Shipping",
          "trackingNumber": "1Z999AA1234567890",
          "carrier": "UPS",
          "estimatedDelivery": "2024-01-25",
          "actualDelivery": "2024-01-24"
        },
        "createdAt": "2024-01-20T15:30:00Z",
        "updatedAt": "2024-01-24T10:15:00Z"
      }
    ],
    "pagination": {
      "page": 1,
      "limit": 10,
      "total": 5,
      "pages": 1
    }
  }
}
```

#### GET /api/orders/{id}
Get detailed order information.

#### POST /api/orders/{id}/cancel
Cancel order (if eligible).

#### GET /api/orders/{id}/invoice
Download order invoice (PDF).

---

## 5. Payment API

### Base URL: `/api/payments`
**Service**: Payment Service (New)
**Port**: 5006
**Authentication**: Required

#### POST /api/payments/methods
Add payment method to customer account.

**Request:**
```json
{
  "type": "card",
  "token": "pm_1234567890", // From Stripe/PayPal
  "isDefault": true
}
```

#### GET /api/payments/methods
Get customer's saved payment methods.

#### DELETE /api/payments/methods/{id}
Remove payment method.

#### POST /api/payments/process
Process payment for order.

**Request:**
```json
{
  "orderId": "123e4567-e89b-12d3-a456-426614174007",
  "paymentMethodId": "123e4567-e89b-12d3-a456-426614174008",
  "amount": 1418.98
}
```

#### POST /api/payments/refund
Process refund.

**Request:**
```json
{
  "paymentId": "123e4567-e89b-12d3-a456-426614174009",
  "amount": 1418.98,
  "reason": "Customer requested refund"
}
```

---

## 6. Search API

### Base URL: `/api/search`
**Service**: Search Service (New)
**Port**: 5008

#### GET /api/search/products
Search products with advanced filtering.

**Query Parameters:**
- `q` (string): Search query
- `category` (string): Category filter
- `brand` (string): Brand filter
- `minPrice`, `maxPrice` (decimal): Price range
- `attributes` (object): Attribute filters
- `sort` (string): Sort field
- `order` (string): asc/desc
- `page`, `limit`: Pagination

**Response: 200 OK**
```json
{
  "success": true,
  "data": {
    "query": "gaming laptop",
    "results": [...], // Same product format as catalog API
    "facets": {
      "categories": [
        { "name": "Laptops", "count": 45 },
        { "name": "Accessories", "count": 12 }
      ],
      "brands": [
        { "name": "TechBrand", "count": 23 },
        { "name": "GamerCorp", "count": 18 }
      ],
      "priceRanges": [
        { "range": "0-500", "count": 8 },
        { "range": "500-1000", "count": 25 },
        { "range": "1000+", "count": 34 }
      ]
    },
    "suggestions": ["gaming laptop bag", "gaming laptop stand"],
    "totalResults": 67,
    "searchTime": 0.045
  }
}
```

#### GET /api/search/suggestions
Get search suggestions/autocomplete.

**Query Parameters:**
- `q` (string): Partial search term

---

## Common API Patterns

### Authentication
All protected endpoints require JWT token in Authorization header:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Response Format
All APIs follow consistent response format:

**Success Response:**
```json
{
  "success": true,
  "message": "Optional success message",
  "data": {
    // Response data
  }
}
```

**Error Response:**
```json
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request data",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      }
    ]
  },
  "timestamp": "2024-01-20T15:30:00Z",
  "path": "/api/auth/register"
}
```

### Error Codes
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid/missing token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `409` - Conflict (duplicate resource)
- `422` - Unprocessable Entity (business rule violation)
- `429` - Too Many Requests (rate limit exceeded)
- `500` - Internal Server Error

### Rate Limiting
- **Public endpoints**: 100 requests/minute per IP
- **Authenticated endpoints**: 1000 requests/minute per user
- **Admin endpoints**: 5000 requests/minute per admin

### Pagination
Standard pagination pattern for list endpoints:
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "limit": 20,
    "total": 150,
    "pages": 8,
    "hasNext": true,
    "hasPrev": false
  }
}
```

### API Versioning
- Header-based: `Accept: application/vnd.ecommerce.v1+json`
- URL-based: `/api/v1/products` (future versions)

This API specification provides a comprehensive foundation for building the e-commerce platform's REST APIs while maintaining consistency, security, and scalability.