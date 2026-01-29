using MediatR;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.Domain.ValueObjects;

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
                new ContactInfo(request.Phone, request.Email, request.Address, null),
                new IdentityInfo(request.IdentityCardNo, request.IssuePlace, request.IssueDate, request.DateOfBirth),
                request.Status);

            // Create polymorphic BankAccount if provided and add to collection
            if (!string.IsNullOrWhiteSpace(request.BankAccount))
            {
                landowner.AddBankAccount(
                    request.BankName ?? string.Empty,
                    request.BankAccount,
                    true
                );
            }

            await _repository.CreateWithoutSaveAsync(landowner, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return (long)landowner.Id;
        }
    }
}
