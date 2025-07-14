using System;
using System.Collections.Generic;
using Unity.VisualScripting;

[System.Flags]
public enum GeneratorDirtyFlags
{
    None = 0,
    OwnedCount = 1 << 0,
    Progress = 1 << 1,
    CurrencyGenerated = 1 << 2,
    RateDirty = 1 << 3,
    AmountGenerated = 1 << 4,
    AutomationDirty = 1 << 5,
    All = ~0,
}

public class GeneratorEntity : IGenerator
{
    public event Action<GeneratorDirtyFlags> OnDirty;
    public bool IsActive { get { return NumOwned > 0; } }
    public string Id { get { return m_data.GeneratorId; } }
    public uint NumOwned {  get; private set; }
    public double CurrentAmount 
    {
        get 
        {
            if (_dirtyFlags.HasFlag(GeneratorDirtyFlags.OwnedCount) || _dirtyFlags.HasFlag(GeneratorDirtyFlags.AmountGenerated))
            {
                CalculateAmountGenerated();
            }

            return _currentAmount; 
        } 
    }
    private double _currentAmount = 0;

    public double TimeToGenerate => GetTimeToGenerate();
    private double _timeToGenerate = 0;
    public double CurrentGenerationProgress { get; private set; }
    private bool _running = false;

    private readonly List<UpgradeData> _appliedUpgrades = new();
    private readonly GeneratorData m_data;
    public GeneratorData Data => m_data;
    private GeneratorDirtyFlags _dirtyFlags;
    private bool _automated = false;

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

    public void ApplyUpgrade(UpgradeData upgrade)
    {
        GameState.Instance.OnPurchaseUpgrade(upgrade);

        _appliedUpgrades.Add(upgrade);

        switch (upgrade.Type)
        {
            case UpgradeType.AmountUpgrade:
                {
                    MarkDirty(GeneratorDirtyFlags.AmountGenerated);
                    break;
                }
            case UpgradeType.RateUpgrade:
                {
                    MarkDirty(GeneratorDirtyFlags.RateDirty);
                    break;
                }
            case UpgradeType.Automation:
                {
                    _automated = true;
                    MarkDirty(GeneratorDirtyFlags.AutomationDirty);

                    if (!_running)
                    {
                        Run();
                    }

                    break;
                }
        }


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
            MarkDirty(GeneratorDirtyFlags.AmountGenerated);

            NumOwned += (uint)numToPurchase;

            GameState.Instance.OnPurchaseGenerator(m_data);
        }
    }
    
    public void Run()
    {
        if(IsActive)
        {
            _running = true;
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
        if (!IsActive || !_running) 
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
            CurrentGenerationProgress = 0;
        }
    }

    private void GenerateCurrency()
    {
        if (!_automated)
        {
            _running = false;
        }

        CurrencyManager.Instance.ModifyCurrency(m_data.GeneratedCurrencyType, CurrentAmount);
    }

    private void CalculateAmountGenerated()
    {
        // Recalculate amount to generate
        _currentAmount = Data.BaseGeneratedCurrency * NumOwned;

        foreach (var upgrade in _appliedUpgrades)
        {
            // If Upgrade modifies rate
            if (upgrade.Type == UpgradeType.AmountUpgrade)
            {
                _currentAmount *= upgrade.UpgradeValue;
            }
        }

        ClearDirtyFlag(GeneratorDirtyFlags.OwnedCount);
        ClearDirtyFlag(GeneratorDirtyFlags.AmountGenerated);
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
                if(upgrade.Type == UpgradeType.RateUpgrade)
                {
                    newRate *= upgrade.UpgradeValue;
                }
            }

            _timeToGenerate = newRate;
            ClearDirtyFlag(GeneratorDirtyFlags.RateDirty);
        }

        return _timeToGenerate;
    }
}
