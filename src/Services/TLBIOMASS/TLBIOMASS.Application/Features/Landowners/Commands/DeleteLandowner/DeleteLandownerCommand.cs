using MediatR;

namespace TLBIOMASS.Application.Features.Landowners.Commands.DeleteLandowner;

public record DeleteLandownerCommand(int Id) : IRequest<bool>;
