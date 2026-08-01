using AutoMapper;
using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Sample;

// ─── Command ────────────────────────────────────────────────────────────────

public record CreateSampleCommand : IRequest<ApiResponse<SampleDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

// ─── Handler ────────────────────────────────────────────────────────────────

public class CreateSampleCommandHandler : IRequestHandler<CreateSampleCommand, ApiResponse<SampleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateSampleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<SampleDto>> Handle(CreateSampleCommand request, CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.Repository<SampleEntity>()
            .AnyAsync(e => e.Name == request.Name, cancellationToken);

        if (exists)
            return ApiResponse<SampleDto>.Fail($"Entity with name '{request.Name}' already exists");

        var entity = _mapper.Map<SampleEntity>(request);
        await _unitOfWork.Repository<SampleEntity>().AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = _mapper.Map<SampleDto>(entity);
        return ApiResponse<SampleDto>.Created(dto, "Created successfully");
    }
}

// ─── Validator ──────────────────────────────────────────────────────────────

public class CreateSampleCommandValidator : AbstractValidator<CreateSampleCommand>
{
    public CreateSampleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}