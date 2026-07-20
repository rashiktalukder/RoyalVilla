using System.ComponentModel.DataAnnotations;

namespace RoyalVilla_API.Models.DTO.VillaAmenities
{
    public class VillaAmenitiesCreateDTO
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        public string? Description { get; set; }

        public int VillaId { get; set; }
    }   
}
