
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "NewUpgradePreset", menuName = "Idle/Upgrade Generator Preset")]
public class UpgradeGeneratorPreset : ScriptableObject
{
    public int RateUpgradeCount = 3;
    public int OutputUpgradeCount = 3;
    public bool IncludeAutomationUpgrade = true;

    public float RateMultiplier = 1.5f;
    public float OutputMultiplier = 2.0f;


    public double OutputBaseCost = 100;
    public double OutputCostMultiplier = 3;

    public double RateBaseCost = 100;
    public double RateCostMultiplier = 3;

    public double AutomationCost = 100;

    public CurrencyType currencyType = CurrencyType.Basic;

    public string RateName = "Speed Boost";
    public string RateDescription = "Increases generation speed.";
    public Sprite RateIcon;

    public string OutputName = "Output Boost";
    public string OutputDescription = "Increases output amount.";
    public Sprite OutputIcon;

    public string AutomationName = "Automation";
    public string AutomationDescription = "Automates this generator.";
    public Sprite AutomationIcon;
}
