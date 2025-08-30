public static class PredefinedQueries
{
    private static readonly Dictionary<string, QueryDefinition> Queries = new()
    {
        ["WORKORDER_BY_CONTRACT"] = new QueryDefinition
        {
            Id = "WORKORDER_BY_CONTRACT",
            Name = "Ordens de Serviço por Contrato",
            Sql = @"
                SELECT w.WONUM, w.DESCRIPTION, w.STATUS, w.ACTSTART, w.ACTFINISH
                FROM MAXIMO.WORKORDER w
                WHERE w.TJ_CONTRACTNUM = :contractNum
                AND w.STATUS = 'FECHADA'
                ORDER BY w.WONUM",
            RequiredRole = "MAXIMO_READER"
        },

        ["CONTRACT_CONSUMPTION"] = new QueryDefinition
        {
            Id = "CONTRACT_CONSUMPTION",
            Name = "Consumo do Contrato",
            Sql = @"
                SELECT 
                    c.CONTRACTLINENUM,
                    c.ITEMNUM,
                    c.DESCRIPTION,
                    SUM(s.QUANTITY) as QTD_CONSUMIDA,
                    SUM(s.LINECOST) as VALOR_CONSUMIDO
                FROM MAXIMO.CONTRACTLINE c
                INNER JOIN MAXIMO.SERVRECTRANS s ON s.ITEMNUM = c.ITEMNUM
                WHERE c.CONTRACTNUM = :contractNum
                GROUP BY c.CONTRACTLINENUM, c.ITEMNUM, c.DESCRIPTION",
            RequiredRole = "MAXIMO_READER"
        }
    };

    public static QueryDefinition GetQuery(string queryId) =>
        Queries.GetValueOrDefault(queryId);
}
