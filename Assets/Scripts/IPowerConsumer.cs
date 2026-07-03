public interface IPowerConsumer
{
    int PowerDemand { get; }
    bool IsPowered { get; }
    void SetPowered(bool powered);
}
