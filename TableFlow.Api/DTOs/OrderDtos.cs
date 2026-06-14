namespace TableFlow.Api.DTOs
{
    public record OrderItemRequest(
        int MenuItemId,
        int? VarientId,
        int Quantity,
        string? Note
    );

    public record CreateOrderRequest(
        int SessionId,
        string? Note,
        List<OrderItemRequest> Items
    );

    public record UpdateOrderStatusRequest(
        string Status
    );

    public record UpdateOrderItemStatusRequest(
        string Status
    );

    public record OrderItemResponse(
        int Id,
        int OrderId,
        int MenuItemId,
        string ItemName,
        int? VarientId,
        string? VarientName,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        string OrderItemStatus,
        string? Note
    );

    public record OrderResponse(
        int Id,
        int SessionId,
        string OrderNumber,
        string OrderStatus,
        DateTime CreatedAt,
        string? Note,
        List<OrderItemResponse> Items,
        decimal OrderTotal
    );
}
