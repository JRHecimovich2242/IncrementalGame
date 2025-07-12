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
    AmountGeneratedDirty = 1 << 4,
    All = ~0,
}

public class GeneratorEntity : IGenerator
{
    public event Action<GeneratorDirtyFlags> OnDirty;
    public bool IsActive { get { return NumOwned > 0; } }
    public string Id { get { return m_data.GeneratorId; } }
    public uint NumOwned {  get; private set; }
    public double CurrentAmount { get { return _currentAmount; } }
    private double _currentAmount = 0;

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
        _currentAmount = data.BaseGeneratedCurrency;
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

    public void Purchase(int numToPurchase)
    {
        double costOfPurchase = GetCostOfNextPurchase();
        // Start at 1 bc the above calculates the cost of buying the 0th new generator
        for (int i = 1; i < numToPurchase; i++)
        {
            costOfPurchase += m_data.CostGrowth;
        }

        if (CurrencyManager.Instance.TrySpendCurrency(m_data.CostType, costOfPurchase))
        {
            MarkDirty(GeneratorDirtyFlags.OwnedCount);

            NumOwned += (uint)numToPurchase;
        }
    }

    public double GetCostOfNextPurchase()
    {
        return m_data.BaseCost + (m_data.CostGrowth * NumOwned);
    }

    public void Sell(int numToSell)
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
            MarkDirty(GeneratorDirtyFlags.Progress);
        }
       
        if (CurrentGenerationProgress >= 1f)
        {
            GenerateCurrency();

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

    private void GenerateCurrency()
    {
        if(_dirtyFlags.HasFlag(GeneratorDirtyFlags.OwnedCount))
        {
            // Recalculate amount to generate
            _currentAmount = Data.BaseGeneratedCurrency * NumOwned;

            ClearDirtyFlag(GeneratorDirtyFlags.OwnedCount);
        }

        CurrencyManager.Instance.ModifyCurrency(m_data.GeneratedCurrencyType, CurrentAmount);
    }

    private double GetTimeToGenerate()
    {
        if (_dirtyFlags.HasFlag(GeneratorDirtyFlags.RateDirty))
        {
            // TODO: Apply upgrades
            double newRate = m_data.BaseRate;
            foreach (var upgrade in _appliedUpgrades)
            {
                // If Upgrade modifies rate

                // Apply upgrades to rate
            }

            _timeToGenerate = newRate;
            ClearDirtyFlag(GeneratorDirtyFlags.RateDirty);
        }

        return _timeToGenerate;
    }
}
