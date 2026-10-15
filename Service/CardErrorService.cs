using System;
using System.Collections.Generic;
using System.IO;
using Tomlyn;

namespace D8_Demo.Service;

public class CardErrorService
{
    private readonly Dictionary<string, string> _errors;

    public CardErrorService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Config/error.toml");

        var toml = File.ReadAllText(path);

        var data = TomlSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(toml);

        _errors = data != null && data.TryGetValue("errors", out var errors)
            ? errors
            : new Dictionary<string, string>();
    }

    public string GetMessage(string errorCode)
    {
        return _errors.TryGetValue(errorCode, out var message)
            ? message
            : $"未知错误：{errorCode}";
    }
}