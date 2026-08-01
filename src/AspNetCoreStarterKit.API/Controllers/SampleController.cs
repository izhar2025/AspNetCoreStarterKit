using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs;
using AspNetCoreStarterKit.Application.Features.Sample;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.API.Controllers;

[Authorize]
public class SampleController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SampleDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var query = new GetAllSamplesQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            SearchTerm = search,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SampleDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetSampleByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<SampleDto>>> Create(CreateSampleCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<SampleDto>>> Update(int id, UpdateSampleCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.Fail("ID mismatch"));

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteSampleCommand(id));
        return HandleResult(result);
    }

    [HttpPost("bulk-upload")]
    public async Task<ActionResult<ApiResponse<BulkUploadResult>>> BulkUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Please upload a file"));

        using var stream = file.OpenReadStream();
        var command = new BulkUploadSamplesCommand
        {
            FileStream = stream,
            FileName = file.FileName
        };

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("bulk-upload/template")]
    public IActionResult DownloadTemplate()
    {
        var excelService = HttpContext.RequestServices.GetRequiredService<IExcelTemplateService>();
        var template = excelService.Generate<SampleBulkUploadDto>("Sample");
        return File(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sample_Template.xlsx");
    }
}