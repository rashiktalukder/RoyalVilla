using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla_API.Models.DTO;
using RoyalVilla_API.Services;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody]RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                //auth service
                if (registrationRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration data is required."));
                }

                if (await _authService.IsEmailExistsAsync(registrationRequestDTO.Email))
                {
                    return Conflict(ApiResponse<object>.Conflict($"User with this email: {registrationRequestDTO.Email} already exists!"));
                }

                var user = await _authService.RegisterAsync(registrationRequestDTO);
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("User registration failed."));
                }

                var response = ApiResponse<UserDTO>.CreatedAt(user, "User Registered Successfully");
                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost("Login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            try
            {
                //auth service
                if (loginRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login data is required."));
                }

                var loginResponse = await _authService.LoginAsync(loginRequestDTO);
                if (loginResponse == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("User login failed."));
                }

                var response = ApiResponse<LoginResponseDTO>.Ok(loginResponse, "User Logged In Successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
