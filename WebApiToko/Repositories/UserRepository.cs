using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApiToko.Data;
using WebApiToko.Dtos;
using WebApiToko.Interfaces;
using WebApiToko.ModelsEF.Toko;

namespace WebApiToko.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        //private readonly TokoDbContext _tokoDbContext;

        public UserRepository(UserManager<ApplicationUser> userManager, TokoDbContext tokoDbContext) 
        {
            _userManager = userManager;
            //_tokoDbContext = tokoDbContext;
        }
        
        public async Task<bool> RegisterUserAsync(UserRegisterDto model)
        {
            var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                throw new BadHttpRequestException($"{string.Join(", ", result.Errors.Select(e => e.Description))}");
            }

            return result.Succeeded;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
    }
}
