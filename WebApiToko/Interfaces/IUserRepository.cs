using WebApiToko.Dtos;
using WebApiToko.ModelsEF.Toko;

namespace WebApiToko.Interfaces
{
    public interface IUserRepository
    {
        public Task<bool> RegisterUserAsync(UserRegisterDto model);
        public Task<bool> UserExistsAsync(string email);
    }
}
