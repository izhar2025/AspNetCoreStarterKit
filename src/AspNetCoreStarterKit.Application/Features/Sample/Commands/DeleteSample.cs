using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Sample;

public record DeleteSampleCommand(int Id) : IRequest<ApiResponse<object>>;

public class DeleteSampleCommandHandler : IRequestHandler<DeleteSampleCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteSampleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> Handle(DeleteSampleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _unitOfWork.Repository<SampleEntity>().GetByIdAsync(request.Id, cancellationToken);
        if (entity == null)
            return ApiResponse<object>.NotFound($"Entity with ID {request.Id} not found");

        entity.IsActive = false;
        _unitOfWork.Repository<SampleEntity>().Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<object>.Ok(null, "Deleted successfully");
    }
}

public class DeleteSampleCommandValidator : AbstractValidator<DeleteSampleCommand>
{
    public DeleteSampleCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
    }
}