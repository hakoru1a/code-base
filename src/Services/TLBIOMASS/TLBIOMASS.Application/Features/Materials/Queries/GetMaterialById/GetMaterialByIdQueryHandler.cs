using MediatR;
using TLBIOMASS.Domain.Materials.Interfaces;
using Shared.DTOs.Material;
using Contracts.Exceptions;
using Mapster;

namespace TLBIOMASS.Application.Features.Materials.Queries.GetMaterialById;

public class GetMaterialByIdQueryHandler : IRequestHandler<GetMaterialByIdQuery, MaterialResponseDto>
{
    private readonly IMaterialRepository _repository;

    public GetMaterialByIdQueryHandler(IMaterialRepository repository)
    {
        _repository = repository;
    }

    public async Task<MaterialResponseDto> Handle(GetMaterialByIdQuery request, CancellationToken cancellationToken)
    {
        var material = await _repository.GetByIdAsync(request.Id);
        if (material == null)
        {
            throw new NotFoundException("Material", request.Id);
        }
        return material.Adapt<MaterialResponseDto>();
    }
}
