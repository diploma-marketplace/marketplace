# Future Market — Backend & RESTful API Contract

This document defines the RESTful API contract between the **Backend (`ASP.NET Core 10.0`)** and **Frontend (`React + TypeScript`)** for the **Future Market** platform.

## 1. Swagger UI & OpenAPI Specification

When the backend is running, interactive API documentation is available at:

- **Swagger UI:** `/swagger` (e.g., `http://localhost:5003/swagger` or `https://localhost:7102/swagger`)
- **Swagger OpenAPI JSON:** `/swagger/v1/swagger.json`
- **ASP.NET Core OpenAPI JSON (Development):** `/openapi/v1.json`

---

## 2. Authentication & Role-Based Access Control (RBAC)

### Session Cookie Authentication
- Authentication uses an **HTTP-only, Secure, SameSite Session Cookie** (`FutureMarket.Session`).
- The cookie is issued upon `POST /api/auth/register` or `POST /api/auth/login` and cleared upon `POST /api/auth/logout`.
- Frontend HTTP clients (`fetch` / `axios`) must include `credentials: "include"` (`withCredentials: true`).

### System Roles (`UserRole`)
| Role | Description | Key Capabilities |
| --- | --- | --- |
| `Guest` (Unauthenticated) | Public visitor | Browse catalog, search, filter, compare products, view Quick View, view 30/90/365-day price history, view seller profiles and reviews. |
| `User` | Individual buyer, private seller, or sole proprietor (FOP) | Buy products, manage Cart & Wishlist, place orders, leave reviews, chat with sellers, submit reports, and list own products (up to tier limit; expandable via FOP verification). |
| `Company` | Verified legal entity seller | Publish and manage products with expanded/unlimited limits, perform bulk catalog imports (`CSV`/`XLSX`/`XML`), fulfill incoming orders, view extended sales/conversion analytics. Cannot act as a buyer. |
| `Admin` | Platform administration & first-party seller | Manage users (block/unblock/roles), verify FOPs and Companies, moderate listings/reviews/reports, manage categories/attributes/brands, publish first-party `Marketplace` products, configure platform limits & commissions, view audit logs. |

---

## 3. Standardized Error Format (`400`, `401`, `403`, `404`, `500`)

All `4xx` and `5xx` responses return `application/json` using the unified [`ApiErrorResponse`](../FutureShopBackend.Application/DTO/Common/ApiErrorResponse.cs) schema:

```json
{
  "status": 400,
  "code": "VALIDATION_ERROR",
  "message": "One or more validation errors occurred.",
  "errors": {
    "email": ["The Email field is not a valid e-mail address."],
    "priceUah": ["The field PriceUah must be between 0.01 and 100000000."]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00",
  "timestamp": "2026-11-13T16:00:00Z"
}
```

### Standard HTTP Error Status Codes
| Status Code | Error Code Examples | Meaning |
| --- | --- | --- |
| `400 Bad Request` | `VALIDATION_ERROR`, `BAD_REQUEST`, `SELF_PURCHASE_FORBIDDEN`, `LISTING_LIMIT_EXCEEDED` | Malformed request syntax, failed field validation (`errors` dictionary populated), or business rule violation. |
| `401 Unauthorized` | `UNAUTHORIZED`, `SESSION_EXPIRED` | Missing, invalid, or expired `FutureMarket.Session` cookie. |
| `403 Forbidden` | `FORBIDDEN`, `ACCOUNT_BLOCKED`, `COMPANY_NOT_VERIFIED` | Authenticated user's role or verification state does not permit the action (e.g., `Company` attempting to buy, or non-owner editing another seller's product). |
| `404 Not Found` | `NOT_FOUND` | Requested entity (`Product`, `Category`, `Order`, `Seller`, `Review`, etc.) does not exist or is not visible. |
| `500 Internal Server Error` | `INTERNAL_SERVER_ERROR` | Unexpected server error; `traceId` is provided for server log correlation without leaking internal stack traces. |

---

## 4. Standardized Pagination Format

