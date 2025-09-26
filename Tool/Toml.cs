using System;
using System.IO;
using Tmds.DBus.Protocol;
using Tommy;

namespace D8_Demo.Tool;

public class Toml
{
    public static string GetToml(string root,string key)
    {
        var configPath = "Config/error.toml";
        string toml = "";
        try
        {
            TextReader tomlText = new StreamReader(configPath);
            var table = TOML.Parse(tomlText);
            toml =  table[root][key];
        }catch(Exception e)
        {
            return "";
        }
        return toml;
    }
}