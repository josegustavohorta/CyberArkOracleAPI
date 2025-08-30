public class OracleService : IOracleService
{
    private readonly ICyberArkService _cyberArkService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OracleService> _logger;

    public async Task<QueryResult> ExecuteSecureQueryAsync(string queryId, Dictionary<string, object> parameters)
    {
        // Obter query predefinida
        var queryTemplate = PredefinedQueries.GetQuery(queryId);
        if (queryTemplate == null)
        {
            throw new ArgumentException($"Query {queryId} não encontrada");
        }

        // Obter credenciais
        var credentials = await _cyberArkService.GetOracleCredentialsAsync();

        using var connection = new OracleConnection(credentials.ConnectionString);
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = queryTemplate.Sql;
        command.CommandTimeout = 30;

        // Adicionar parâmetros de forma segura
        foreach (var param in parameters)
        {
            command.Parameters.Add(new OracleParameter($":{param.Key}", param.Value ?? DBNull.Value));
        }

        var stopwatch = Stopwatch.StartNew();
        var dataTable = new DataTable();

        using var adapter = new OracleDataAdapter(command);
        await Task.Run(() => adapter.Fill(dataTable));

        stopwatch.Stop();

        return new QueryResult
        {
            Data = ConvertDataTableToList(dataTable),
            RowCount = dataTable.Rows.Count,
            ExecutionTime = stopwatch.ElapsedMilliseconds
        };
    }
}