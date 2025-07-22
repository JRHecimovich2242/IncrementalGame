public interface IGenerator
{
    bool IsActive { get; }
    string Id { get; }
    uint NumOwned { get; }
    double CurrentAmount { get; }
    double TimeToGenerate { get; }
    double CurrentGenerationProgress { get; }
    float ActivationTime { get; }
    void Tick(double deltaTime);
    void Purchase(int numToPurchase);
    void Sell(int numToSell);
    void ApplyUpgrade(UpgradeData upgrade, bool shouldNotify);
    bool CanShow();
    bool ShouldObscure();
}
