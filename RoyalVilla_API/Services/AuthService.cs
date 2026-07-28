using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RoyalVilla_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
        }
        
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _db.ApplicationUsers.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequestDTO.Email);

                if (user == null)
                {
                    return null;
                }

                var isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

                if (!isValid)
                {
                    return null;
                }

                //generate token
                var token = await GenerateJwtToken(user);
                var roles = await _userManager.GetRolesAsync(user);

                LoginResponseDTO loginResponseDTO = new LoginResponseDTO
                {
                    Token = token,
                    UserDTO = _mapper.Map<UserDTO>(user)
                };
                loginResponseDTO.UserDTO.Role = roles.FirstOrDefault() ?? "Customer";

                return loginResponseDTO;
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public async Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                if (await IsEmailExistsAsync(registrationRequestDTO.Email))
                {
                    throw new InvalidOperationException($"User with this email: {registrationRequestDTO.Email} already exists!");
                }

                ApplicationUser user = new()
                {
                    Email = registrationRequestDTO.Email,
                    Name = registrationRequestDTO.Name,
                    UserName = registrationRequestDTO.Email,
                    NormalizedEmail = registrationRequestDTO.Email.ToUpper(),
                    EmailConfirmed = true,
                };

                var res = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
                if(!res.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to create user: {string.Join(", ", res.Errors.Select(e => e.Description))}");
                }

                var role = string.IsNullOrEmpty(registrationRequestDTO.Role) ? "Customer" : registrationRequestDTO.Role;
                if(!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);

                var userDto = _mapper.Map<UserDTO>(user);
                userDto.Role = role;

                return userDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);
            var roles = await _userManager.GetRolesAsync(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
