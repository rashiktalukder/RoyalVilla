using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;

namespace RoyalVilla_API.Controllers.V2
{
    [Route("api/v{version:apiVersion}/villa")]
    [ApiVersion("2.0")]
    [ApiController]
    //[Authorize(Roles = "Admin,Customer")]
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
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDTO>>>> GetVillas([FromQuery] string? filterBy, [FromQuery] string? filterQuery, [FromQuery] string? sortBy, [FromQuery] string? sortOrder = "asc", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if(page < 1)
            {
                page = 1;
            }
            if(pageSize<1)
            {
                pageSize = 10;
            }
            if(pageSize > 100)
            {
                pageSize = 100;
            }

            var villasQuery = _db.Villa.AsQueryable();
            if(!string.IsNullOrEmpty(filterQuery) && !string.IsNullOrEmpty(filterBy))
            {
                switch(filterBy.ToLower())
                {
                    case "name":
                        villasQuery = villasQuery.Where(x => x.Name.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "details":
                        villasQuery = villasQuery.Where(x => x.Details.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "rate":
                        if (double.TryParse(filterQuery, out double rate))
                        {
                            villasQuery = villasQuery.Where(x => x.Rate == rate);
                        }
                        break;
                    case "minrate":
                        if (double.TryParse(filterQuery, out double minrate))
                        {
                            villasQuery = villasQuery.Where(x => x.Rate >= minrate);
                        }
                        break;
                    case "maxrate":
                        if (double.TryParse(filterQuery, out double maxrate))
                        {
                            villasQuery = villasQuery.Where(x => x.Rate <= maxrate);
                        }
                        break;
                    case "occupancy":
                        if (int.TryParse(filterQuery, out int occupancy))
                        {
                            villasQuery = villasQuery.Where(x => x.Occupancy == occupancy);
                        }
                        break;

                    default:
                        break;
                }
            }
            //sorting
            if (!string.IsNullOrEmpty(sortBy))
            {
                var isDescending = sortOrder?.ToLower() == "desc";
                villasQuery = sortBy.ToLower() switch
                {
                    "name" => isDescending ? villasQuery.OrderByDescending(x => x.Name) : villasQuery.OrderBy(x => x.Name),
                    "rate" => isDescending ? villasQuery.OrderByDescending(x => x.Rate) : villasQuery.OrderBy(x => x.Rate),
                    "occupancy" => isDescending ? villasQuery.OrderByDescending(x => x.Occupancy) : villasQuery.OrderBy(x => x.Occupancy),
                    "sqft" => isDescending ? villasQuery.OrderByDescending(x => x.Sqft) : villasQuery.OrderBy(x => x.Sqft),
                    "id" => isDescending ? villasQuery.OrderByDescending(x => x.Id) : villasQuery.OrderBy(x => x.Id),
                    _ => villasQuery.OrderBy(x => x.Id)
                };
            }
            else
            {
                villasQuery = villasQuery.OrderBy(x => x.Id);
            }

            var skip = (page - 1) * pageSize;
            var totalCount = await villasQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            var villas = await villasQuery.Skip(skip).Take(pageSize).ToListAsync();
            var dtoResponseVilla = _mapper.Map<List<VillaDTO>>(villas);

            var messageBuilder = new System.Text.StringBuilder();
            messageBuilder.Append($"Successfully Retrived {dtoResponseVilla.Count} villas.");
            messageBuilder.Append($" Page {page} of {totalPages}, {totalCount} total records.");

            if (!string.IsNullOrEmpty(filterQuery) && !string.IsNullOrEmpty(filterBy))
            {
                messageBuilder.Append($" Filtered by {filterBy} with query '{filterQuery}'.");
            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                messageBuilder.Append($" Sorted by {sortBy} ");
            }

            Response.Headers.Add("X-Pagination-CurrentPage", page.ToString());
            Response.Headers.Add("X-Pagination-PageSizes", pageSize.ToString());
            Response.Headers.Add("X-Pagination-TotalCount", totalCount.ToString());
            Response.Headers.Add("X-Pagination-TotalPages", totalPages.ToString());

            var response = ApiResponse<IEnumerable<VillaDTO>>.Ok(dtoResponseVilla, messageBuilder.ToString());
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<VillaDTO>>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return NotFound(ApiResponse<object>.NotFound("Villa ID must be greater than 0"));
                }

                var villa = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);

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
                }
                ;

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

                if (villaDto.Id != id)
                {
                    return BadRequest("Villa Id Mismatch");
                }

                var existingVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Id == id);
                if (existingVilla == null)
                {
                    return NotFound($"villa with id:{id} not found.");
                }

                var duplicateVilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower() && u.Id != id);

                if (duplicateVilla != null)
                {
                    return Conflict($"villa with name:{villaDto.Name} already exists.");
                }
                ;

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
