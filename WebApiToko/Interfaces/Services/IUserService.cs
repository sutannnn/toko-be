using WebApiToko.Dtos;

namespace WebApiToko.Interfaces.Services
{
    public interface IUserService
    {
        public Task<bool> RegisterUserAsync(UserRegisterDto model);
    }
}
