public enum CurrencyType
{
    Basic = 0,
    CryonCrystals = 1,
}

public interface ICurrency
{
    CurrencyType Type { get; }
    HugeInt Amount { get; }
    void Add(HugeInt amount);
    bool Spend(HugeInt amount);
}

public class SimpleCurrency : ICurrency
{
    public CurrencyType Type { get; private set; }

    public HugeInt Amount { get; private set; }

    public SimpleCurrency(CurrencyType type, HugeInt startingAmount)
    {
        Type = type;
        Amount = startingAmount;
    }

    public virtual void Add(HugeInt amount)
    {
        Amount += amount;
    }

    public virtual bool Spend(HugeInt amount)
    {
        if(amount > Amount)
        {
            return false;
        }

        Amount -= amount;
        return true;
    }
}
