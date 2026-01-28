using System.Linq;
using MediatR;
using TLBIOMASS.Domain.Landowners.Interfaces;
using Shared.DTOs.Landowner;
using Shared.SeedWork;
using Mapster;
using TLBIOMASS.Domain.Landowners;
using TLBIOMASS.Domain.Landowners.Specifications;

namespace TLBIOMASS.Application.Features.Landowners.Queries.GetLandowners;

public class GetLandownersQueryHandler : IRequestHandler<GetLandownersQuery, PagedList<LandownerResponseDto>>
{
    private readonly ILandownerRepository _repository;

    public GetLandownersQueryHandler(ILandownerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedList<LandownerResponseDto>> Handle(GetLandownersQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAll();

        query = ApplyFilter(query, request.Filter);

        if (!string.IsNullOrEmpty(request.Filter.Search))
        {
            var spec = new LandownerSearchSpecification(request.Filter.Search);
            query = query.Where(spec.ToExpression());
        }

        if (request.Filter.IsActive.HasValue)
        {
            var spec = new LandownerIsActiveSpecification(request.Filter.IsActive.Value);
            query = query.Where(spec.ToExpression());
        }

        query = ApplySort(query, request.Filter.OrderBy, request.Filter.OrderByDirection);

        var pagedItems = await _repository.GetPageAsync(query, request.Filter.PageNumber, request.Filter.PageSize, cancellationToken);

        return new PagedList<LandownerResponseDto>(
            pagedItems.Adapt<List<LandownerResponseDto>>(),
            pagedItems.GetMetaData().TotalItems,
            request.Filter.PageNumber, request.Filter.PageSize);
    }

    private static IQueryable<Landowner> ApplyFilter(IQueryable<Landowner> query, LandownerPagedFilterDto filter)
    {
        if (filter == null) return query;

        if (filter.IsActive.HasValue)
            query = query.Where(x => x.IsActive == filter.IsActive.Value);

        return query;
    }

    private static IQueryable<Landowner> ApplySort(IQueryable<Landowner> query, string? orderBy, string? direction)
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query.OrderBy(x => x.Id);

        var isDescending = direction?.ToLower() == "desc";

        return orderBy.ToLower() switch
        {
            "name" => isDescending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),
            "phone" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Phone : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Phone : null),
            "email" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Email : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Email : null),
            "address" => isDescending
                ? query.OrderByDescending(x => x.Contact != null ? x.Contact.Address : null)
                : query.OrderBy(x => x.Contact != null ? x.Contact.Address : null),
            "bankname" => isDescending
                ? query.OrderByDescending(x => x.Bank != null ? x.Bank.BankName : null)
                : query.OrderBy(x => x.Bank != null ? x.Bank.BankName : null),
            "isactive" => isDescending
                ? query.OrderByDescending(x => x.IsActive)
                : query.OrderBy(x => x.IsActive),
            "createddate" => isDescending
                ? query.OrderByDescending(x => x.CreatedDate)
                : query.OrderBy(x => x.CreatedDate),
            "lastmodifieddate" => isDescending
                ? query.OrderByDescending(x => x.LastModifiedDate)
                : query.OrderBy(x => x.LastModifiedDate),
            _ => query.OrderBy(x => x.Id)
        };
    }
}
