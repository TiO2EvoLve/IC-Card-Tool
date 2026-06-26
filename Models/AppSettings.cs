namespace D8_Demo.Models;

public class AppSettings
{
    public string Theme { get; set; } = "Light";
    public int SleepTime { get; set; } = 500;
    public bool Sound{ get; set; }
    
    public static AppSettings Instance { get; set; } = new();
    
    
    public void Reset()
    {
        Theme = "Default";
        SleepTime = 500;
        Sound = false;
    }
}