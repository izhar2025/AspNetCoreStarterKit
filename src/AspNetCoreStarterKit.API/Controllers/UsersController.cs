// AspNetCoreStarterKit.API/Controllers/UsersController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreStarterKit.API.Attributes;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Features.Users;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.API.Controllers;

[Authorize]
[Route("api/v1/users")]
[RequirePermission("ManageUsers")]
public class UsersController : BaseApiController
{
    [HttpGet]
    [RequirePermission("ViewUsers")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserDto>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? roleId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isLockedOut = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            RoleId = roleId,
            IsActive = isActive,
            IsLockedOut = isLockedOut,
            SearchTerm = search,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }

    [HttpGet("{id}")]
    [RequirePermission("ViewUsers")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetUserByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(CreateUserCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, UpdateUserCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.Fail("ID mismatch"));

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteUserCommand(id));
        return HandleResult(result);
    }

    [HttpPut("{id}/lock")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<object>>> LockUser(int id)
    {
        var result = await Mediator.Send(new LockUserCommand(id));
        return HandleResult(result);
    }

    [HttpPut("{id}/unlock")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<object>>> UnlockUser(int id)
    {
        var result = await Mediator.Send(new UnlockUserCommand(id));
        return HandleResult(result);
    }

    [HttpPost("{id}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<object>>> AdminResetPassword(int id, [FromBody] string newPassword)
    {
        var result = await Mediator.Send(new AdminResetPasswordCommand { Id = id, NewPassword = newPassword });
        return HandleResult(result);
    }

    [HttpPost("bulk-upload")]
    [ProducesResponseType(typeof(ApiResponse<BulkUploadResult>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<BulkUploadResult>>> BulkUpload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("Please upload a file"));

        using var stream = file.OpenReadStream();
        var command = new BulkUploadUsersCommand
        {
            FileStream = stream,
            FileName = file.FileName
        };

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("bulk-upload/template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public IActionResult DownloadTemplate()
    {
        var excelService = HttpContext.RequestServices.GetRequiredService<IExcelTemplateService>();
        var template = excelService.Generate<UserBulkUploadDto>("User");
        return File(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Users_Template.xlsx");
    }
}