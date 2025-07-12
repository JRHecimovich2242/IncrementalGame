public interface IGenerator
{
    bool IsActive { get; }
    string Id { get; }
    uint NumOwned { get; }
    double CurrentAmount { get; }
    double TimeToGenerate { get; }
    double CurrentGenerationProgress { get; }
    void Tick(double deltaTime);
    void Purchase(uint numToPurchase);
    void Sell(uint numToSell);
    void ApplyUpgrade(IUpgrade upgrade);
}
