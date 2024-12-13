using AutoMapper;
using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Helpers;
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

        [ProducesResponseType(StatusCodes.Status201Created)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AddSongRequestDto addSongRequestDto)
        {
            LoggerHelper<SongRequestsController>.LogCalledEndpoint(_logger, HttpContext);

            // Map or Convert DTO to Domain Model
            var songRequestDomainModel = _mapper.Map<SongRequest>(addSongRequestDto);

            // Use Domain Model to create Artist
            songRequestDomainModel = await _unitOfWork.SongRequests.CreateAsync(songRequestDomainModel);
            await _unitOfWork.SaveAsync();

            // Map Domain Model back to DTO
            var songRequestDto = _mapper.Map<SongRequestDto>(songRequestDomainModel);

            LoggerHelper<SongRequestsController>.LogResultEndpoint(_logger, HttpContext, "Created", songRequestDto);

            // Show information to the client
            return CreatedAtAction(nameof(GetById), new { id = songRequestDto.Id }, songRequestDto);
        }

        /// <summary>
        /// GET: api/songrequests
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            LoggerHelper<SongRequestsController>.LogCalledEndpoint(_logger, HttpContext);

            // Get all song requests
            var songRequestsDomainModel = await _unitOfWork.SongRequests.GetAllAsync();

            // Map Domain Model to DTO
            var songRequestsDto = _mapper.Map<List<SongRequestDto>>(songRequestsDomainModel);

            LoggerHelper<SongRequestsController>.LogResultEndpoint(_logger, HttpContext, "OK", songRequestsDto);

            // Return DTO to the client
            return Ok(songRequestsDto);
        }

        /// <summary>
        /// GET: api/songrequests/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            LoggerHelper<SongRequestsController>.LogCalledEndpoint(_logger, HttpContext);

            // Get data from database - Domain Model
            var songRequestDomainModel = await _unitOfWork.SongRequests.GetByIdAsync(id);

            if (songRequestDomainModel == null)
            {
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Item not found.",
                    Detail = $"The item with id '{id}' couldn't be found."                  
                };

                LoggerHelper<SongRequestsController>.LogResultEndpoint(_logger, HttpContext, "Not Found", problemDetails);

                return NotFound(problemDetails);
            }

            // Convert Domain Model to DTO
            var songRequestDto = _mapper.Map<SongRequestDto>(songRequestDomainModel);

            _logger.LogInformation("Result endpoint '{method}', '{endpoint}': '{result}', response: {@response}",
                HttpContext.Request.Method, HttpContext.Request.Path, "OK", songRequestDto);

            // Return DTO back to client
            return Ok(songRequestDto);
        }
    }
}