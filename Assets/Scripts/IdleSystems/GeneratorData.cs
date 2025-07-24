using UnityEngine;

[CreateAssetMenu(fileName ="New Generator", menuName ="Idle/Generator Data")]
public class GeneratorData : ScriptableObject
{
    public string GeneratorId;
    [TextArea]
    public string GeneratorDescription;
    public CurrencyType CostType;
    public double CurrencyRequiredToShowView;
    public double CurrencyRequiredToUnobscureView;
    public double BaseCost;
    public HugeInt BaseCost_HugeInt;
    public int CostGrowthNumerator;
    public int CostGrowthDenominator;
    public double CostGrowth;
    public CurrencyType GeneratedCurrencyType;
    public double BaseGeneratedCurrency;
    public double BaseRate;
    public Sprite Icon;

    public bool IsOwned { get { return GameState.Instance.OwnedGeneratorDict.ContainsKey(this); } }
}
