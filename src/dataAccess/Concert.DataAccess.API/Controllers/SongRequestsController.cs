using AutoMapper;
using Concert.Business.Models.Domain;
using Concert.DataAccess.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Concert.DataAccess.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SongRequestsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SongRequestsController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        // GET: api/songrequests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Get all song requests
            var songRequestsDomainModel = await _unitOfWork.SongRequests.GetAllAsync();

            // Map Domain Model to DTO
            var songRequestsDto = _mapper.Map<List<SongRequestDto>>(songRequestsDomainModel);

            // Return DTO to the client
            return Ok(songRequestsDto);
        }
    }
}