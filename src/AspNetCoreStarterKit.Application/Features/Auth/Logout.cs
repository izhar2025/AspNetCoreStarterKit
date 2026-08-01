// AspNetCoreStarterKit.Application/Features/Auth/Logout.cs
using MediatR;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Domain.Entities.Security;
using AspNetCoreStarterKit.Domain.Interfaces;

namespace AspNetCoreStarterKit.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : IRequest<ApiResponse<object>>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<object>>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<object>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _unitOfWork.Repository<RefreshToken>()
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            refreshToken.RevokedReason = "User logged out";
            _unitOfWork.Repository<RefreshToken>().Update(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<object>.Ok(string.Empty, "Logged out successfully");
    }
}