namespace TableFlow.Api.DTOs
{
    public record CreateUserRequest(string Fullname, string Email, string Password, string Role);

    public record UpdateUserRequest(string Fullname, string Email, string Role, string? NewPassword);

    public record UserResponse(string Id, string Fullname, string Email, string Role);

    public record LoginRequest(string Email, string Password);

    public record LoginResponse(
        string Token,
        string UserId,
        string FullName,
        string Email,
        string Role
    );
}
