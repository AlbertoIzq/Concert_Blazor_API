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
        private readonly ILogger<SongRequestsController> _logger;

        public SongRequestsController(IUnitOfWork unitOfWork, IMapper mapper,
            ILogger<SongRequestsController> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // GET: api/songrequests
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Called endpoint '{endpoint}'", "SongRequests.GetAll");

            // Get all song requests
            var songRequestsDomainModel = await _unitOfWork.SongRequests.GetAllAsync();

            // Map Domain Model to DTO
            var songRequestsDto = _mapper.Map<List<SongRequestDto>>(songRequestsDomainModel);

            _logger.LogInformation("Result endpoint '{endpoint}': {result}, response: {@response}",
                "SongRequests.GetAll", "OK",songRequestsDto);

            // Return DTO to the client
            return Ok(songRequestsDto);
        }
    }
}