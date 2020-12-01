using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SocialDistancing.API.Common.Attributes;
using SocialDistancing.API.DataContracts;
using SocialDistancing.API.DataContracts.Helpers;
using SocialDistancing.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using S = SocialDistancing.Services.Model;
using SocialDistancing.API.Common.Middlewares;

namespace SocialDistancing.API.Controllers.V1
{
    [APIKeyAuth]
    [ApiVersion("1.0")]
    [Route("api/terminals")]//required for default versioning
    [Route("api/v{version:apiVersion}/terminals")]
    [ApiController]
    public class TerminalController : Controller
    {
        private readonly ITerminalService _service;
        private readonly IMapper _mapper;
        private readonly ILogger<TerminalController> _logger;
        private readonly IApiKeyValidation _validation;

        public TerminalController(ITerminalService service, IMapper mapper, ILogger<TerminalController> logger, IApiKeyValidation validation)
        {
            _service = service;
            _mapper = mapper;
            _logger = logger;
            _validation = validation;
        }

        #region GET
        /// <summary>
        /// Returns a collection of terminal entities.
        /// </summary>
        /// <remarks>
        /// XML comments included in controllers will be extracted and injected in Swagger/OpenAPI file.
        /// </remarks>
        /// <param></param>
        /// <returns>
        /// Returns a collection of terminal entities.
        /// </returns>
        /// <response code="201">Returns the terminal list.</response>
        /// <response code="204">If the item is null.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Terminal>))]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(List<Terminal>))]
        [HttpGet()]
        public async Task<List<Terminal>> GetAll(string locationId)
        {
            _logger.LogDebug($"TerminalControllers::Get::");

            var data = await _service.GetAsync();

            if (data != null)
            {
                if (locationId != null)
                    data = data.FindAll(t => t.LocationID == locationId);

                return _mapper.Map<List<Terminal>>(data);
            }
            else
                return null;
        }

