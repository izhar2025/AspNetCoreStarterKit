using AutoMapper;
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Sample;

public record UpdateSampleCommand : IRequest<ApiResponse<SampleDto>>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateSampleCommandHandler : IRequestHandler<UpdateSampleCommand, ApiResponse<SampleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateSampleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SampleDto>> Handle(UpdateSampleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<SampleEntity>().GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
            return ApiResponse<SampleDto>.NotFound($"Entity with ID {request.Id} not found");

        var duplicate = await _unitOfWork.Repository<SampleEntity>()
            .AnyAsync(e => e.Name == request.Name && e.Id != request.Id, cancellationToken);

        if (duplicate)
            return ApiResponse<SampleDto>.Fail($"Entity with name '{request.Name}' already exists");

        _mapper.Map(request, entity);
        _unitOfWork.Repository<SampleEntity>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<SampleDto>(entity);
        return ApiResponse<SampleDto>.Ok(dto, "Updated successfully");
    }
}

public class UpdateSampleCommandValidator : AbstractValidator<UpdateSampleCommand>
{
    public UpdateSampleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}