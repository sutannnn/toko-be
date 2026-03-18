using WebApiToko.Dtos;

namespace WebApiToko.Interfaces.Services
{
    public interface IUserService
    {
        public Task<bool> RegisterUserAsync(UserRegisterDto model);
        public Task<LoginResponseDto> LoginUserAsync(UserLoginDto model);
        public Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequest request);
        public Task<bool> LogoutUserAsync(string accessToken);
        public Task<bool> LogoutAllUserAsync(string userId);
    }
}
