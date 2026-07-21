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
        public async Task<ActionResult<String>> GetVillas()
        {
            return "This is V2";
        }

        [HttpGet("{id:int}")]
        
        public async Task<ActionResult<String>> GetVillaById(int id)
        {
            try
            {
                return "This is V2" + id;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
