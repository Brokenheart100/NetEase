namespace NetEase.Dtos
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class LoginResponse
    {
        public string Message { get; set; }
        public UserInfo User { get; set; }
        public string Token { get; set; }
    }

    public class ErrorResponse
    {
        public string Message { get; set; }
    }
}
