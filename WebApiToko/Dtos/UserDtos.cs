namespace WebApiToko.Dtos
{
    public class UserRegisterDto
    {
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string NIK { get; set; }
        public DateTime BirthDate { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
    public class  UserLoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Expires { get; set; } = string.Empty;
    }
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}
