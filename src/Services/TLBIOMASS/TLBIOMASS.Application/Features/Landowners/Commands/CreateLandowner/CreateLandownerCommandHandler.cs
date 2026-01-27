using MediatR;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Interfaces;

namespace TLBIOMASS.Application.Features.Landowners.Commands.CreateLandowner
{
    public class CreateLandownerCommandHandler : IRequestHandler<CreateLandownerCommand, long>
    {
        private readonly ILandownerRepository _repository;

        public CreateLandownerCommandHandler(ILandownerRepository repository)
        {
            _repository = repository;
        }

        public async Task<long> Handle(CreateLandownerCommand request, CancellationToken cancellationToken)
        {
            var landowner = Landowner.Create(
                request.Name,
                request.Phone,
                request.Email,
                request.Address,
                request.BankAccount,
                request.BankName,
                request.IdentityCardNo,
                request.IssuePlace,
                request.IssueDate,
                request.DateOfBirth,
                request.IsActive
            );

            //landowner.AddDomainEvent(new LandownerCreatedEvent(landowner.Id, landowner.Name));

            await _repository.CreateAsync(landowner);
            await _repository.SaveChangesAsync(cancellationToken);

            return (long)landowner.Id;
        }
    }
}
