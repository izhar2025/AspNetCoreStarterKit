namespace AspNetCoreStarterKit.Application.Interfaces;

public interface IExcelTemplateService
{
    byte[] Generate<TDto>(string entityName) where TDto : class, new();
}