using FluentValidation;
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs;
using AspNetCoreStarterKit.Application.Interfaces;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Sample;

public record BulkUploadSamplesCommand : IRequest<ApiResponse<BulkUploadResult>>
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
}

public class BulkUploadSamplesCommandHandler : IRequestHandler<BulkUploadSamplesCommand, ApiResponse<BulkUploadResult>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBulkUploadService _bulkUploadService;

    public BulkUploadSamplesCommandHandler(IUnitOfWork unitOfWork, IBulkUploadService bulkUploadService)
    {
        _unitOfWork = unitOfWork;
        _bulkUploadService = bulkUploadService;
    }

    public async Task<ApiResponse<BulkUploadResult>> Handle(BulkUploadSamplesCommand request, CancellationToken cancellationToken)
    {
        var result = new BulkUploadResult();

        var data = await _bulkUploadService.ParseAsync<SampleBulkUploadDto>(request.FileStream, result, cancellationToken);

        if (!data.Any())
            return ApiResponse<BulkUploadResult>.Fail("No valid rows found");

        var existingNames = (await _unitOfWork.Repository<SampleEntity>()
            .FindAsync(e => data.Select(d => d.Name).Contains(e.Name), cancellationToken))
            .Select(e => e.Name)
            .ToHashSet();

        foreach (var item in data)
        {
            if (existingNames.Contains(item.Name))
            {
                result.AddError(item.RowNumber, "Name", item.Name, "Name already exists");
                result.Failed++;
                continue;
            }

            var entity = new SampleEntity
            {
                Name = item.Name,
                Description = item.Description,
                IsActive = true
            };

            await _unitOfWork.Repository<SampleEntity>().AddAsync(entity, cancellationToken);
            existingNames.Add(item.Name);
            result.Success++;
        }

        result.FinalizeResult();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<BulkUploadResult>.Ok(result, $"Upload complete: {result.Success} succeeded, {result.Failed} failed");
    }
}

public class BulkUploadSamplesCommandValidator : AbstractValidator<BulkUploadSamplesCommand>
{
    public BulkUploadSamplesCommandValidator()
    {
        RuleFor(x => x.FileStream).NotNull();
        RuleFor(x => x.FileName).NotEmpty().Must(x => x.EndsWith(".xlsx") || x.EndsWith(".xls"));
    }
}