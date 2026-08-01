using MediatR;
using Microsoft.EntityFrameworkCore;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Sample;

public class GetAllSamplesQuery : PaginationParams, IRequest<PagedResult<SampleDto>>;

public class GetAllSamplesQueryHandler : IRequestHandler<GetAllSamplesQuery, PagedResult<SampleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllSamplesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<SampleDto>> Handle(GetAllSamplesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<SampleEntity>().Query();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(e => e.Name.Contains(request.SearchTerm) ||
                                     (e.Description != null && e.Description.Contains(request.SearchTerm)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
            "createdon" => request.SortDescending ? query.OrderByDescending(e => e.CreatedOn) : query.OrderBy(e => e.CreatedOn),
            _ => query.OrderBy(e => e.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new SampleDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                IsActive = e.IsActive,
                CreatedOn = e.CreatedOn
            })
            .ToListAsync(cancellationToken);

        return PagedResult<SampleDto>.Create(items, totalCount, request.PageNumber, request.PageSize);
    }
}