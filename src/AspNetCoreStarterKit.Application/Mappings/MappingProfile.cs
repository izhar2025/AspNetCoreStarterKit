using AutoMapper;
using AspNetCoreStarterKit.Application.DTOs;
using AspNetCoreStarterKit.Application.DTOs.Security;
using AspNetCoreStarterKit.Application.Features.Roles;
using AspNetCoreStarterKit.Application.Features.Sample;
using AspNetCoreStarterKit.Application.Features.Users;
using AspNetCoreStarterKit.Domain.Entities;
using AspNetCoreStarterKit.Domain.Entities.Security;

namespace AspNetCoreStarterKit.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateSampleCommand, SampleEntity>();
        CreateMap<UpdateSampleCommand, SampleEntity>();
        CreateMap<SampleEntity, SampleDto>();

        // AspNetCoreStarterKit.Application/Mappings/MappingProfile.cs - Add these mappings
        CreateMap<CreateUserCommand, User>();
        CreateMap<UpdateUserCommand, User>();
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : null));

        CreateMap<CreateRoleCommand, Role>();
        CreateMap<UpdateRoleCommand, Role>();
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.UsersCount, opt => opt.Ignore())
            .ForMember(dest => dest.Permissions, opt => opt.Ignore());

        CreateMap<Permission, PermissionDto>();

    }
}