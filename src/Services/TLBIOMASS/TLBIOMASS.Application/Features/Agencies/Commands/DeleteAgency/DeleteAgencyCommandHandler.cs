using MediatR;
using TLBIOMASS.Domain.Agencies.Interfaces;
using Contracts.Exceptions;

namespace TLBIOMASS.Application.Features.Agencies.Commands.DeleteAgency;

public class DeleteAgencyCommandHandler : IRequestHandler<DeleteAgencyCommand, bool>
{
    private readonly IAgencyRepository _repository;

    public DeleteAgencyCommandHandler(IAgencyRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteAgencyCommand request, CancellationToken cancellationToken)
    {
        var agency = await _repository.GetByIdAsync(request.Id);

        if (agency == null)
        {
            throw new NotFoundException("Agency", request.Id);
        }

        await _repository.DeleteAsync(agency);
        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
