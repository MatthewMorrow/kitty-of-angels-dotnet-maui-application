// Version 1.0
namespace KittyOfAngels.Services
{
    public interface IAppSettings
    {
        string? GetSetting(string key);
        void SaveSetting(string key, string value);
    }
}