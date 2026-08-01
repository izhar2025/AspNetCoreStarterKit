using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Sample;

public record GetSampleByIdQuery(int Id) : IRequest<ApiResponse<SampleDto>>;

public class GetSampleByIdQueryHandler : IRequestHandler<GetSampleByIdQuery, ApiResponse<SampleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSampleByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<SampleDto>> Handle(GetSampleByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<SampleEntity>().GetByIdAsync(request.Id, cancellationToken);

        if (entity == null)
            return ApiResponse<SampleDto>.NotFound($"Entity with ID {request.Id} not found");

        var dto = new SampleDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedOn = entity.CreatedOn
        };

        return ApiResponse<SampleDto>.Ok(dto);
    }
}