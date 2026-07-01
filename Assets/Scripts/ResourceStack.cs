using System;

[Serializable]
public struct ResourceStack
{
    public ResourceType type;
    public int amount;

    public ResourceStack(ResourceType type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
}
