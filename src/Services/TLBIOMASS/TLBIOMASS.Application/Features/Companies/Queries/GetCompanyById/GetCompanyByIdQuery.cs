using MediatR;
using Shared.DTOs.Company;

namespace TLBIOMASS.Application.Features.Companies.Queries.GetCompanyById;

public class GetCompanyByIdQuery : IRequest<CompanyResponseDto>
{
    public int Id { get; set; }
}
