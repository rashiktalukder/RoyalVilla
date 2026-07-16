using System.ComponentModel.DataAnnotations;

namespace RoyalVilla_API.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public required string Email { get; set; } = default!;
        public required string Name { get; set; } = default!;
        public required string Role { get; set; } = default!;
    }
}