        /// <summary>
        /// Returns a terminal entity according to the provided Id.
        /// </summary>
        /// <remarks>
        /// XML comments included in controllers will be extracted and injected in Swagger/OpenAPI file.
        /// </remarks>
        /// <param name="samNumber"></param>
        /// <param name="field"></param>
        /// <returns>
        /// Returns a terminal entity according to the provided Id.
        /// </returns>
        /// <response code="201">Returns the terminal.</response>
        /// <response code="204">If the item is null.</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Terminal))]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(Terminal))]
        [HttpGet("{samNumber}")]
        public async Task<Terminal> Get(string samNumber, string field)
        {
            _logger.LogDebug($"TerminalControllers::Get::{samNumber}");

            var data = await _service.GetAsync(samNumber);

            if (data != null)
            {
                var terminal = _mapper.Map<Terminal>(data);
                if (field != null)
                {
                    switch (field.ToLower())
                    {
                        case "location":
                            return new Terminal() { LocationID = terminal.LocationID };
                        case "in_use":
                            return new Terminal() { InUse = terminal.InUse };
                        default:
                            break;
                    }
                    return null;
                }
                else
                {
                    return terminal;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Returns the activity of a terminal and its neighbors
        /// </summary>
        /// <remarks>
        /// XML comments included in controllers will be extracted and injected in Swagger/OpenAPI file.
        /// </remarks>
        /// <param name="samNumber"></param>
        /// <param name="detailed"></param>
        /// <returns>
        /// Returns the activity of a terminal and its neighbors
        /// </returns>
        /// <response code="201">Returns the activity of a terminal and its neighbors</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TerminalAvailability))]
        [HttpGet("{samNumber}/in-use")]
        public async Task<TerminalAvailability> Get(string samNumber, bool detailed = false)
        {
            _logger.LogDebug($"TerminalControllers::Get::{samNumber}");

            var data = await _service.GetAsync(samNumber);
            if (data != null)
            {
                var availability = new TerminalAvailability();

                var terminal = _mapper.Map<Terminal>(data);
                availability.InUse = terminal.InUse;

                if (detailed)
                {
                    var leftTerminal = await _service.GetAsync(terminal.LeftNeighborSamNumber);
                    if (leftTerminal != null)
                    {
                        availability.LeftInUse = leftTerminal.InUse;
                    }
                    var rightTerminal = await _service.GetAsync(terminal.RightNeighborSamNumber);
                    if (rightTerminal != null)
                    {
                        availability.RightInUse = rightTerminal.InUse;
                    }
                }               
                return availability;
            }
            else
                return null;
        }

        /// <summary>
        /// Returns the terminal avaibility
        /// </summary>
        /// <remarks>
        /// XML comments included in controllers will be extracted and injected in Swagger/OpenAPI file.
        /// </remarks>
        /// <param name="samNumber"></param>
        /// <returns>
        /// Returns the terminal avaibility
        /// </returns>
        /// <response code="201">Returns the terminal avaibility</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        [HttpGet("{samNumber}/available")]
        public async Task<bool> Get(string samNumber)
        {
            _logger.LogDebug($"TerminalControllers::Get::{samNumber}::available");

            Response.Headers.Add("Authorization", "Axes " + _validation.GenerateApiKey());

            var terminal = await _service.GetAsync(samNumber);
            if (terminal != null)
            {
                var availability = true;

                var leftTerminal = await _service.GetAsync(terminal.LeftNeighborSamNumber);
                if ((leftTerminal != null))
                {
                    availability &= !leftTerminal.InUse;
                }

                if (availability)
                {
                    var rightTerminal = await _service.GetAsync(terminal.RightNeighborSamNumber);
                    if ((rightTerminal != null))
                    {
                        availability &= !rightTerminal.InUse;
                    }
                }
                return availability;
            }
            else
                return true;
        }
        #endregion

        #region POST
        /// <summary>
        /// Creates a terminal neighbor entry.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <param name="terminal"></param>
        /// <returns>A newly created terminal.</returns>
        /// <response code="201">Returns the newly created item.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Terminal))]
        public async Task<Terminal> CreateTerminal([FromBody] Terminal terminal)
        {
            _logger.LogDebug($"TerminalControllers::Post::");

            if (terminal == null)
                throw new ArgumentNullException("terminal");

            var terminalCheck = await _service.GetAsync(terminal.SamNumber);

            if (terminalCheck != null && terminalCheck.LocationID == terminal.LocationID)
            {
                return null;
            }
            else 
            {
                var data = await _service.CreateAsync(_mapper.Map<S.Terminal>(terminal));

                if (data != null)
                    return _mapper.Map<Terminal>(data);
                else
                    return null;
            }
        }

        /// <summary>
        /// Updates the terminal available property
        /// </summary>
        /// <param name="samNumber"></param>
        /// <param name="occupied"></param>
        /// <returns>Returns a boolean notifying if the terminal has been updated properly.</returns>
        /// /// <response code="200">Returns a boolean notifying if the terminal has been updated properly.</response>
        [HttpPost("{samNumber}/available")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<bool> UpdateStatus(string samNumber, bool occupied)
        {
            Response.Headers.Add("Authorization", "Axes " + _validation.GenerateApiKey());
            var terminal = await _service.GetAsync(samNumber);
            if (terminal != null)
            {
                terminal.InUse = occupied;
                return await _service.UpdateAsync(samNumber,terminal);
            }
            return false;
        }

        #endregion

        #region DELETE
        /// <summary>
        /// Deletes a terminal entity.
        /// </summary>
        /// <remarks>
        /// No remarks.
        /// </remarks>
        /// <param name="id">Terminal Id</param>
        /// <returns>
        /// Boolean notifying if the terminal has been deleted properly.
        /// </returns>
        /// <response code="200">Boolean notifying if the terminal has been deleted properly.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<bool> DeleteTerminal(string id)
        {
            var data = await _service.GetAsync(id);
            if (data != null)
            {
                return await _service.DeleteAsync(id);
            }
            return false;
        }
        #endregion

        #region PUT
        /// <summary>
        /// Updates a terminal entity.
        /// </summary>
        /// <remarks>
        /// No remarks.
        /// </remarks>
        /// <param name="samNumber"></param>
        /// <param name="terminal"></param>
        /// <returns>
        /// Returns a boolean notifying if the terminal has been updated properly.
        /// </returns>
        /// <response code="200">Returns a boolean notifying if the terminal has been updated properly.</response>
        [HttpPut("{samNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<bool> UpdateTerminal(string samNumber, [FromBody] Terminal terminal)
        {
            if (terminal == null)
                throw new ArgumentNullException("terminal");

            var data = await _service.GetAsync(samNumber);
            if (data != null)
            {
                return await _service.UpdateAsync(samNumber, _mapper.Map<S.Terminal>(terminal));
            }
            return false;
        }
        #endregion

        #region PATCH
        /// <summary>
        /// Updates properties of a terminal entity.
        /// </summary>
        /// <remarks>
        /// No remarks.
        /// </remarks>
        /// <param name="samNumber"></param>
        /// <param name="terminal"></param>
        /// <returns>
        /// Returns a boolean notifying if the terminal has been updated properly.
        /// </returns>
        /// <response code="200">Returns a boolean notifying if the terminal has been updated properly.</response>
        [HttpPatch("{samNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<bool> PatchTerminal(string samNumber, [FromBody] TerminalPatch terminal)
        {
            if (terminal == null)
                throw new ArgumentNullException("patch");

            var data = await _service.GetAsync(samNumber);
            if (data != null)
            {
                return await _service.PatchAsync(samNumber, _mapper.Map<S.TerminalPatch>(terminal));
            }
            return false;
        }     
        #endregion
    }
}
