
using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgradePreset", menuName = "Idle/Upgrade Generator Preset")]
public class UpgradeGeneratorPreset : ScriptableObject
{
    public int rateUpgradeCount = 3;
    public int outputUpgradeCount = 3;
    public bool includeAutomationUpgrade = true;

    public double baseCost = 100;
    public double costMultiplier = 3;

    public float rateMultiplier = 1.5f;
    public float outputMultiplier = 2.0f;

    public CurrencyType currencyType = CurrencyType.Basic;

    public string rateName = "Speed Boost";
    public string rateDescription = "Increases generation speed.";
    public Sprite rateIcon;

    public string outputName = "Output Boost";
    public string outputDescription = "Increases output amount.";
    public Sprite outputIcon;

    public string automationName = "Automation";
    public string automationDescription = "Automates this generator.";
    public Sprite automationIcon;
}
