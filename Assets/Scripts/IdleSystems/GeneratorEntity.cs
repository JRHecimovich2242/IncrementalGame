using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

                ClearDirtyFlag(GeneratorDirtyFlags.OwnedCount);
                ClearDirtyFlag(GeneratorDirtyFlags.AmountGenerated);
            }

            return _currentAmount; 
        } 
    }
    private double _currentAmount = 0;

    public double TimeToGenerate => GetTimeToGenerate();
    private double _timeToGenerate = 0;
    public double CurrentGenerationProgress { get; private set; }
    public float ActivationTime { get; private set; }
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

    public void LoadSavedInfo(GeneratorSaveEntry loadFrom)
    {
        if(loadFrom != null)
        {
            if(loadFrom.GenerationProgressOnQuit > 0 || _automated)
            {
                CurrentGenerationProgress = loadFrom.GenerationProgressOnQuit;

                double secondsElapsedOffline = GameState.Instance.OfflineElapsed.TotalSeconds;
                if(CurrentGenerationProgress >= 1)
                {
                    GenerateCurrency();
                    secondsElapsedOffline -= TimeToGenerate;
                }
                else
                {
                    double timeRemainingInOfflineGeneration = TimeToGenerate / (1 - CurrentGenerationProgress);
                    // Check if the generation that was running on shutdown would have completed
                    if (secondsElapsedOffline > timeRemainingInOfflineGeneration)
                    {
                        secondsElapsedOffline -= timeRemainingInOfflineGeneration;
                        GenerateCurrency();
                        CurrentGenerationProgress = 0;
                        MarkDirty(GeneratorDirtyFlags.Progress);
                    }
                    else
                    {
                        _running = true;
                    }
                }

                if(_automated)
                {
                    while (secondsElapsedOffline > TimeToGenerate)
                    {
                        // Calculate the number of completions that would have happened in time offline
                        secondsElapsedOffline -= TimeToGenerate;
                        GenerateCurrency();
                    }

                    if(secondsElapsedOffline > 0)
                    {
                        CurrentGenerationProgress = secondsElapsedOffline / TimeToGenerate;
                        MarkDirty(GeneratorDirtyFlags.Progress);
                    }

                }
            }
        }
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

    public void ApplyUpgrade(UpgradeData upgrade, bool shouldNotify = true)
    {
        if(shouldNotify)
        {
            GameState.Instance.OnPurchaseUpgrade(upgrade);
        }

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

    public void SetNumOwned(uint newNumOwned)
    {
        NumOwned = newNumOwned;
        MarkDirty(GeneratorDirtyFlags.OwnedCount);
        MarkDirty(GeneratorDirtyFlags.AmountGenerated);
    }

    public void Purchase(int numToPurchase)
    {
        if(numToPurchase == PurchaseQuantityController.PURCHASE_MAX_POSSIBLE)
        {
            numToPurchase = GetMaxAffordableUnits(CurrencyManager.Instance.GetCurrency(m_data.CostType));
        }

        double costOfPurchase = GetCostToPurchase(numToPurchase);

        if (CurrencyManager.Instance.TrySpendCurrency(m_data.CostType, costOfPurchase))
        {
            MarkDirty(GeneratorDirtyFlags.OwnedCount);
            MarkDirty(GeneratorDirtyFlags.AmountGenerated);

            NumOwned += (uint)numToPurchase;

            GameState.Instance.OnPurchaseGenerator(m_data, numToPurchase);
        }
    }
    
    public void Run()
    {
        if(IsActive)
        {
            if (TimeToGenerate > 0)
            {
                _running = true;
                ActivationTime = Time.time;
            }
            else
            {
                GenerateCurrency();
            }
        }
    }

    private double GetCostForNext(int n)
    {
        if (n <= 0)
        {
            return 0;
        }

        double b = m_data.BaseCost;
        double r = m_data.CostGrowth;
        double currOwned = NumOwned;

        if (Mathf.Approximately((float)r, 1f))
        {
            // Linear fallback: cost = baseCost * n
            return b * n;
        }

        double factor = Math.Pow(r, currOwned);
        // geometric sum formula
        return b * factor * (Math.Pow(r, n) - 1) / (r - 1);
    }

    private int GetMaxAffordableUnits(double funds)
    {
        double b = m_data.BaseCost;
        double r = m_data.CostGrowth;
        double currOwned = NumOwned;

        if (funds < GetCostForNext(1))
        {
            return 0;
        }

        if (Mathf.Approximately((float)r, 1f))
        {
            // Linear case: cost per unit = b * 1^L = b
            return (int)(funds / b);
        }

        double factor = Math.Pow(r, currOwned);
        double numerator = funds * (r - 1);
        double denom = b * factor;
        double inside = 1 + numerator / denom;

        // protect against rounding error
        int n = (int)Math.Floor(Math.Log(inside) / Math.Log(r));
        return Math.Max(0, n);
    }

    public double GetCostToPurchase(int numToPurchase)
    {
        if(numToPurchase == PurchaseQuantityController.PURCHASE_MAX_POSSIBLE)
        {
            numToPurchase = GetMaxAffordableUnits(CurrencyManager.Instance.GetCurrency(m_data.CostType));

            if(numToPurchase == 0)
            {
                numToPurchase = 1;
            }
        }

        return Math.Round(GetCostForNext(numToPurchase));
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

        // Marking dirty here mostly to notify view
        MarkDirty(GeneratorDirtyFlags.CurrencyGenerated);

        CurrencyManager.Instance.ModifyCurrency(m_data.GeneratedCurrencyType, CurrentAmount);

        ClearDirtyFlag(GeneratorDirtyFlags.CurrencyGenerated);
    }

    private void CalculateAmountGenerated()
    {
        // Recalculate amount to generate
        // Base yield = BaseGeneratedCurrency × Owned
        double amount = Data.BaseGeneratedCurrency * NumOwned;

        // Apply all amount multipliers
        foreach (var up in _appliedUpgrades)
        {
            if (up.Type == UpgradeType.AmountUpgrade)
            {
                amount *= up.UpgradeValue;
            }
        }

        _currentAmount = amount;
    }

    private double GetTimeToGenerate()
    {
        if (_dirtyFlags.HasFlag(GeneratorDirtyFlags.RateDirty))
        {
            // Combine all speed multipliers
            double speedMul = 1.0;
            foreach (var up in _appliedUpgrades)
            {
                if (up.Type == UpgradeType.RateUpgrade)
                {
                    speedMul *= up.UpgradeValue;
                }
            }

            // More speed → less time per cycle
            _timeToGenerate = Data.BaseRate / speedMul;
            ClearDirtyFlag(GeneratorDirtyFlags.RateDirty);
        }
        return _timeToGenerate;
    }

    public bool CanShow()
    {
        return IsActive || CurrencyManager.Instance.GetCurrency(m_data.CostType) >= m_data.CurrencyRequiredToShowView;
    }

    public bool ShouldObscure()
    {
        return !IsActive && CurrencyManager.Instance.GetCurrency(m_data.CostType) < m_data.CurrencyRequiredToUnobscureView;
    }
}
