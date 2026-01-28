using MediatR;

namespace TLBIOMASS.Application.Features.Agencies.Commands.DeleteAgency;

public record DeleteAgencyCommand(int Id) : IRequest<bool>;