Paginated list endpoints accept `page` (default `1`) and `pageSize` (default `20`, max `100`) query parameters and return [`PagedResponse<T>`](../FutureShopBackend.Application/DTO/Common/PagedResponse.cs):

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 145,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 5. RESTful Endpoints by Module

### 5.1. Authentication (`/api/auth`) — [`AuthController`](Controllers/AuthController.cs)
| Method | Route | Access | Request Body | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/api/auth/register` | Public | `RegisterRequest` (`email`, `password`, `fullName`, `phoneNumber`, `role`: `User`\|`Company`, `companyDetails`) | `AuthSessionResponse` + `Set-Cookie` | `201`, `400`, `500` |
| `POST` | `/api/auth/login` | Public | `LoginRequest` (`email`, `password`, `rememberMe`) | `AuthSessionResponse` + `Set-Cookie` | `200`, `400`, `401`, `403`, `500` |
| `POST` | `/api/auth/logout` | Authenticated | — | `MessageResponse` (clears cookie) | `200`, `401`, `500` |
| `GET` | `/api/auth/me` | Authenticated | — | `AuthSessionResponse` | `200`, `401`, `500` |
| `POST` | `/api/auth/verify-contact` | Authenticated | `VerifyContactRequest` (`channel`, `code`) | `MessageResponse` | `200`, `400`, `401`, `500` |
| `POST` | `/api/auth/forgot-password` | Public | `ForgotPasswordRequest` (`email`) | `MessageResponse` | `200`, `400`, `500` |
| `POST` | `/api/auth/reset-password` | Public | `ResetPasswordRequest` (`email`, `resetToken`, `newPassword`) | `MessageResponse` | `200`, `400`, `500` |

### 5.2. User & Seller Profiles (`/api/users`, `/api/sellers`) — [`UsersController`](Controllers/UsersController.cs), [`SellersController`](Controllers/SellersController.cs)
| Method | Route | Access | Request Body | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/users/me` | Authenticated | — | `UserProfileResponse` | `200`, `401`, `500` |
| `PUT` | `/api/users/me` | Authenticated | `UpdateProfileRequest` (`fullName`, `phoneNumber`, `avatarUrl`, `companyName`, `address`, `websiteUrl`, `socialLinks`, `preferredLanguage`, `preferredTheme`) | `UserProfileResponse` | `200`, `400`, `401`, `500` |
| `PUT` | `/api/users/me/password` | Authenticated | `ChangePasswordRequest` (`currentPassword`, `newPassword`) | `MessageResponse` | `200`, `400`, `401`, `500` |
| `POST` | `/api/users/me/verification` | `User` (FOP), `Company` | `SubmitSellerVerificationRequest` (`isSoleProprietor`, `legalEntityName`, `taxIdentificationCode`, `legalAddress`, `bankIban`, `supportingDocumentNotes`) | `SellerVerificationResponse` | `201`, `400`, `401`, `403`, `500` |
| `GET` | `/api/users/me/analytics` | `User`, `Company`, `Admin` | — | `SellerAnalyticsResponse` | `200`, `401`, `403`, `500` |
| `DELETE` | `/api/users/me` | Authenticated | — | — | `204`, `401`, `403`, `500` |
| `GET` | `/api/sellers/{sellerId}` | Public | — | `PublicSellerProfileResponse` | `200`, `400`, `404`, `500` |

