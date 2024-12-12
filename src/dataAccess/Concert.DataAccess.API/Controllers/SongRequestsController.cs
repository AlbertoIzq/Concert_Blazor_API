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
            _logger.LogInformation("Called endpoint '{endpoint}'",
                "SongRequests.GetAll");

            // Get all song requests
            var songRequestsDomainModel = await _unitOfWork.SongRequests.GetAllAsync();

            // Map Domain Model to DTO
            var songRequestsDto = _mapper.Map<List<SongRequestDto>>(songRequestsDomainModel);

            // Instead of @response you could use as a param JsonSerializer.Serialize(songRequestsDto))
            // but then the type is not saved in the json
            _logger.LogInformation("Result endpoint '{endpoint}': {result}, response: {@response}",
                "SongRequests.GetAll", "OK",songRequestsDto);

            // Return DTO to the client
            return Ok(songRequestsDto);
        }

        // GET: api/songrequests/{id}
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            _logger.LogInformation("Called endpoint '{endpoint}' with id: {id}",
                "SongRequests.GetById", id);

            // Get data from database - Domain Model
            var songRequestDomainModel = await _unitOfWork.SongRequests.GetByIdAsync(id);

            if (songRequestDomainModel == null)
            {
                _logger.LogInformation("Result endpoint '{endpoint}' with id: {id}: {result}",
                "SongRequests.GetById", id, "Not Found");

                return NotFound();
            }

            // Convert Domain Model to DTO
            var songRequestDto = _mapper.Map<SongRequestDto>(songRequestDomainModel);

            _logger.LogInformation("Result endpoint '{endpoint}' with id: {id}: {result}, response: {@response}",
                "SongRequests.GetById", id, "OK", songRequestDto);

            // Return DTO back to client
            return Ok(songRequestDto);
        }
    }
}