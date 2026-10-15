using System;
using System.IO;
using System.Text.Json;
using D8_Demo.Models;

namespace D8_Demo.Tool;

public class SettingsManage
{
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    public AppSettings Settings { get; private set; }

    public SettingsManage()
    {
        Settings = Load();
    }

    private static AppSettings Load()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }

        return new AppSettings();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}