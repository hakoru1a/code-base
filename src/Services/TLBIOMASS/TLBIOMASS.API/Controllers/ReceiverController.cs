using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Receivers.Commands.CreateReceiver;
using TLBIOMASS.Application.Features.Receivers.Commands.UpdateReceiver;
using TLBIOMASS.Application.Features.Receivers.Commands.DeleteReceiver;
using TLBIOMASS.Application.Features.Receivers.Queries.GetAllReceivers;
using TLBIOMASS.Application.Features.Receivers.Queries.GetReceivers;
using TLBIOMASS.Application.Features.Receivers.Queries.GetReceiverById;
using Shared.DTOs.Receiver;
using Mapster;
using Infrastructure.Common;
using Shared.SeedWork;

namespace TLBIOMASS.API.Controllers
{
    [ApiVersion("1.0")]
    public class ReceiverController : ApiControllerBase<ReceiverController>
    {
        private const string EntityName = "Receiver";

        public ReceiverController(IMediator mediator, ILogger<ReceiverController> logger)
            : base(mediator, logger)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<ReceiverResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] ReceiverFilterDto filter)
        {
            var query = new GetAllReceiversQuery { Filter = filter };
            return await HandleGetAllAsync<GetAllReceiversQuery, ReceiverResponseDto>(query, EntityName);
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<ReceiverResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedList([FromQuery] ReceiverPagedFilterDto filter)
        {
            var query = new GetReceiversQuery { Filter = filter };
            return await HandleGetPagedAsync<GetReceiversQuery, ReceiverResponseDto>(
                query, EntityName, filter.PageNumber, filter.PageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<ReceiverResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<ReceiverResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetReceiverByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetReceiverByIdQuery, ReceiverResponseDto>(query, id, EntityName);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<int>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ReceiverCreateDto dto)
        {
            var command = dto.Adapt<CreateReceiverCommand>();
            return await HandleCreateAsync(command, EntityName, dto.Name);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] ReceiverUpdateDto dto)
        {
            var command = dto.Adapt<UpdateReceiverCommand>();
            command.Id = id;
            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteReceiverCommand(id);
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
