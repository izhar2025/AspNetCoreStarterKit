using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Reflection;
using AspNetCoreStarterKit.Application.Common.Attributes;
using AspNetCoreStarterKit.Application.Interfaces;

namespace AspNetCoreStarterKit.Infrastructure.Services;

public class ExcelTemplateService : IExcelTemplateService
{
    public ExcelTemplateService()
    {
        ExcelPackage.License.SetNonCommercialOrganization("ISC");
    }

    public byte[] Generate<TDto>(string entityName) where TDto : class, new()
    {
        var properties = GetMappedProperties<TDto>();

        using var package = new ExcelPackage();

        BuildTemplateSheet(package, entityName, properties);
        BuildInstructionsSheet(package, entityName, properties);
        BuildLookupsSheet(package, properties);

        return package.GetAsByteArray();
    }

    private static void BuildTemplateSheet(ExcelPackage package, string entityName, List<(PropertyInfo Prop, ExcelColumnAttribute Attr)> properties)
    {
        var ws = package.Workbook.Worksheets.Add("Template");

        ws.Cells[1, 1].Value = $"{entityName} Bulk Upload Template";
        ws.Cells[1, 1, 1, properties.Count].Merge = true;
        StyleTitleRow(ws.Cells[1, 1]);

        for (int i = 0; i < properties.Count; i++)
        {
            var (_, attr) = properties[i];
            var cell = ws.Cells[2, i + 1];
            cell.Value = attr.Header;
            StyleHeaderCell(cell, attr.IsRequired);

            if (!string.IsNullOrWhiteSpace(attr.Note))
                cell.AddComment(attr.Note, "System");
        }

        for (int i = 0; i < properties.Count; i++)
        {
            var (_, attr) = properties[i];
            ws.Cells[3, i + 1].Value = attr.Example ?? string.Empty;
            StyleSampleCell(ws.Cells[3, i + 1]);
        }

        for (int i = 0; i < properties.Count; i++)
            StyleSampleCell(ws.Cells[4, i + 1]);

        ws.View.FreezePanes(3, 1);
        ws.Cells[ws.Dimension.Address].AutoFitColumns(12, 40);

        ws.Cells[5, 1].Value = "↑ Replace sample rows above. Do NOT modify row 1 (title) or row 2 (headers).";
        ws.Cells[5, 1, 5, properties.Count].Merge = true;
        ws.Cells[5, 1].Style.Font.Italic = true;
        ws.Cells[5, 1].Style.Font.Color.SetColor(Color.Gray);
    }

    private static void BuildInstructionsSheet(ExcelPackage package, string entityName, List<(PropertyInfo Prop, ExcelColumnAttribute Attr)> properties)
    {
        var ws = package.Workbook.Worksheets.Add("Instructions");

        ws.Cells[1, 1].Value = $"{entityName} — Field Instructions";
        ws.Cells[1, 1, 1, 5].Merge = true;
        StyleTitleRow(ws.Cells[1, 1]);

        string[] cols = { "#", "Column Name", "Required", "Example", "Notes / Allowed Values" };
        for (int c = 0; c < cols.Length; c++)
        {
            ws.Cells[2, c + 1].Value = cols[c];
            StyleHeaderCell(ws.Cells[2, c + 1], false);
        }

        for (int i = 0; i < properties.Count; i++)
        {
            var (_, attr) = properties[i];
            int row = i + 3;

            ws.Cells[row, 1].Value = i + 1;
            ws.Cells[row, 2].Value = attr.Header;
            ws.Cells[row, 3].Value = attr.IsRequired ? "✅ Yes" : "No";
            ws.Cells[row, 4].Value = attr.Example ?? "-";
            ws.Cells[row, 5].Value = BuildNoteText(attr);

            if (i % 2 == 0)
            {
                ws.Cells[row, 1, row, 5].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells[row, 1, row, 5].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(245, 245, 245));
            }
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns(10, 60);
        ws.View.FreezePanes(3, 1);
    }

    private static void BuildLookupsSheet(ExcelPackage package, List<(PropertyInfo Prop, ExcelColumnAttribute Attr)> properties)
    {
        var enumProps = properties.Where(x => !string.IsNullOrWhiteSpace(x.Attr.AllowedValues)).ToList();
        if (!enumProps.Any()) return;

        var ws = package.Workbook.Worksheets.Add("Lookups");
        ws.Cells[1, 1].Value = "Valid values for restricted columns";
        ws.Cells[1, 1, 1, enumProps.Count].Merge = true;
        StyleTitleRow(ws.Cells[1, 1]);

        for (int i = 0; i < enumProps.Count; i++)
        {
            var (_, attr) = enumProps[i];
            int col = i + 1;

            ws.Cells[2, col].Value = attr.Header;
            StyleHeaderCell(ws.Cells[2, col], false);

            var values = attr.AllowedValues!.Split('|');
            for (int v = 0; v < values.Length; v++)
                ws.Cells[v + 3, col].Value = values[v].Trim();
        }

        ws.Cells[ws.Dimension.Address].AutoFitColumns(15, 30);
    }

    private static void StyleTitleRow(ExcelRange cell)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.Size = 13;
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(31, 73, 125));
        cell.Style.Font.Color.SetColor(Color.White);
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        cell.Style.Border.Bottom.Style = ExcelBorderStyle.Thick;
        cell.Style.Border.Bottom.Color.SetColor(Color.FromArgb(0, 112, 192));
    }

    private static void StyleHeaderCell(ExcelRange cell, bool isRequired)
    {
        cell.Style.Font.Bold = true;
        cell.Style.Font.Size = 10;

        var bgColor = isRequired ? Color.FromArgb(0, 112, 192) : Color.FromArgb(68, 114, 196);

        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(bgColor);
        cell.Style.Font.Color.SetColor(Color.White);
        cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        cell.Style.WrapText = true;

        if (isRequired && cell.Value is string v && !v.EndsWith(" *"))
            cell.Value = v + " *";

        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin, Color.White);
    }

    private static void StyleSampleCell(ExcelRange cell)
    {
        cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
        cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(235, 241, 251));
        cell.Style.Font.Italic = true;
        cell.Style.Font.Color.SetColor(Color.FromArgb(89, 89, 89));
        cell.Style.Border.BorderAround(ExcelBorderStyle.Hair, Color.FromArgb(180, 180, 180));
    }

    private static string BuildNoteText(ExcelColumnAttribute attr)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(attr.Note))
            parts.Add(attr.Note);
        if (!string.IsNullOrWhiteSpace(attr.AllowedValues))
            parts.Add($"Allowed: {attr.AllowedValues.Replace("|", ", ")}");
        return parts.Any() ? string.Join(" | ", parts) : "-";
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
}