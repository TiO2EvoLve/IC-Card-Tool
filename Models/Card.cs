using System;

namespace D8_Demo.Models;

public class Card
{
    public string? SN { get; set; }
    public string? UID16 { get; set; }
    public string? UID16_ { get; set; }
    public string? UID10 { get; set; }
    public string? UID10_ { get; set; }
    public string? ATS { get; set; }
    public DateTime Time { get; set; }

    public Card(string sn, string uid16, string uid16_, string uid10, string uid10_, string ats, DateTime time)
    {
        SN = sn;
        UID16 = uid16;
        UID16_ = uid16_;
        UID10 = uid10;
        UID10_ = uid10_;
        ATS = ats;
        Time = time;
    }

    public Card()
    {
        
    }
}

