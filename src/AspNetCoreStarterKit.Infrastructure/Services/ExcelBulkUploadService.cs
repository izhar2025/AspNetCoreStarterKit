using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System.Reflection;
using AspNetCoreStarterKit.Application.Common.Attributes;
using AspNetCoreStarterKit.Application.Common.Models;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.Infrastructure.Services;

public class ExcelBulkUploadService : IBulkUploadService
{
    private readonly ILogger<ExcelBulkUploadService> _logger;

    public ExcelBulkUploadService(ILogger<ExcelBulkUploadService> logger)
    {
        _logger = logger;
        ExcelPackage.License.SetNonCommercialOrganization("YourOrganization");
    }

    public Task<List<TDto>> ParseAsync<TDto>(Stream stream, BulkUploadResult result, CancellationToken cancellationToken = default)
        where TDto : class, new()
    {
        var dtos = new List<TDto>();

        using var package = new ExcelPackage(stream);
        var sheet = package.Workbook.Worksheets.FirstOrDefault()
            ?? throw new InvalidOperationException("No worksheets found.");

        int headerRow = 1;
        int dataStartRow = 2;

        var headerMap = BuildHeaderMap(sheet, headerRow);
        var properties = GetMappedProperties<TDto>();
        var rowNumberProp = typeof(TDto).GetProperty("RowNumber");
        bool hasRowNumber = rowNumberProp != null && rowNumberProp.CanWrite;

        int lastRow = sheet.Dimension?.End.Row ?? 0;
        result.Total = Math.Max(0, lastRow - dataStartRow + 1);

        for (int row = dataStartRow; row <= lastRow; row++)
        {
            if (IsRowEmpty(sheet, row, sheet.Dimension?.End.Column ?? 1))
            {
                result.Total--;
                continue;
            }

            var dto = new TDto();
            bool rowHasError = false;

            if (hasRowNumber)
                rowNumberProp!.SetValue(dto, row);

            foreach (var (prop, attr) in properties)
            {
                if (!headerMap.TryGetValue(attr.Header.Trim().ToLowerInvariant(), out int colIndex))
                    continue;

                var cellValue = sheet.Cells[row, colIndex].Text?.Trim();

                if (attr.IsRequired && string.IsNullOrWhiteSpace(cellValue))
                {
                    result.AddError(row, attr.Header, cellValue, $"{attr.Header} is required.");
                    rowHasError = true;
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(cellValue))
                {
                    try
                    {
                        SetPropertyValue(prop, dto, cellValue);
                    }
                    catch (Exception ex)
                    {
                        result.AddError(row, attr.Header, cellValue, $"Invalid value: {ex.Message}");
                        rowHasError = true;
                    }
                }
            }

            if (!rowHasError)
                dtos.Add(dto);
        }

        result.FinalizeResult();
        return Task.FromResult(dtos);
    }

    private static Dictionary<string, int> BuildHeaderMap(ExcelWorksheet sheet, int headerRow)
    {
        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        int lastCol = sheet.Dimension?.End.Column ?? 0;

        for (int col = 1; col <= lastCol; col++)
        {
            var header = sheet.Cells[headerRow, col].Text?.Trim();
            if (!string.IsNullOrWhiteSpace(header))
                map[header.ToLowerInvariant()] = col;
        }

        return map;
    }

    private static List<(PropertyInfo Prop, ExcelColumnAttribute Attr)> GetMappedProperties<TDto>()
    {
        return typeof(TDto)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => (Prop: p, Attr: p.GetCustomAttribute<ExcelColumnAttribute>()))
            .Where(x => x.Attr != null)
            .OrderBy(x => x.Attr!.Order)
            .Select(x => (x.Prop, x.Attr!))
            .ToList();
    }

    private static bool IsRowEmpty(ExcelWorksheet sheet, int row, int lastCol)
    {
        for (int col = 1; col <= lastCol; col++)
        {
            if (!string.IsNullOrWhiteSpace(sheet.Cells[row, col].Text))
                return false;
        }
        return true;
    }

    private static void SetPropertyValue(PropertyInfo prop, object target, string value)
    {
        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

        object converted = targetType switch
        {
            _ when targetType == typeof(string) => value,
            _ when targetType == typeof(int) => int.Parse(value),
            _ when targetType == typeof(decimal) => decimal.Parse(value),
            _ when targetType == typeof(double) => double.Parse(value),
            _ when targetType == typeof(bool) => ParseBool(value),
            _ when targetType == typeof(DateTime) => DateTime.Parse(value),
            _ when targetType == typeof(DateOnly) => DateOnly.Parse(value),
            _ when targetType == typeof(TimeSpan) => TimeSpan.Parse(value),
            _ when targetType.IsEnum => Enum.Parse(targetType, value, ignoreCase: true),
            _ => Convert.ChangeType(value, targetType)
        };

        prop.SetValue(target, converted);
    }

    private static bool ParseBool(string value) =>
        value.ToUpperInvariant() switch
        {
            "TRUE" or "YES" or "1" or "Y" => true,
            "FALSE" or "NO" or "0" or "N" => false,
            _ => throw new FormatException($"Cannot convert '{value}' to boolean.")
        };
}