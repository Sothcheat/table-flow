namespace TableFlow.Web.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; } = "";
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
