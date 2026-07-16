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

        [HttpPost]
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
    }
}
