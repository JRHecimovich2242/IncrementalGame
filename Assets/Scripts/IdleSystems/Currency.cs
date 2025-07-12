public enum CurrencyType
{
    Basic = 0,
    CryonCrystals = 1,
}

public interface ICurrency
{
    CurrencyType Type { get; }
    double Amount { get; }
    void Add(double amount);
    bool Spend(double amount);
}

public class SimpleCurrency : ICurrency
{
    public CurrencyType Type { get; private set; }

    public double Amount { get; private set; }

    public SimpleCurrency(CurrencyType type, double startingAmount)
    {
        Type = type;
        Amount = startingAmount;
    }

    public virtual void Add(double amount)
    {
        Amount += amount;
    }

    public virtual bool Spend(double amount)
    {
        if(amount > Amount)
        {
            return false;
        }

        Amount -= amount;
        return true;
    }
}
