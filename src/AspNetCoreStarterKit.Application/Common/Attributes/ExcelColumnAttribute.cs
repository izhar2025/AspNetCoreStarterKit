namespace AspNetCoreStarterKit.Application.Common.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class ExcelColumnAttribute : Attribute
{
    public string Header { get; set; }
    public int Order { get; set; }
    public bool IsRequired { get; set; }
    public string? Example { get; set; }
    public string? Note { get; set; }
    public string? AllowedValues { get; set; }

    public ExcelColumnAttribute(string header, int order = 0)
    {
        Header = header;
        Order = order;
    }
}