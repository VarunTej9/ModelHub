using ModelHub.Application.Contracts;
using ModelHub.Application.Interfaces;
using ModelHub.Domain.Entities;
using ModelHub.Domain.Exceptions;

namespace ModelHub.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(IUserRepository userRepository, IPasswordService passwordService, IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (existingUser is not null)
        {
            throw new BusinessRuleException("An account with this email already exists.");
        }

        var user = new AppUser(email);

        var passwordHash = _passwordService.HashPassword(user, request.Password);

        user.SetPasswordHash(passwordHash);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthResponse(user.Id, user.Email, user.Role, _jwtTokenService.CreateToken(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !user.IsActive || !_passwordService.VerifyPassword(user, user.PasswordHash, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        return new AuthResponse(user.Id, user.Email, user.Role, _jwtTokenService.CreateToken(user));
    }
}
