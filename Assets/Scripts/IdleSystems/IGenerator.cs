public interface IGenerator
{
    bool IsActive { get; }
    string Id { get; }
    uint NumOwned { get; }
    double CurrentAmount { get; }
    double TimeToGenerate { get; }
    double CurrentGenerationProgress { get; }
    void Tick(double deltaTime);
    void Purchase(int numToPurchase);
    void Sell(int numToSell);
    void ApplyUpgrade(UpgradeData upgrade);
}