### 5.3. Categories & Brands (`/api/categories`, `/api/brands`) — [`CategoriesController`](Controllers/CategoriesController.cs), [`BrandsController`](Controllers/BrandsController.cs)
| Method | Route | Access | Request Body | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/categories` | Public | — | `IReadOnlyList<CategoryTreeDto>` | `200`, `500` |
| `GET` | `/api/categories/{categoryId}/attributes` | Public | — | `IReadOnlyList<CategoryAttributeDefinitionDto>` | `200`, `404`, `500` |
| `POST` | `/api/categories` | `Admin` | `UpsertCategoryRequest` | `CategoryTreeDto` | `201`, `400`, `401`, `403`, `500` |
| `PUT` | `/api/categories/{categoryId}` | `Admin` | `UpsertCategoryRequest` | `CategoryTreeDto` | `200`, `400`, `401`, `403`, `404`, `500` |
| `DELETE` | `/api/categories/{categoryId}` | `Admin` | — | — | `204`, `400`, `401`, `403`, `404`, `500` |
| `GET` | `/api/brands` | Public | — | `IReadOnlyList<BrandDto>` | `200`, `500` |
| `POST` | `/api/brands` | `Admin` | `UpsertBrandRequest` | `BrandDto` | `201`, `400`, `401`, `403`, `500` |
| `PUT` | `/api/brands/{brandId}` | `Admin` | `UpsertBrandRequest` | `BrandDto` | `200`, `400`, `401`, `403`, `404`, `500` |
| `DELETE` | `/api/brands/{brandId}` | `Admin` | — | — | `204`, `401`, `403`, `404`, `500` |

### 5.4. Products, Search, Quick View, Price History & Comparison (`/api/products`) — [`ProductsController`](Controllers/ProductsController.cs)
| Method | Route | Access | Request / Query | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/products` | Public | `CatalogSearchQuery` (`query`, `categoryId`, `brandId`, `sellerId`, `sellerType`: `Marketplace`\|`ThirdParty`, `condition`: `New`\|`Used`, `minPrice`, `maxPrice`, `inStockOnly`, `attributes`, `sortBy`, `page`, `pageSize`) | `PagedResponse<ProductCardDto>` | `200`, `400`, `500` |
| `GET` | `/api/products/suggestions` | Public | `?q={prefix}&limit=8` | `SearchSuggestionsResponse` | `200`, `400`, `500` |
| `GET` | `/api/products/recommendations` | Public | `?limit=12` | `IReadOnlyList<ProductCardDto>` | `200`, `500` |
| `GET` | `/api/products/compare` | Public | `?ids={id1}&ids={id2}&onlyDifferences=false` | `ProductComparisonResponse` | `200`, `400`, `404`, `500` |
| `GET` | `/api/products/{productId}` | Public | — | `ProductDetailsResponse` (PDP) | `200`, `404`, `500` |
| `GET` | `/api/products/{productId}/quick-view` | Public | — | `ProductQuickViewResponse` | `200`, `404`, `500` |
| `GET` | `/api/products/{productId}/price-history` | Public | `?periodDays=30\|90\|365` | `ProductPriceHistoryResponse` | `200`, `400`, `404`, `500` |
| `POST` | `/api/products` | `User`, `Company`, `Admin` | `UpsertProductRequest` | `ProductDetailsResponse` | `201`, `400`, `401`, `403`, `500` |
| `PUT` | `/api/products/{productId}` | Owner / `Admin` | `UpsertProductRequest` | `ProductDetailsResponse` | `200`, `400`, `401`, `403`, `404`, `500` |
| `PATCH` | `/api/products/{productId}/status` | Owner / `Admin` | `UpdateProductStatusRequest` (`status`) | `ProductDetailsResponse` | `200`, `400`, `401`, `403`, `404`, `500` |
| `DELETE` | `/api/products/{productId}` | Owner / `Admin` | — | — | `204`, `401`, `403`, `404`, `500` |
| `GET` | `/api/products/{productId}/stats` | Owner / `Admin` | — | `ProductStatisticsResponse` | `200`, `401`, `403`, `404`, `500` |
| `POST` | `/api/products/import` | `Company`, `Admin` | `BulkProductImportRequest` (`format`: `Csv`\|`Xlsx`\|`Xml`, `feedSourceUrl`, `overwriteExisting`) | `BulkProductImportResultResponse` | `200`, `400`, `401`, `403`, `500` |

