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
    public bool IsActive { get { return NumOwned > HugeInt.Zero; } }
    public string Id { get { return m_data.GeneratorId; } }
    public HugeInt NumOwned {  get; private set; }
    public HugeInt CurrentAmount 
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
    private HugeInt _currentAmount = HugeInt.Zero;

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
        NumOwned = HugeInt.Zero;
        _currentAmount = new HugeInt((long)data.BaseGeneratedCurrency);
		RecalcNextCost();
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
                    GenerateCurrency(HugeInt.One);
                    secondsElapsedOffline -= TimeToGenerate;
                }
                else
                {
                    double timeRemainingInOfflineGeneration = TimeToGenerate / (1 - CurrentGenerationProgress);
                    // Check if the generation that was running on shutdown would have completed
                    if (secondsElapsedOffline > timeRemainingInOfflineGeneration)
                    {
                        secondsElapsedOffline -= timeRemainingInOfflineGeneration;
                        GenerateCurrency(HugeInt.One);
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
                    int GenerationsToPerform = Mathf.FloorToInt((float)secondsElapsedOffline / (float)TimeToGenerate);
                    GenerateCurrency(GenerationsToPerform);

                    double currentElapsed = secondsElapsedOffline - (TimeToGenerate * GenerationsToPerform);

                    if(currentElapsed > 0)
                    {
                        CurrentGenerationProgress = currentElapsed / TimeToGenerate;
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

    public void SetNumOwned(HugeInt newNumOwned)
    {
        NumOwned = newNumOwned;
        MarkDirty(GeneratorDirtyFlags.OwnedCount);
        MarkDirty(GeneratorDirtyFlags.AmountGenerated);

        RecalcNextCost();
    }

    public void Purchase(int numToPurchase)
    {
        HugeInt numToPurchase_Huge;

        if (numToPurchase == PurchaseQuantityController.PURCHASE_MAX_POSSIBLE)
        {
            numToPurchase_Huge = GetMaxAffordableUnits(CurrencyManager.Instance.GetCurrency(m_data.CostType));
        }
        else
        {
            numToPurchase_Huge = numToPurchase;
        }

        HugeInt costOfPurchase = GetCostToPurchase(numToPurchase_Huge);

        if (CurrencyManager.Instance.TrySpendCurrency(m_data.CostType, costOfPurchase))
        {
            MarkDirty(GeneratorDirtyFlags.OwnedCount);
            MarkDirty(GeneratorDirtyFlags.AmountGenerated);

            NumOwned += numToPurchase_Huge;

            GameState.Instance.OnPurchaseGenerator(m_data, numToPurchase_Huge);
        }

        RecalcNextCost();
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
                GenerateCurrency(HugeInt.One);
            }
        }
    }

	private HugeInt _nextCost; // price of the very next unit
	private void RecalcNextCost()
	{
		// first purchase OR no growth
		if (NumOwned == HugeInt.Zero || m_data.CostGrowthNumerator <= HugeInt.Zero || m_data.CostGrowthDenominator <= HugeInt.Zero)
		{
			_nextCost = (HugeInt)m_data.BaseCost;
			return;
		}

		// nextCost = baseCost * (num^owned) / (den^owned)
		var ownedBI = (System.Numerics.BigInteger)NumOwned; // assuming cast exists

		HugeInt numPow = HugeInt.Pow(m_data.CostGrowthNumerator, ownedBI);
		HugeInt denPow = HugeInt.Pow(m_data.CostGrowthDenominator, ownedBI);

		_nextCost = ((HugeInt)m_data.BaseCost * numPow) / denPow;
	}

	private HugeInt GetCostForNext(HugeInt n)
	{
		if (m_data.CostGrowthNumerator <= HugeInt.Zero || m_data.CostGrowthDenominator <= HugeInt.Zero)
		{
			return m_data.BaseCost_HugeInt;
		}

		if (n <= HugeInt.Zero) 
        {
            return HugeInt.Zero; 
        }

		HugeInt total = HugeInt.Zero;
		HugeInt price = _nextCost; // cost of the very next unit

		HugeInt i = HugeInt.Zero;
		while (i < n)
		{
			total += price;
			price = HugeIntMath.MulDivFloor(price, m_data.CostGrowthNumerator, m_data.CostGrowthDenominator);
			i += HugeInt.One;
		}

		return total;
	}
	private HugeInt GetMaxAffordableUnits(HugeInt funds)
	{
		if (funds <= HugeInt.Zero || m_data.CostGrowthNumerator <= HugeInt.Zero || m_data.CostGrowthDenominator <= HugeInt.Zero)
        {
			return HugeInt.Zero;
		}

		// Linear shortcut if growth = 1
		if (m_data.CostGrowthNumerator == m_data.CostGrowthDenominator)
		{
			HugeInt unitPrice = _nextCost; // all future prices equal in linear case
			return funds / unitPrice;
		}

		// Can't afford one?
		if (GetCostForNext(HugeInt.One) > funds)
			return HugeInt.Zero;

		// Exponential search
		HugeInt low = HugeInt.One;
		HugeInt high = new HugeInt(2);
		while (GetCostForNext(high) <= funds)
		{
			low = high;
			high = high * (HugeInt)2;
		}

		// Binary search between low (affordable) and high (not)
		while (low + HugeInt.One < high)
		{
			HugeInt mid = (low + high) / 2;

			if (GetCostForNext(mid) <= funds)
            {
				low = mid;
			}

			else high = mid;
		}

		return low;
	}

	public HugeInt GetCostToPurchase(HugeInt numToPurchase)
    {

        if (numToPurchase == PurchaseQuantityController.PURCHASE_MAX_POSSIBLE)
        {
            numToPurchase = GetMaxAffordableUnits(CurrencyManager.Instance.GetCurrency(m_data.CostType));
        }

        if (numToPurchase == 0)
        {
            numToPurchase = 1;
        }

        return GetCostForNext(numToPurchase);
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
            GenerateCurrency(HugeInt.One);
            CurrentGenerationProgress = 0;
        }
    }

    private void GenerateCurrency(HugeInt NumGenerations)
    {
        if (!_automated)
        {
            _running = false;
        }

        // Marking dirty here mostly to notify view
        MarkDirty(GeneratorDirtyFlags.CurrencyGenerated);

		CurrencyManager.Instance.ModifyCurrency(m_data.GeneratedCurrencyType, CurrentAmount * NumGenerations);

		ClearDirtyFlag(GeneratorDirtyFlags.CurrencyGenerated);
    }

    private void CalculateAmountGenerated()
    {
        // Recalculate amount to generate
        // Base yield = BaseGeneratedCurrency × Owned
       
        HugeInt baseGeneratedBD = (HugeInt)m_data.BaseGeneratedCurrency;
        HugeInt amount = baseGeneratedBD * NumOwned;

        // Apply all amount multipliers
        foreach (var up in _appliedUpgrades)
        {
            if (up.Type == UpgradeType.AmountUpgrade)
            {
                amount *= (HugeInt)up.UpgradeValue;
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
        return IsActive || CurrencyManager.Instance.GetCurrency(m_data.CostType) >= (HugeInt)m_data.CurrencyRequiredToShowView;
    }

    public bool ShouldObscure()
    {
        return !IsActive && CurrencyManager.Instance.GetCurrency(m_data.CostType) < (HugeInt)m_data.CurrencyRequiredToUnobscureView;
    }
}
