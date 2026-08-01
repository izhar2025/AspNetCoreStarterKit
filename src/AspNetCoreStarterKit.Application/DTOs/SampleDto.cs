using AspNetCoreStarterKit.Application.Common.Attributes;

namespace AspNetCoreStarterKit.Application.DTOs;

public class SampleDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreateSampleDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateSampleDto : CreateSampleDto
{
    public int Id { get; set; }
}

public class SampleBulkUploadDto
{
    public int RowNumber { get; set; }

    [ExcelColumn("Name", order: 0, IsRequired = true, Example = "Sample Name")]
    public string Name { get; set; } = string.Empty;

    [ExcelColumn("Description", order: 1, IsRequired = false, Example = "Sample description")]
    public string? Description { get; set; }
}