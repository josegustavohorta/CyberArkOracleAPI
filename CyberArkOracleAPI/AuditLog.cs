// AuditLog.cs
public class AuditLog
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Action { get; set; }
    public string QueryId { get; set; }
    public string Parameters { get; set; }
    public int? RowsReturned { get; set; }
    public long? ExecutionTime { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; }
}