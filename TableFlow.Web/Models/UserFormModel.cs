namespace TableFlow.Web.Models
{
    public class UserFormModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsEditMode { get; set; } = false;
        public string? NewPassword { get; set; }
    }
}
