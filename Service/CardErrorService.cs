using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tomlyn;

namespace D8_Demo.Service;

public class CardErrorService
{
    private readonly Dictionary<string, string> _errors;

    public CardErrorService()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Config/error.toml");

        if (!Path.Exists(path))
            throw new FileNotFoundException($"未找到错误码配置文件：{path}");

        var toml = File.ReadAllText(path, Encoding.UTF8);

        var data = TomlSerializer.Deserialize<
            Dictionary<string, Dictionary<string, string>>>(toml);

        _errors = new Dictionary<string, string>(
            StringComparer.OrdinalIgnoreCase);

        if (data == null)
            throw new InvalidOperationException("TOML 文件解析失败");

        foreach (var item in data)
        {
            if (item.Value.TryGetValue("desc", out var desc))
            {
                _errors[item.Key] = desc;
            }
        }
    }

    public string GetMessage(string errorCode)
    {
        return _errors.TryGetValue(errorCode, out var message)
            ? message
            : $"未知错误：{errorCode}";
    }
}