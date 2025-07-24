
using UnityEngine;
using UnityEditor;
using System.IO;

public class UpgradeGeneratorTool : EditorWindow
{
    private int[] SetGeneratorPrereqValues = { 1, 5, 25, 50 };

    private GeneratorData targetGenerator;
    private string upgradeFolder = "Assets/Resources/Upgrades/";
    private int rateUpgradeCount = 3;
    private int outputUpgradeCount = 3;
    private bool includeAutomationUpgrade = true;
    private float rateMultiplier = 1.5f;
    private float outputMultiplier = 2.0f;
    private Sprite rateIcon = null;
    private string rateName = string.Empty;
    private string rateDescription = string.Empty;
    private double rateBaseCost = 100;
    private double rateCostMultiplier = 3;
    private Sprite outputIcon = null;
    private string outputName = string.Empty;
    private string outputDescription = string.Empty;
    private double outputUpgradeBaseCost = 100;
    private double outputUpgradeCostMultiplier = 3;
    private Sprite automationIcon = null;
    private string automationName = string.Empty;
    private string automationDescription = string.Empty;
    private double automationUpgradeCost = 100;

    private GeneratorData prerequisiteGenerator;
    private int prerequisiteAmount;
    private int prerequisiteScalar;


    private UpgradeGeneratorPreset preset;
    private CurrencyType selectedCurrency = CurrencyType.Basic;


    [MenuItem("Tools/Upgrade Generator")]
    public static void ShowWindow()
    {
        GetWindow<UpgradeGeneratorTool>("Upgrade Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Upgrade Generator Tool", EditorStyles.boldLabel);

        targetGenerator = (GeneratorData)EditorGUILayout.ObjectField("Target Generator", targetGenerator, typeof(GeneratorData), false);
        upgradeFolder = EditorGUILayout.TextField("Save Folder", upgradeFolder);

        selectedCurrency = (CurrencyType)EditorGUILayout.EnumPopup("Currency Type", selectedCurrency);

        EditorGUILayout.Space();

        GUILayout.Label("Preset", EditorStyles.boldLabel);
        preset = (UpgradeGeneratorPreset)EditorGUILayout.ObjectField("Preset", preset, typeof(UpgradeGeneratorPreset), false);
        if (preset != null && GUILayout.Button("Apply Preset"))
        {
            ApplyPreset();
        }

        EditorGUILayout.Space(8);
        GUILayout.Label("Upgrade Settings", EditorStyles.boldLabel);


        EditorGUILayout.Space(6);
        rateUpgradeCount = EditorGUILayout.IntField("Rate Upgrades", rateUpgradeCount);
        rateMultiplier = EditorGUILayout.FloatField("Rate Multiplier Per Upgrade", rateMultiplier);
        rateName = EditorGUILayout.TextField("Rate Upgrade Name", rateName);
        rateBaseCost = EditorGUILayout.DoubleField("Base Cost", rateBaseCost);
        rateCostMultiplier = EditorGUILayout.DoubleField("Cost Multiplier", rateCostMultiplier);
        rateDescription = EditorGUILayout.TextField("Rate Upgrade Description", rateDescription);
        rateIcon = (Sprite)EditorGUILayout.ObjectField("Rate Upgrade Icon", rateIcon, typeof(Sprite), false);

        outputUpgradeCount = EditorGUILayout.IntField("Output Upgrades", outputUpgradeCount);
        outputMultiplier = EditorGUILayout.FloatField("Output Multiplier Per Upgrade", outputMultiplier);
        outputName = EditorGUILayout.TextField("Output Upgrade Name", outputName);
        outputDescription = EditorGUILayout.TextField("Output Upgrade Description", outputDescription);
        outputUpgradeBaseCost = EditorGUILayout.DoubleField("Base Cost", outputUpgradeBaseCost);
        outputUpgradeCostMultiplier = EditorGUILayout.DoubleField("Cost Multiplier", outputUpgradeCostMultiplier);
        outputIcon = (Sprite)EditorGUILayout.ObjectField("Output Upgrade Icon", outputIcon, typeof(Sprite), false);

        includeAutomationUpgrade = EditorGUILayout.Toggle("Include Automation Upgrade", includeAutomationUpgrade);
        automationName = EditorGUILayout.TextField("Automation Upgrade Name", automationName);
        automationUpgradeCost = EditorGUILayout.DoubleField("Base Cost", automationUpgradeCost);
        automationDescription = EditorGUILayout.TextField("Automation Upgrade Description", automationDescription);
        automationIcon = (Sprite)EditorGUILayout.ObjectField("Automation Upgrade Icon", automationIcon, typeof(Sprite), false);

        // Requirements
        prerequisiteGenerator = (GeneratorData)EditorGUILayout.ObjectField("Prerequisite Generator", prerequisiteGenerator, typeof(GeneratorData), false);
        prerequisiteAmount = EditorGUILayout.IntField("Prerequisite amount", prerequisiteAmount);
        prerequisiteScalar = EditorGUILayout.IntField("Prerequisite scalar", prerequisiteScalar);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Generate Upgrades"))
        {
            GenerateUpgrades();
        }
    }

