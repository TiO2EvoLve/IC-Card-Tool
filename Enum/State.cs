namespace D8_Demo.Enum;

//用于区分读取状态
public enum State
{
    等待放卡,
    等待读取,
    同一张卡,
    复位异常,
    读卡中,
    非CPU卡
}
//用于判断卡片种类
public enum CardCheckType { Unknown, Cpu, M1, Ul }