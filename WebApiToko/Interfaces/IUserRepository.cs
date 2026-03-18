using System.IdentityModel.Tokens.Jwt;
using WebApiToko.Data;
using WebApiToko.Dtos;
using WebApiToko.Helper;
using WebApiToko.ModelsEF.Toko;

namespace WebApiToko.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> RegisterUserAsync(UserRegisterDto model);
        public Task<bool> UserEmailExistsAsync(string email);
        public Task<bool> UserNameExistsAsync(string name);
        public Task<LoginResponseDto> LoginUserAsync(UserLoginDto model);
        public Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequest request);
        //public Task<bool> LogoutUserAsync(string token, string userId);
        public Task<bool> RevokeTokenAsync(string accessToken);
        public Task<bool> RevokeAllTokensAsync(string userId);
        Task<bool> IsTokenRevokedAsync(string jti);
    }
}
