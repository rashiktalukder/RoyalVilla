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
        public async Task<ActionResult<IEnumerable<Villa>>> GetVillas()
        {
            return Ok(await _db.Villa.ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Villa>> GetVillaById(int id)
        {
            try
            {
                if(id <= 0)
                {
                    return BadRequest("Villa Id Must be greater than 0");
                }

                var villa = await _db.Villa.FirstOrDefaultAsync(u=>u.Id == id);

                return Ok(villa);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<Villa>> CreateVilla(VillaCreateDTO villaDto)
        {
            try
            {
                if (villaDto == null)
                {
                    return BadRequest("Villa Data Required");
                }

                Villa villa = _mapper.Map<Villa>(villaDto);

                await _db.Villa.AddAsync(villa);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(CreateVilla), new { id = villa.Id, villa });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
