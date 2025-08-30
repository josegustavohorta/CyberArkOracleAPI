public interface ICyberArkService
{
    Task<DatabaseCredentials> GetOracleCredentialsAsync();
}

public class CyberArkService : ICyberArkService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CyberArkService> _logger;

    public CyberArkService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<CyberArkService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<DatabaseCredentials> GetOracleCredentialsAsync()
    {
        try
        {
            var appId = _configuration["CyberArk:ApplicationId"];
            var safe = _configuration["CyberArk:Safe"];
            var object_ = _configuration["CyberArk:Object"];
            var ccpUrl = _configuration["CyberArk:CcpUrl"];

            var url = $"{ccpUrl}/AIMWebService/api/Accounts?" +
                     $"AppID={appId}&Safe={safe}&Object={object_}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(json);

            return new DatabaseCredentials
            {
                Username = result.UserName,
                Password = result.Content,
                ConnectionString = BuildConnectionString(result)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar credenciais no CyberArk");
            throw new Exception("Falha ao obter credenciais seguras");
        }
    }

    private string BuildConnectionString(dynamic cyberArkResponse)
    {
        return $"Data Source=(DESCRIPTION=" +
               $"(ADDRESS=(PROTOCOL=TCP)(HOST={_configuration["Oracle:Host"]})" +
               $"(PORT={_configuration["Oracle:Port"]}))" +
               $"(CONNECT_DATA=(SERVICE_NAME={_configuration["Oracle:ServiceName"]})));" +
               $"User Id={cyberArkResponse.UserName};Password={cyberArkResponse.Content};";
    }
}