public enum CurrencyType
{
    Basic = 0,
    CryonCrystals = 1,
}

public interface ICurrency
{
    CurrencyType Name { get; }
    double Amount { get; }
    void Add(double amount);
    bool Spend(double amount);
}

public class SimpleCurrency : ICurrency
{
    public CurrencyType Name { get; private set; }

    public double Amount { get; private set; }

    public SimpleCurrency(CurrencyType name, double startingAmount)
    {
        Name = name;
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
