using WebApiToko.Dtos;
using WebApiToko.Interfaces;
using WebApiToko.Interfaces.Services;

namespace WebApiToko.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository) 
        {
            _userRepository = userRepository;
        }
        public async Task<bool> RegisterUserAsync(UserRegisterDto model)
        {
            var userExists = await _userRepository.UserEmailExistsAsync(model.Email);
            if (userExists)
            {
                throw new BadHttpRequestException("User already exists.");
            }
            var registerResult = await _userRepository.RegisterUserAsync(model);
            return registerResult;
        }
        public async Task<LoginResponseDto> LoginUserAsync(UserLoginDto model)
        {
            var userExists = await _userRepository.UserNameExistsAsync(model.UserName);
            if (!userExists)
            {
                throw new BadHttpRequestException("User not found");
            }
            var loginResult = await _userRepository.LoginUserAsync(model);

            return loginResult;
        }
        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequest model)
        {
            var loginResult = await _userRepository.RefreshTokenAsync(model);

            return loginResult;
        }

        public async Task<bool> LogoutUserAsync(string accessToken)
        {
            var loginResult = await _userRepository.RevokeTokenAsync(accessToken);

            return loginResult;
        }
        public async Task<bool> LogoutAllUserAsync(string userId)
        {
            var loginResult = await _userRepository.RevokeAllTokensAsync(userId);

            return loginResult;
        }
    }
}
