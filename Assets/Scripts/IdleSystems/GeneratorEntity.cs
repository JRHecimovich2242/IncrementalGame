using System;
using System.Collections.Generic;

[System.Flags]
public enum GeneratorDirtyFlags
{
    None = 0,
    OwnedCount = 1 << 0,
    Progress = 1 << 1,
    CurrencyGenerated = 1 << 2,
    RateDirty = 1 << 3,
    AmountGeneratedDirty = 1 << 3,
    All = ~0,
}

public class GeneratorEntity : IGenerator
{
    public event Action<GeneratorDirtyFlags> OnDirty;
    public bool IsActive { get { return NumOwned > 0; } }
    public string Id { get { return m_data.GeneratorId; } }
    public uint NumOwned {  get; private set; }
    public double CurrentAmount { get; private set; }

    public double TimeToGenerate => GetTimeToGenerate();
    private double _timeToGenerate = 0;
    public double CurrentGenerationProgress { get; private set; }

    private readonly List<IUpgrade> _appliedUpgrades = new();
    private readonly GeneratorData m_data;
    public GeneratorData Data => m_data;
    private GeneratorDirtyFlags _dirtyFlags;

    public GeneratorEntity(GeneratorData data)
    {
        m_data = data;      
        NumOwned = 0;
        MarkDirty(GeneratorDirtyFlags.All);
    }

    private void MarkDirty(GeneratorDirtyFlags dirtyFlags)
    {
        _dirtyFlags |= dirtyFlags;
        OnDirty?.Invoke(dirtyFlags);
    }

    public void ClearDirtyFlag(GeneratorDirtyFlags dirtyFlags)
    {
        _dirtyFlags &= ~dirtyFlags;
    }

    public void ApplyUpgrade(IUpgrade upgrade)
    {
        throw new System.NotImplementedException();

        //switch (upgrade.UpgradeType)
        //{
        //    case UpgradeType.AmountUpgrade:
        //        {
        //            MarkDirty(GeneratorDirtyFlags.AmountGeneratedDirty);
        //            break;
        //        }
        //    case UpgradeType.RateUpgrade:
        //        {
        //            MarkDirty(GeneratorDirtyFlags.RateDirty);
        //            break;
        //        }
        //}
    }

    public void Purchase(uint numToPurchase)
    {
        double costOfPurchase = m_data.BaseCost + (m_data.CostGrowth * NumOwned);
        // Start at 1 bc the above calculates the cost of buying the 0th new generator
        for (int i = 1; i < numToPurchase; i++)
        {
            costOfPurchase += m_data.CostGrowth;
        }

        if (CurrencyManager.Instance.TrySpendCurrency(m_data.CostType, costOfPurchase))
        {
            MarkDirty(GeneratorDirtyFlags.OwnedCount);

            NumOwned += numToPurchase;
        }
    }

    public void Sell(uint numToSell)
    {
        throw new System.NotImplementedException();
    }

    public void Tick(double deltaTime)
    {
        if (!IsActive) 
        { 
            return; 
        }

        if(TimeToGenerate <= 0)
        {
            // Generate every tick
            CurrentGenerationProgress = 1;
        }
        else
        {
            CurrentGenerationProgress += deltaTime / TimeToGenerate;
        }
       
        if (CurrentGenerationProgress >= 1f)
        {
            // Generate currency
            CurrencyManager.Instance.ModifyCurrency(m_data.GeneratedCurrencyType, CurrentAmount);

            // Allow overflow into next tick
            double overflow = CurrentGenerationProgress - 1;
            if (overflow > 0)
            {
                CurrentGenerationProgress = overflow;
            }
            else
            {
                CurrentGenerationProgress = 0;
            }
        }
    }

    private double GetTimeToGenerate()
    {
        if (_dirtyFlags.HasFlag(GeneratorDirtyFlags.RateDirty))
        {
            double baseRate = m_data.BaseRate;
            foreach (var upgrade in _appliedUpgrades)
            {
                // If Upgrade modifies rate

                // Apply upgrades to rate
            }

            ClearDirtyFlag(GeneratorDirtyFlags.RateDirty);
        }

        return _timeToGenerate;
    }
}
