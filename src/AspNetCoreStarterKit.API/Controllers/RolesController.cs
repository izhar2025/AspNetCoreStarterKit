// AspNetCoreStarterKit.API/Controllers/RolesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspNetCoreStarterKit.API.Attributes;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Features.Roles;

namespace AspNetCoreStarterKit.API.Controllers;

[Authorize]
[Route("api/v1/roles")]
[RequirePermission("ManageRoles")]
public class RolesController : BaseApiController
{
    [HttpGet]
    [RequirePermission("ViewRoles")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RoleDto>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResult<RoleDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool? isSystemRole = null,
        [FromQuery] bool? includePermissions = true,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false)
    {
        var query = new GetAllRolesQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            IsSystemRole = isSystemRole,
            IncludePermissions = includePermissions,
            SearchTerm = search,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }

    [HttpGet("{id}")]
    [RequirePermission("ViewRoles")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(int id)
    {
        var result = await Mediator.Send(new GetRoleByIdQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create(CreateRoleCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(int id, UpdateRoleCommand command)
    {
        if (id != command.Id)
            return BadRequest(ApiResponse<object>.Fail("ID mismatch"));

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
    {
        var result = await Mediator.Send(new DeleteRoleCommand(id));
        return HandleResult(result);
    }

    [HttpPut("{id}/permissions")]
    [ProducesResponseType(typeof(ApiResponse<RoleDto>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> AssignPermissions(int id, AssignPermissionsToRoleCommand command)
    {
        if (id != command.RoleId)
            return BadRequest(ApiResponse<object>.Fail("Role ID mismatch"));

        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    [HttpGet("{id}/permissions")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<ActionResult<ApiResponse<object>>> GetRolePermissions(int id)
    {
        var role = await Mediator.Send(new GetRoleByIdQuery(id));

        if (!role.Success)
            return HandleResult(role);

        // Fixed: Use null-coalescing operator to provide default value
        return Ok(ApiResponse<object>.Ok(role.Data?.Permissions ?? new object(), "Permissions retrieved successfully"));
    }

    [HttpGet("permissions")]
    [RequirePermission("ViewPermissions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PermissionDto>>), 200)]
    public async Task<ActionResult<ApiResponse<PagedResult<PermissionDto>>>> GetAllPermissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? category = null,
        [FromQuery] string? module = null,
        [FromQuery] string? search = null)
    {
        var query = new GetAllPermissionsQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            Category = category,
            Module = module,
            SearchTerm = search
        };

        var result = await Mediator.Send(query);
        return HandlePagedResult(result);
    }
}