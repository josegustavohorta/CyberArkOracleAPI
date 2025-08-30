using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authService,
        IAuditService auditService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validar contra AD
            var user = await _authService.ValidateUserAsync(request.Username, request.Password);

            if (user == null)
            {
                await _auditService.LogFailedLoginAsync(request.Username, HttpContext.Connection.RemoteIpAddress?.ToString());
                return Unauthorized(new { message = "Credenciais inválidas" });
            }

            // Gerar JWT
            var token = _authService.GenerateJwtToken(user);

            await _auditService.LogSuccessfulLoginAsync(user.Username, HttpContext.Connection.RemoteIpAddress?.ToString());

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresIn = 3600,
                UserName = user.Username,
                Permissions = user.Permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no login");
            return StatusCode(500, new { message = "Erro interno" });
        }
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        var username = User.Identity?.Name;
        var newToken = await _authService.RefreshTokenAsync(username);

        return Ok(new { token = newToken });
    }
}
