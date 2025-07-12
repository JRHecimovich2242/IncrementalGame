using UnityEngine;

[CreateAssetMenu(fileName ="New Generator", menuName ="Idle/Generator Data")]
public class GeneratorData : ScriptableObject
{
    public string GeneratorId;
    public CurrencyType CostType;
    public double BaseCost;
    public double CostGrowth;
    public CurrencyType GeneratedCurrencyType;
    public double BaseGeneratedCurrency;
    public double BaseRate;
    public Sprite Icon;
}
