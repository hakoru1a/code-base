using MediatR;

namespace TLBIOMASS.Application.Features.Materials.Commands.DeleteMaterial;

public record DeleteMaterialCommand(int Id) : IRequest<bool>;
