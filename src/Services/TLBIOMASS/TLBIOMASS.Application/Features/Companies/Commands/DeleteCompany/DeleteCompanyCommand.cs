using MediatR;

namespace TLBIOMASS.Application.Features.Companies.Commands.DeleteCompany;

public class DeleteCompanyCommand : IRequest<bool>
{
    public int Id { get; set; }

    public DeleteCompanyCommand(int id)
    {
        Id = id;
    }
}
