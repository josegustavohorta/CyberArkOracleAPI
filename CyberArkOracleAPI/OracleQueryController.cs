using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class OracleQueryController : ControllerBase
{
    private readonly IOracleService _oracleService;
    private readonly IAuditService _auditService;
    private readonly PermissionService _permissionService;
    private readonly ILogger<OracleQueryController> _logger;

    public OracleQueryController(
        IOracleService oracleService,
        IAuditService auditService,
        PermissionService permissionService,
        ILogger<OracleQueryController> logger)
    {
        _oracleService = oracleService;
        _auditService = auditService;
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteQuery([FromBody] QueryRequest request)
    {
        var username = User.Identity?.Name;

        try
        {
            // Validar permissões
            if (!await _permissionService.CanUserExecuteQuery(username, request.QueryId))
            {
                await _auditService.LogUnauthorizedAccessAsync(username, request.QueryId);
                return Forbid("Sem permissão para executar esta consulta");
            }

            // Executar query
            var result = await _oracleService.ExecuteSecureQueryAsync(
                request.QueryId,
                request.Parameters
            );

            // Auditar
            await _auditService.LogQueryExecutionAsync(new AuditLog
            {
                Username = username,
                QueryId = request.QueryId,
                Parameters = System.Text.Json.JsonSerializer.Serialize(request.Parameters),
                RowsReturned = result.RowCount,
                ExecutionTime = result.ExecutionTime,
                Success = true,
                Timestamp = DateTime.UtcNow,
                IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            return Ok(new QueryResponse
            {
                Success = true,
                Data = result.Data,
                RowCount = result.RowCount,
                ExecutionTime = result.ExecutionTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar query {QueryId}", request.QueryId);

            await _auditService.LogQueryExecutionAsync(new AuditLog
            {
                Username = username,
                QueryId = request.QueryId,
                Success = false,
                ErrorMessage = ex.Message,
                Timestamp = DateTime.UtcNow
            });

            return StatusCode(500, new { message = "Erro ao executar consulta" });
        }
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableQueries()
    {
        var username = User.Identity?.Name;
        var queries = await _permissionService.GetUserQueriesAsync(username);

        return Ok(queries);
    }
}