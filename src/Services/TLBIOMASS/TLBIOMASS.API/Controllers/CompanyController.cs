using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLBIOMASS.Application.Features.Companies.Commands.CreateCompany;
using TLBIOMASS.Application.Features.Companies.Commands.UpdateCompany;
using TLBIOMASS.Application.Features.Companies.Commands.DeleteCompany;
using TLBIOMASS.Application.Features.Companies.Queries.GetCompanies;
using TLBIOMASS.Application.Features.Companies.Queries.GetCompaniesPaged;
using TLBIOMASS.Application.Features.Companies.Queries.GetCompanyById;
using Shared.DTOs.Company;
using Mapster;
using Infrastructure.Common;
using Shared.SeedWork;

namespace TLBIOMASS.API.Controllers
{
    [ApiVersion("1.0")]
    public class CompanyController : ApiControllerBase<CompanyController>
    {
        private const string EntityName = "Company";

        public CompanyController(IMediator mediator, ILogger<CompanyController> logger)
            : base(mediator, logger)
        {
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiSuccessResult<List<CompanyResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetList([FromQuery] CompanyFilterDto filter)
        {
            var query = new GetCompaniesQuery { Filter = filter };
            return await HandleGetAllAsync<GetCompaniesQuery, CompanyResponseDto>(query, EntityName);
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiSuccessResult<List<CompanyResponseDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagedList([FromQuery] CompanyPagedFilterDto filter)
        {
            var query = new GetCompaniesPagedQuery { Filter = filter };
            return await HandleGetPagedAsync<GetCompaniesPagedQuery, CompanyResponseDto>(
                query, EntityName, filter.PageNumber, filter.PageSize);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<CompanyResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<CompanyResponseDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetCompanyByIdQuery { Id = id };
            return await HandleGetByIdAsync<GetCompanyByIdQuery, CompanyResponseDto>(query, id, EntityName);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiSuccessResult<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CompanyCreateDto dto)
        {
            var command = dto.Adapt<CreateCompanyCommand>();
            return await HandleCreateAsync(command, EntityName, dto.CompanyName);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyUpdateDto dto)
        {
            var command = dto.Adapt<UpdateCompanyCommand>();
            command.Id = id;
            return await HandleUpdateAsync(command, id, dto.Id, EntityName);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiSuccessResult<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResult<bool>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteCompanyCommand(id);
            return await HandleDeleteAsync(command, id, EntityName);
        }
    }
}