### 5.5. Shopping Cart & Wishlist (`/api/cart`, `/api/wishlist`) — [`CartController`](Controllers/CartController.cs), [`WishlistController`](Controllers/WishlistController.cs)
| Method | Route | Access | Request Body | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/cart` | `User` | — | `CartResponse` (grouped by seller) | `200`, `401`, `403`, `500` |
| `POST` | `/api/cart/items` | `User` | `AddCartItemRequest` (`productId`, `quantity`) | `CartResponse` | `200`, `400`, `401`, `403`, `404`, `500` |
| `PUT` | `/api/cart/items/{productId}` | `User` | `UpdateCartItemQuantityRequest` (`quantity`) | `CartResponse` | `200`, `400`, `401`, `403`, `404`, `500` |
| `DELETE` | `/api/cart/items/{productId}` | `User` | — | `CartResponse` | `200`, `401`, `403`, `404`, `500` |
| `DELETE` | `/api/cart` | `User` | — | — | `204`, `401`, `403`, `500` |
| `GET` | `/api/wishlist` | `User` | `?page=1&pageSize=20` | `PagedResponse<WishlistItemDto>` | `200`, `400`, `401`, `500` |
| `POST` | `/api/wishlist/{productId}` | `User` | — | `WishlistItemDto` | `201`, `400`, `401`, `404`, `500` |
| `DELETE` | `/api/wishlist/{productId}` | `User` | — | — | `204`, `401`, `404`, `500` |

### 5.6. Orders & Sandbox Payments (`/api/orders`, `/api/payments`) — [`OrdersController`](Controllers/OrdersController.cs), [`PaymentsController`](Controllers/PaymentsController.cs)
| Method | Route | Access | Request Body / Query | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `POST` | `/api/orders` | `User` | `CreateOrderRequest` (`shipping`, `paymentMethod`, `sellerIds`) | `CheckoutOrdersResponse` | `201`, `400`, `401`, `403`, `500` |
| `GET` | `/api/orders/my` | `User` | `OrderFilterQuery` (`status`, `page`, `pageSize`) | `PagedResponse<OrderSummaryDto>` | `200`, `400`, `401`, `403`, `500` |
| `GET` | `/api/orders/seller` | `User`, `Company`, `Admin` | `OrderFilterQuery` (`status`, `page`, `pageSize`) | `PagedResponse<OrderSummaryDto>` | `200`, `400`, `401`, `403`, `500` |
| `GET` | `/api/orders/{orderId}` | Buyer / Seller / `Admin` | — | `OrderDetailsResponse` | `200`, `401`, `403`, `404`, `500` |
| `PATCH` | `/api/orders/{orderId}/status` | Buyer / Seller / `Admin` | `UpdateOrderStatusRequest` (`status`, `trackingNumber`, `comment`) | `OrderDetailsResponse` | `200`, `400`, `401`, `403`, `404`, `500` |
| `POST` | `/api/payments/checkout` | `User` | `InitiateTestPaymentRequest` (`orderId`) | `PaymentDetailsResponse` | `201`, `400`, `401`, `403`, `404`, `500` |
| `POST` | `/api/payments/{paymentId}/confirm` | `User` | `ConfirmTestPaymentRequest` (`simulateSuccess`, `sandboxToken`) | `PaymentDetailsResponse` | `200`, `400`, `401`, `403`, `404`, `500` |
| `GET` | `/api/payments/order/{orderId}` | Buyer / Seller / `Admin` | — | `PaymentDetailsResponse` | `200`, `401`, `403`, `404`, `500` |

### 5.7. Reviews, Buyer-Seller Chat & Reports (`/api/reviews`, `/api/conversations`, `/api/reports`) — [`ReviewsController`](Controllers/ReviewsController.cs), [`ConversationsController`](Controllers/ConversationsController.cs), [`ReportsController`](Controllers/ReportsController.cs)
| Method | Route | Access | Request Body / Query | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/reviews/product/{productId}` | Public | `PaginationQuery` | `PagedResponse<ReviewDto>` | `200`, `400`, `404`, `500` |
| `GET` | `/api/reviews/seller/{sellerId}` | Public | `PaginationQuery` | `PagedResponse<ReviewDto>` | `200`, `400`, `404`, `500` |
| `GET` | `/api/reviews/my` | `User` | `PaginationQuery` | `PagedResponse<ReviewDto>` | `200`, `400`, `401`, `500` |
| `POST` | `/api/reviews` | `User` | `CreateReviewRequest` (`productId`, `orderId`, `rating`, `comment`, `pros`, `cons`) | `ReviewDto` | `201`, `400`, `401`, `403`, `404`, `500` |
| `POST` | `/api/reviews/{reviewId}/reply` | Seller / `Admin` | `ReplyToReviewRequest` (`replyText`) | `ReviewDto` | `200`, `400`, `401`, `403`, `404`, `500` |
| `DELETE` | `/api/reviews/{reviewId}` | Author / `Admin` | — | — | `204`, `401`, `403`, `404`, `500` |
| `GET` | `/api/conversations` | Authenticated | `PaginationQuery` | `PagedResponse<ConversationSummaryDto>` | `200`, `400`, `401`, `500` |
| `POST` | `/api/conversations` | Authenticated | `StartConversationRequest` (`sellerId`, `productId`, `orderId`, `initialMessage`) | `ConversationSummaryDto` | `201`, `400`, `401`, `404`, `500` |
| `GET` | `/api/conversations/{conversationId}/messages` | Participant | `PaginationQuery` | `PagedResponse<ChatMessageDto>` | `200`, `400`, `401`, `403`, `404`, `500` |
| `POST` | `/api/conversations/{conversationId}/messages` | Participant | `SendMessageRequest` (`content`) | `ChatMessageDto` | `201`, `400`, `401`, `403`, `404`, `500` |
| `POST` | `/api/reports` | `User`, `Company`, `Admin` | `CreateReportRequest` (`targetType`, `targetId`, `reason`, `description`) | `ReportDetailsDto` | `201`, `400`, `401`, `404`, `500` |

