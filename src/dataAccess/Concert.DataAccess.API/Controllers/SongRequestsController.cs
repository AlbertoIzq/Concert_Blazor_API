using AutoMapper;
using Concert.Business.Constants;
using Concert.Business.Models.Domain;
using Concert.DataAccess.API.Helpers;
using Concert.DataAccess.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Concert.DataAccess.API.Controllers
{
    [ApiController]
    [Route("api/song-requests")]
    //[Authorize]
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

        /// <summary>
        /// POST: api/songrequests
        /// </summary>
        /// <param name="addSongRequestDto"></param>
        /// <returns></returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Authorize(Roles = BackConstants.WRITER_ROLE_NAME)]
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = BackConstants.READER_ROLE_NAME + "," + BackConstants.ADMIN_ROLE_NAME)]
        public async Task<IActionResult> GetAll([FromQuery] bool includeSoftDeleted = false)
        {
            LoggerHelper<SongRequestsController>.LogCalledEndpoint(_logger, HttpContext);

            // Only Admin role can retrieve soft deleted entities
            if (includeSoftDeleted && !User.IsInRole(BackConstants.ADMIN_ROLE_NAME))
            {
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "You don't have permission to access this resource."
                };

                // Return a 403 Forbidden response
                return new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            // Get all song requests
            var songRequestsDomainModel = await _unitOfWork.SongRequests.GetAllAsync(includeSoftDeleted);

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
        [Authorize(Roles = BackConstants.READER_ROLE_NAME)]
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

            LoggerHelper<SongRequestsController>.LogResultEndpoint(_logger, HttpContext, "Ok", songRequestDto);

            // Return DTO back to client
            return Ok(songRequestDto);
        }

        /// <summary>
        /// PUT: api/songrequests/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateSongRequestDto"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = BackConstants.WRITER_ROLE_NAME)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateSongRequestDto updateSongRequestDto)
        {
            LoggerHelper<SongRequestsController>.LogCalledEndpoint(_logger, HttpContext);

            // Map DTO to Domain Model
            var songRequestDomainModel = _mapper.Map<SongRequest>(updateSongRequestDto);
            
            // Update SongRequest if it exists
            songRequestDomainModel = await _unitOfWork.SongRequests.UpdateAsync(id, songRequestDomainModel);
            await _unitOfWork.SaveAsync();

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

            LoggerHelper<SongRequestsController>.LogResultEndpoint(_logger, HttpContext, "Ok", songRequestDto);

            return Ok(songRequestDto);
        }

        /// <summary>
        /// DELETE: api/songrequests/{id}
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hardDelete"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = BackConstants.WRITER_ROLE_NAME + "," + BackConstants.ADMIN_ROLE_NAME)]
        public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] bool hardDelete = false)
        {
            LoggerHelper<SongRequestsController>.LogCalledEndpoint(_logger, HttpContext);

            // Only Admin role can do a hard delete
            if (hardDelete && !User.IsInRole(BackConstants.ADMIN_ROLE_NAME))
            {
                var problemDetails = new ProblemDetails()
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "You don't have permission to access this resource."
                };

                // Return a 403 Forbidden response
                return new ObjectResult(problemDetails)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            SongRequest? songRequestDomainModel;
            // Perform a hard or soft delete from database - Domain Model
            if (hardDelete)
            {
                songRequestDomainModel = await _unitOfWork.SongRequests.HardDeleteAsync(id);
            }
            else
            {
                songRequestDomainModel = await _unitOfWork.SongRequests.SoftDeleteAsync(id);
            }
            await _unitOfWork.SaveAsync();

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

            LoggerHelper<SongRequestsController>.LogResultEndpoint(_logger, HttpContext, "No Content");

            // Return No content back to client
            return NoContent();
        }

        /// <summary>
        /// PUT: api/songrequests/{id}/restore
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{id}/restore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = BackConstants.ADMIN_ROLE_NAME)]
        public async Task<IActionResult> Restore([FromRoute] int id)
        {
            LoggerHelper<SongRequestsController>.LogCalledEndpoint(_logger, HttpContext);

            // Restore from database - Domain Model
            var songRequestDomainModel = await _unitOfWork.SongRequests.RestoreAsync(id);
            await _unitOfWork.SaveAsync();

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

            LoggerHelper<SongRequestsController>.LogResultEndpoint(_logger, HttpContext, "Ok", songRequestDto);

            // Return DTO back to client
            return Ok(songRequestDto);
        }
    }
}