using System;
using System.IO;
using System.Text.Json;
using D8_Demo.Models;

namespace D8_Demo.Tool;

public class SettingsManage
{
    private static readonly string FilePath =
        Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    public static AppSettings Settings { get; private set; } = new();

    static SettingsManage() => Load();

    private static void Load()
    {
        if (File.Exists(FilePath))
        {
            var json = File.ReadAllText(FilePath);
            Settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
    }

    public static void Save()
    {
        var json = JsonSerializer.Serialize(Settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }
}