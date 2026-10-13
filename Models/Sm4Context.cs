namespace D8_Demo.Models;

public class Sm4Context
{
    public int mode;
    public long[] sk;
    public bool isPadding;
    public Sm4Context()
    {
        this.mode = 1;
        this.isPadding = true;
        this.sk = new long[32];
    }
}