namespace FutureShopBackend.Domain.Entities;

// У БД зберігаються рядками в нижньому регістрі: "fop", "unpaid", "new" тощо
// (конвертер описаний в AppDbContext.Lower<TEnum>()).

/// <summary>ФОП / Компанія (asp_net_users.status_type, може бути NULL).</summary>
public enum SellerStatusType
{
    Fop,
    Company
}

public enum PaymentStatus
{
    Unpaid,
    Paid,
    Refunded
}

public enum OrderStatus
{
    New,
    Processing,
    Shipped,
    Completed,
    Cancelled
}

public enum OrderItemStatus
{
    New,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}
