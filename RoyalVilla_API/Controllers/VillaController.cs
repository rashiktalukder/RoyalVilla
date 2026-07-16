using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers
{
    [Route("api/villa")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas()
        {
            var villas = await _db.Villa.ToListAsync();
            var dtoResponseVilla = _mapper.Map<List<VillaDTO>>(villas);
            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoResponseVilla, "Villas Retrived Successfully");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
        {
            try
            {
                if(id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0"));
                }

                var villa = await _db.Villa.FirstOrDefaultAsync(u=>u.Id == id);

                return Ok(ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villa), "Records Retrived Successfully"));

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> CreateVilla(VillaCreateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Villa Data is null"));
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower());

                if (duplicateVilla != null)
                {
                    return Conflict(ApiResponse<object>.Conflict($"villa with name:{villaDto.Name} already exists."));
                };

                Villa villa = _mapper.Map<Villa>(villaDto);

                await _db.Villa.AddAsync(villa);
                await _db.SaveChangesAsync();

                var response = ApiResponse<VillaDTO>.CreatedAt(_mapper.Map<VillaDTO>(villa), "Villa Created Successfully");
                return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id }, response);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VillaDTO>> UpdateVilla(int id, VillaUpdateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                {
                    return BadRequest("Villa Data Required");
                }

                if(villaDto.Id != id)
                {
                    return BadRequest("Villa Id Mismatch");
                }

                var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                if(existingVilla == null)
                {
                    return NotFound($"villa with id:{id} not found.");
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower() && u.Id != id);

                if(duplicateVilla != null)
                {
                    return Conflict($"villa with name:{villaDto.Name} already exists.");
                };

                _mapper.Map(villaDto, existingVilla);

                existingVilla.UpdatedDate = DateTime.Now;

                await _db.SaveChangesAsync();
                var response = ApiResponse<VillaDTO>.Ok(_mapper.Map<VillaDTO>(villaDto), "Villa Updated Successfully");
                return Ok(villaDto);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVilla(int id)
        {
            try
            {
                var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return NotFound(ApiResponse<object>.NotFound($"villa with id:{id} not found."));
                }

                _db.Villa.Remove(existingVilla);
                await _db.SaveChangesAsync();

                return Ok(ApiResponse<object>.NoContent("Villa Deleted Siccessfully"));
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
