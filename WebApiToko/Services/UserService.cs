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
            var userExists = await _userRepository.UserExistsAsync(model.Email);
            if (userExists)
            {
                throw new BadHttpRequestException("User already exists.");
            }
            var registerResult = await _userRepository.RegisterUserAsync(model);
            return registerResult;
        }
    }
}
