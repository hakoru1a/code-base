using MediatR;

namespace TLBIOMASS.Application.Features.MaterialRegions.Commands.DeleteMaterialRegion;

public record DeleteMaterialRegionCommand(int Id) : IRequest<bool>;
