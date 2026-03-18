using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto model )
        {
            if (!ModelState.IsValid)    
                return BadRequest(ModelState);

            var result = await _userService.LoginUserAsync(model);

            if (result != null)
            {
                return Ok(new ApiResponse<LoginResponseDto>
                (
                    "Success",
                    "User logged in successfully",
                    result
                ));
            }
            return BadRequest(result);
        }
        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.RefreshTokenAsync(request);
            return Ok(result);
        }
        //[Authorize]
        //[HttpPost("Logout")]
        //public async Task<IActionResult> Logout()
        //{
        //    var authHeader = Request.Headers["Authorization"].ToString();
        //    if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        //        return BadRequest("Invalid token");

        //    var token = authHeader["Bearer ".Length..];
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        //    if (string.IsNullOrEmpty(userId))
        //        return BadRequest("User ID not found");

        //    await _userService.LogoutUserAsync(token, userId);
        //    return Ok(new ApiResponse<string>
        //        (
        //            "Success",
        //            "User logged out successfully",
        //            null
        //        ));
        //}
        [Authorize]
        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            var accessToken = Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(accessToken) || !accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Missing or invalid Authorization header.");
            }
            accessToken = accessToken["Bearer ".Length..].Trim();

            await _userService.LogoutUserAsync(accessToken);
            return Ok(new { message = "Logged out successfully." });
        }

        [Authorize]
        [HttpPost("LogoutAll")]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest("User not authenticated.");

            await _userService.LogoutAllUserAsync(userId);
            return Ok(new { message = "Logged out from all devices." });
        }
    }
}
