namespace TableFlow.Api.DTOs
{

    public record CategoryResponse(
        int Id,
        string CategoryName,
        int DisplayOrder,
        string StationName,
        int ItemCount
    );

    public record CreateCategoryRequest(
        string CategoryName,
        int DisplayOrder,
        string StationName
    );

    public record UpdateCategoryRequest(
        string CategoryName,
        int DisplayOrder,
        string StationName
    );

    public record MenuItemResponse(
        int Id,
        string ItemName,       
        decimal BasePrice,     
        bool IsAvailable,
        bool HasVarient,
        string? ImageUrl,
        int CategoryId,
        string CategoryName,
        List<MenuItemVarientResponse> Varients
    );

    public record CreateMenuItemRequest(
        string ItemName,       
        decimal BasePrice,     
        bool IsAvailable,
        bool HasVarient,
        string? ImageUrl,
        int CategoryId
    );

    public record UpdateMenuItemRequest(
        string ItemName,
        decimal BasePrice,
        bool IsAvailable,
        bool HasVarient,
        string? ImageUrl,
        int CategoryId
    );

    public record MenuItemVarientResponse(
        int Id,
        int MenuItemId,
        string VarientName,
        decimal Price,
        bool IsAvailable
    );

    public record CreateMenuItemVarientRequest(
        string VarientName,
        decimal Price,
        bool IsAvailable
    );

    public record UpdateMenuItemVarientRequest(
        string VarientName,
        decimal Price,
        bool IsAvailable
    );

    public record UpdateItemAvailabilityRequest(bool IsAvailable);
}
