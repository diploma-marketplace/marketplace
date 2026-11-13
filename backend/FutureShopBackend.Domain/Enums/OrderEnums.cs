namespace FutureShopBackend.Domain.Enums;

/// <summary>
/// Order lifecycle status.
/// </summary>
public enum OrderStatus
{
    Created,
    Paid,
    ConfirmedBySeller,
    Shipped,
    Delivered,
    Completed,
    Cancelled,
    Returned
}

/// <summary>
/// Payment processing status for an order.
/// </summary>
public enum PaymentStatus
{
    Pending,
    Succeeded,
    Failed,
    Refunded
}

/// <summary>
/// Supported payment methods (test mode only in MVP).
/// </summary>
public enum PaymentMethod
{
    TestCard,
    CashOnDelivery
}

/// <summary>
/// Supported order delivery options.
/// </summary>
public enum DeliveryMethod
{
    Courier,
    PostOffice,
    Pickup
}
