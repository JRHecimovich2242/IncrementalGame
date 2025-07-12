public enum UpgradeType
{
    RateUpgrade = 0,
    AmountUpgrade = 1,
}

public interface IUpgrade
{
    int Priority { get; }
    string Id { get; }
    bool IsUnlocked { get; }
    UpgradeType UpgradeType { get; }
    void ApplyTo(IGenerator generator);
}
