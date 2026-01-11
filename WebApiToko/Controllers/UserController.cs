using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebApiToko.Data;
using WebApiToko.Dtos;
using WebApiToko.Interfaces.Services;

namespace WebApiToko.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _userService.RegisterUserAsync(model);

            if (result)
            {
                return Ok(new ApiResponse<UserRegisterDto>
                (
                    "Success",
                    "User registered successfully",
                    model
                ));
            }

            return BadRequest(result);
        }
    }
}
