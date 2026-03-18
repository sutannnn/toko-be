using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Pipelines;
using System.Security.Claims;
using System.Text;
using WebApiToko.Data;
using WebApiToko.Dtos;
using WebApiToko.Helper;
using WebApiToko.Interfaces;
using WebApiToko.ModelsEF.Toko;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebApiToko.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppIdentityDbContext _appIdentityDbContext;

        public UserRepository(UserManager<ApplicationUser> userManager, IConfiguration configuration, AppIdentityDbContext appIdentityDbContext) 
        {
            _userManager = userManager;
            _configuration = configuration;
            _appIdentityDbContext = appIdentityDbContext;
        }
        
        public async Task<bool> RegisterUserAsync(UserRegisterDto model)
        {
            using var transaction = await _appIdentityDbContext.Database.BeginTransactionAsync();
            try
            {
                // 1. Buat user secara manual (tanpa UserManager.CreateAsync)
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    NormalizedUserName = model.UserName.Normalize(),
                    Email = model.Email,
                    NormalizedEmail = model.Email.Normalize(),
                    PasswordHash = _userManager.PasswordHasher.HashPassword(null!, model.Password)
                };

                _appIdentityDbContext.Users.Add(user); // Menyimpan ke DbSet<IdentityUser>

                // 2. Simpan user dulu agar dapat Id
                await _appIdentityDbContext.SaveChangesAsync();

                // 3. Simpan UserProfile
                var userProfile = new UserProfile
                {
                    UserId = user.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.BirthDate,
                    Address = model.Address,
                    Nik = model.NIK,
                    CreatedBy = "System",
                    CreatedAt = DateTime.Now
                };
                _appIdentityDbContext.UserProfiles.Add(userProfile);

                // 4. Simpan semuaS
                await _appIdentityDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"{ex.InnerException.Message}, {ex.Message}");
            }
        }

        public async Task<bool> UserEmailExistsAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email) != null;
        }
        public async Task<bool> UserNameExistsAsync(string name)
        {
            var result = await _userManager.FindByNameAsync(name);

            return result != null;
        }

        public async Task<LoginResponseDto> LoginUserAsync(UserLoginDto model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            var isPasswordValid = await _userManager.CheckPasswordAsync(user!, model.Password);
            if (!isPasswordValid)
            {
                throw new BadHttpRequestException("Username atau Password salah!");
            }
            //var token = GenerateJwtToken(user!);
            var (accessToken, jti) = GenerateJwtToken(user);
            var refreshToken = await GenerateRefreshTokenAsync(jti, user.Id);
            var loginResult = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserName = user.UserName!,
                Expires = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss")
            };

            return loginResult;
        }
        private (string token, string jti) GenerateJwtToken(ApplicationUser user)
        {
            var jti = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, jti),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            // Tambahkan role jika ada
            //var roles = await _userManager.GetRolesAsync(user);
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JWT")["Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var issuer = _configuration.GetSection("JWT")["ValidIssuer"];
            var audience = _configuration.GetSection("JWT")["ValidAudience"];

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), jti);
        }
        private async Task<string> GenerateRefreshTokenAsync(string jwtId, string userId)
        {
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                JwtId = jwtId,
                UserId = userId,
                ExpiryDate = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now
            };

            _appIdentityDbContext.RefreshTokens.Add(refreshToken);
            await _appIdentityDbContext.SaveChangesAsync();
            return refreshToken.Token;
        }
        //public async Task<bool> LogoutUserAsync(string token, string userId)
        //{
        //    var tokenHash = HashHelper.ComputeSha256Hash(token);
        //    var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        //    _appIdentityDbContext.RevokedTokens.Add(new RevokeToken
        //    {
        //        TokenHash = tokenHash,
        //        UserId = userId, // ← SIMPAN USER ID
        //        ExpiresAt = jwtToken.ValidTo
        //    });
        //    await _appIdentityDbContext.SaveChangesAsync();
        //    return true;
        //}
        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                throw new BadHttpRequestException("Invalid access token.");

            var jti = principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
                throw new BadHttpRequestException("Invalid token claims.");

            var storedRefreshToken = await _appIdentityDbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken && rt.UserId == userId);

            if (storedRefreshToken == null || storedRefreshToken.Invalidated || storedRefreshToken.ExpiryDate < DateTime.Now)
                throw new BadHttpRequestException("Invalid or expired refresh token.");

            if (storedRefreshToken.JwtId != jti)
                throw new BadHttpRequestException("Token mismatch.");

            // Invalidate old refresh token
            storedRefreshToken.Invalidated = true;
            await _appIdentityDbContext.SaveChangesAsync();

            // Load the ApplicationUser and generate new tokens
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new BadHttpRequestException("User not found.");

            var (newAccessToken, newJti) = GenerateJwtToken(user);
            var newRefreshToken = await GenerateRefreshTokenAsync(newJti, userId);

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserName = user.UserName ?? "",
                Expires = DateTime.Now.AddHours(1).ToString("yyyy-MM-dd HH:mm:ss")
            };
        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var validIssuer = _configuration["JWT:ValidIssuer"];
            var validAudience = _configuration["JWT:ValidAudience"];
            var secret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret missing");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(validIssuer),
                ValidIssuer = string.IsNullOrEmpty(validIssuer) ? null : validIssuer,

                ValidateAudience = !string.IsNullOrEmpty(validAudience),
                ValidAudience = string.IsNullOrEmpty(validAudience) ? null : validAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ValidateLifetime = false // allow expired token for refresh
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            }
            catch
            {
                return null;
            }
        }
        public async Task<bool> RevokeTokenAsync(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(accessToken!))
                return false;

            var jwtToken = handler.ReadJwtToken(accessToken!);
            var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;

            if (string.IsNullOrEmpty(jti))
                return false;

            var token = await _appIdentityDbContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.JwtId == jti);
            if (token != null)
            {
                token.Invalidated = true;
                await _appIdentityDbContext.SaveChangesAsync();
            }
            return true;
        }
        public async Task<bool> RevokeAllTokensAsync(string userId)
        {
            var tokens = await _appIdentityDbContext.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.Invalidated)
                .ToListAsync();

            foreach (var token in tokens)
                token.Invalidated = true;

            await _appIdentityDbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IsTokenRevokedAsync(string jti)
        {
            return await _appIdentityDbContext.RefreshTokens
                .AnyAsync(rt => rt.JwtId == jti && rt.Invalidated);
        }
    }
}