    private void GenerateUpgrades()
    {
        if (targetGenerator == null)
        {
            Debug.LogError("No target generator assigned!");
            return;
        }

        if (!AssetDatabase.IsValidFolder(upgradeFolder))
        {
            Directory.CreateDirectory(upgradeFolder);
            AssetDatabase.Refresh();
        }

        int upgradeNumber = 0;
        HugeInt currentCost = (HugeInt)rateBaseCost;     
        double costMultiplier = rateCostMultiplier;
        int prerequisitesRequired = prerequisiteAmount;

        for (int i = 0; i < rateUpgradeCount; i++)
        {
            CreateUpgrade($"{rateName} {i + 1}", rateDescription,
                UpgradeType.RateUpgrade, targetGenerator, currentCost, rateMultiplier, upgradeNumber++, rateIcon, prerequisiteGenerator, i < SetGeneratorPrereqValues.Length ? SetGeneratorPrereqValues[i] : prerequisitesRequired);
            currentCost *= costMultiplier;
            prerequisitesRequired += prerequisiteScalar;
        }

        upgradeNumber = 0;
        currentCost = (HugeInt)outputUpgradeBaseCost;
        costMultiplier = outputUpgradeCostMultiplier;
        prerequisitesRequired = prerequisiteAmount;
        for (int i = 0; i < outputUpgradeCount; i++)
        {
            CreateUpgrade($"{outputName} {i + 1}", outputDescription,
                UpgradeType.AmountUpgrade, targetGenerator, currentCost, outputMultiplier, upgradeNumber++, outputIcon, prerequisiteGenerator, i < SetGeneratorPrereqValues.Length ? SetGeneratorPrereqValues[i] : prerequisitesRequired);
            currentCost *= costMultiplier;
            prerequisitesRequired += prerequisiteScalar;
        }

        upgradeNumber = 0;
        currentCost = (HugeInt)automationUpgradeCost;
        prerequisitesRequired = prerequisiteAmount;
        if (includeAutomationUpgrade)
        {
            CreateUpgrade(automationName, automationDescription,
                UpgradeType.Automation, targetGenerator, currentCost, 1.0f, upgradeNumber++, automationIcon, prerequisiteGenerator, prerequisitesRequired);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Upgrades generated successfully.");
    }

    private void ApplyPreset()
    {
        if (preset == null) 
        { 
            return; 
        } 

        rateUpgradeCount = preset.RateUpgradeCount;
        outputUpgradeCount = preset.OutputUpgradeCount;
        includeAutomationUpgrade = preset.IncludeAutomationUpgrade;
        rateMultiplier = preset.RateMultiplier;
        outputMultiplier = preset.OutputMultiplier;
        selectedCurrency = preset.currencyType;
        rateName = preset.RateName;
        rateDescription = preset.RateDescription;
        rateIcon = preset.RateIcon;
        rateBaseCost = preset.RateBaseCost;
        rateCostMultiplier = preset.RateCostMultiplier;
        outputName = preset.OutputName;
        outputDescription = preset.OutputDescription;
        outputUpgradeBaseCost = preset.OutputBaseCost;
        outputUpgradeCostMultiplier = preset.OutputCostMultiplier;
        outputIcon = preset.OutputIcon;
        automationName = preset.AutomationName;
        automationDescription = preset.AutomationDescription;
        automationIcon = preset.AutomationIcon;
        automationUpgradeCost = preset.AutomationCost;

    }

    private void CreateUpgrade(string name, string description, UpgradeType type, GeneratorData generator, HugeInt cost, float value, int index, Sprite icon, GeneratorData prereq = null, int prereqCount = 0)
    {
        string safeName = $"{generator.GeneratorId}_{type}_{index}";
        string assetPath = Path.Combine(upgradeFolder, safeName + ".asset");

        UpgradeData upgrade = AssetDatabase.LoadAssetAtPath<UpgradeData>(assetPath);
        if (upgrade == null)
        {
            upgrade = ScriptableObject.CreateInstance<UpgradeData>();
            AssetDatabase.CreateAsset(upgrade, assetPath);
        }

        upgrade.UpgradeName = name;
        upgrade.UpgradeDescription = description;
        upgrade.Type = type;
        upgrade.TargetGenerator = generator;
        upgrade.UpgradeValue = value;
        upgrade.CostType = selectedCurrency;
        upgrade.Cost = cost;
        upgrade.UpgradeVisuals.UpgradeIcon = icon;
        
        if(prerequisiteGenerator == null)
        {
            upgrade.DisplayCondition = new UpgradeDisplayCondition
            {
                conditionType = DisplayConditionType.None,
                requiredOwnedCount = 0,
                RequiredGenerator = null
            };

        }
        else
        {
            upgrade.DisplayCondition = new UpgradeDisplayCondition
            {
                conditionType = DisplayConditionType.GeneratorOwnedCount,
                requiredOwnedCount = prereqCount,
                RequiredGenerator = prereq
            };

        }

        EditorUtility.SetDirty(upgrade);
    }
}