### 5.8. Administration & Moderation (`/api/admin`) — [`AdminController`](Controllers/AdminController.cs)
| Method | Route | Access | Request Body / Query | Response Body | Status Codes |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/api/admin/users` | `Admin` | `AdminUserFilterQuery` | `PagedResponse<AdminUserSummaryDto>` | `200`, `400`, `401`, `403`, `500` |
| `PATCH` | `/api/admin/users/{userId}/status` | `Admin` | `UpdateUserStatusRequest` (`status`, `reason`) | `AdminUserSummaryDto` | `200`, `400`, `401`, `403`, `404`, `500` |
| `PATCH` | `/api/admin/users/{userId}/role` | `Admin` | `UpdateUserRoleRequest` (`role`, `reason`) | `AdminUserSummaryDto` | `200`, `400`, `401`, `403`, `404`, `500` |
| `DELETE` | `/api/admin/users/{userId}` | `Admin` | — | — | `204`, `401`, `403`, `404`, `500` |
| `GET` | `/api/admin/verifications` | `Admin` | `?status=...&page=1&pageSize=20` | `PagedResponse<SellerVerificationResponse>` | `200`, `400`, `401`, `403`, `500` |
| `PATCH` | `/api/admin/verifications/{verificationId}` | `Admin` | `ReviewVerificationRequest` (`status`, `reviewerComment`) | `SellerVerificationResponse` | `200`, `400`, `401`, `403`, `404`, `500` |
| `GET` | `/api/admin/reports` | `Admin` | `?status=...&page=1&pageSize=20` | `PagedResponse<ReportDetailsDto>` | `200`, `400`, `401`, `403`, `500` |
| `PATCH` | `/api/admin/reports/{reportId}` | `Admin` | `ResolveReportRequest` (`status`, `moderatorNote`) | `ReportDetailsDto` | `200`, `400`, `401`, `403`, `404`, `500` |
| `GET` | `/api/admin/settings` | `Admin` | — | `PlatformSettingsDto` | `200`, `401`, `403`, `500` |
| `PUT` | `/api/admin/settings` | `Admin` | `PlatformSettingsDto` | `PlatformSettingsDto` | `200`, `400`, `401`, `403`, `500` |
| `GET` | `/api/admin/analytics` | `Admin` | — | `PlatformAnalyticsResponse` | `200`, `401`, `403`, `500` |
| `GET` | `/api/admin/audit-logs` | `Admin` | `PaginationQuery` | `PagedResponse<AuditLogEntryDto>` | `200`, `400`, `401`, `403`, `500` |
