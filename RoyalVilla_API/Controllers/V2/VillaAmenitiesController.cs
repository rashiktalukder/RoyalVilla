using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using RoyalVilla_API.Models.DTO.VillaAmenities;

namespace RoyalVilla_API.Controllers.V2
{
    [Route("api/v{version:apiVersion}/villa-amenities")]
    [ApiVersion("2.0")]
    [ApiController]
    public class VillaAmenitiesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAmenitiesController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaAmenitiesDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaAmenitiesDTO>>>> GetVillaAmenities()
        {
            var villas = await _db.VillaAmenities.ToListAsync();
            var dtoResponseVillaAmenities = _mapper.Map<List<VillaAmenitiesDTO>>(villas);
            var response = ApiResponse<IEnumerable<VillaAmenitiesDTO>>.Ok(dtoResponseVillaAmenities, "Villas Retrived Successfully");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> GetVillaAmenitiesById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("VillaAmenities ID must be greater than 0"));
                }

                var villa = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);

                return Ok(ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(villa), "Records Retrived Successfully"));

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<VillaAmenitiesDTO>>> CreateVillaAmenities(VillaAmenitiesCreateDTO villaAmenitiesDTO)
        {
            try
            {
                if (villaAmenitiesDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("VillaAmenities Data is null"));
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == villaAmenitiesDTO.VillaId);

                if (duplicateVilla == null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"villa with Id:{villaAmenitiesDTO.VillaId} does not exists."));
                }
                ;

                VillaAmenities villaAmenities = _mapper.Map<VillaAmenities>(villaAmenitiesDTO);
                villaAmenities.CreatedDate = DateTime.Now;

                await _db.VillaAmenities.AddAsync(villaAmenities);
                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaAmenitiesDTO>.CreatedAt(_mapper.Map<VillaAmenitiesDTO>(villaAmenities), "Villa-Amenities Created Successfully");
                return CreatedAtAction(nameof(CreateVillaAmenities), new { id = villaAmenities.Id }, response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaAmenitiesDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VillaAmenitiesDTO>> UpdateVillaAmenities(int id, VillaAmenitiesUpdateDTO villaAmenitiesDTO)
        {
            try
            {
                if (villaAmenitiesDTO == null)
                {
                    return BadRequest("VillaAmenities Data Required");
                }

                if (villaAmenitiesDTO.Id != id)
                {
                    return BadRequest("VillaAmenities Id Mismatch");
                }

                var existingVilla = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return NotFound($"villa with id:{id} not found.");
                }

                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(x => x.Id == id);
                if(existingVillaAmenities==null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"Villa-Amenities with id: {id} was not found"));
                }

                _mapper.Map(villaAmenitiesDTO, existingVillaAmenities);

                existingVillaAmenities.UpdatedDate = DateTime.Now;

                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaAmenitiesDTO>.Ok(_mapper.Map<VillaAmenitiesDTO>(existingVillaAmenities), "VillaAmenities Updated Successfully");
                return Ok(villaAmenitiesDTO);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenities(int id)
        {
            try
            {
                var existingVillaAmenities = await _db.VillaAmenities.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVillaAmenities == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"villa with id:{id} not found."));
                }

                _db.VillaAmenities.Remove(existingVillaAmenities);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<object>.NoContent("VillaAmenities Deleted Siccessfully"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }








    }
}
