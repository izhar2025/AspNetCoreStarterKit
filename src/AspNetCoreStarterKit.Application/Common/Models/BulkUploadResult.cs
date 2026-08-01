namespace AspNetCoreStarterKit.Application.Common.Models;

public class BulkUploadResult
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public int Skipped { get; set; }
    public List<BulkUploadRowError> Errors { get; set; } = new();
    public bool HasErrors => Errors.Any();

    public void AddError(int row, string column, string? value, string reason)
        => Errors.Add(BulkUploadRowError.Create(row, column, value, reason));

    public void FinalizeResult()  // ← Renamed from Finalize
    {
        Failed = Errors.Count;
        Success = Total - Failed - Skipped;
    }
}

public class BulkUploadRowError
{
    public int Row { get; set; }
    public string Column { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Reason { get; set; } = string.Empty;

    public static BulkUploadRowError Create(int row, string column, string? value, string reason)
        => new() { Row = row, Column = column, Value = value, Reason = reason };
}