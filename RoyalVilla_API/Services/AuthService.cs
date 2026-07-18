using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext db, IMapper mapper, IConfiguration configuration)
        {
            _db = db;
            _mapper = mapper;
            _configuration = configuration;
        }
        
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());

                if (user == null || user.Password != loginRequestDTO.Password)
                {
                    return null;
                }

                //generate token
                var token = GenerateJwtToken(user);

                return new LoginResponseDTO
                {
                    UserDTO = _mapper.Map<UserDTO>(user),
                    Token = token
                };
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

                User user = new()
                {
                    Email = registrationRequestDTO.Email,
                    Name = registrationRequestDTO.Name,
                    Password = registrationRequestDTO.Password,
                    Role = string.IsNullOrEmpty(registrationRequestDTO.Role) ? "Customer" : registrationRequestDTO.Role,
                    CreatedDate = DateTime.Now
                };

                await _db.Users.AddAsync(user);
                await _db.SaveChangesAsync();

                return _mapper.Map<UserDTO>(user);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Role, user.Role),
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
