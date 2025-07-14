
using UnityEngine;
using UnityEditor;
using System.IO;

public class UpgradeGeneratorTool : EditorWindow
{
    private GeneratorData targetGenerator;
    private string upgradeFolder = "Assets/Upgrades/";
    private int rateUpgradeCount = 3;
    private int outputUpgradeCount = 3;
    private bool includeAutomationUpgrade = true;
    private double baseCost = 100;
    private double costMultiplier = 3;
    private float rateMultiplier = 1.5f;
    private float outputMultiplier = 2.0f;
    private Sprite rateIcon = null;
    private string rateName = string.Empty;
    private string rateDescription = string.Empty;
    private Sprite outputIcon = null;
    private string outputName = string.Empty;
    private string outputDescription = string.Empty;
    private Sprite automationIcon = null;
    private string automationName = string.Empty;
    private string automationDescription = string.Empty;

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

        baseCost = EditorGUILayout.DoubleField("Base Cost", baseCost);
        costMultiplier = EditorGUILayout.DoubleField("Cost Multiplier", costMultiplier);

        EditorGUILayout.Space(6);
        rateUpgradeCount = EditorGUILayout.IntField("Rate Upgrades", rateUpgradeCount);
        rateMultiplier = EditorGUILayout.FloatField("Rate Multiplier Per Upgrade", rateMultiplier);
        rateName = EditorGUILayout.TextField("Rate Upgrade Name", rateName);
        rateDescription = EditorGUILayout.TextField("Rate Upgrade Description", rateDescription);
        rateIcon = (Sprite)EditorGUILayout.ObjectField("Rate Upgrade Icon", rateIcon, typeof(Sprite), false);

        outputUpgradeCount = EditorGUILayout.IntField("Output Upgrades", outputUpgradeCount);
        outputMultiplier = EditorGUILayout.FloatField("Output Multiplier Per Upgrade", outputMultiplier);
        outputName = EditorGUILayout.TextField("Output Upgrade Name", outputName);
        outputDescription = EditorGUILayout.TextField("Output Upgrade Description", outputDescription);
        outputIcon = (Sprite)EditorGUILayout.ObjectField("Output Upgrade Icon", outputIcon, typeof(Sprite), false);

        includeAutomationUpgrade = EditorGUILayout.Toggle("Include Automation Upgrade", includeAutomationUpgrade);
        automationName = EditorGUILayout.TextField("Automation Upgrade Name", automationName);
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
        double currentCost = baseCost;
        int prerequisitesRequired = prerequisiteAmount;

        for (int i = 0; i < rateUpgradeCount; i++)
        {
            CreateUpgrade($"{rateName} {i + 1}", rateDescription,
                UpgradeType.RateUpgrade, targetGenerator, currentCost, rateMultiplier, upgradeNumber++, rateIcon, prerequisiteGenerator, prerequisitesRequired);
            currentCost *= costMultiplier;
            prerequisitesRequired = prerequisiteAmount * prerequisiteScalar * i;
        }

        upgradeNumber = 0;
        currentCost = baseCost;
        prerequisitesRequired = prerequisiteAmount;
        for (int i = 0; i < outputUpgradeCount; i++)
        {
            CreateUpgrade($"{outputName} {i + 1}", outputDescription,
                UpgradeType.AmountUpgrade, targetGenerator, currentCost, outputMultiplier, upgradeNumber++, outputIcon, prerequisiteGenerator, prerequisitesRequired);
            currentCost *= costMultiplier;
            prerequisitesRequired = prerequisiteAmount * prerequisiteScalar * i;
        }

        upgradeNumber = 0;
        currentCost = baseCost;
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
        if (preset == null) return;
        rateUpgradeCount = preset.rateUpgradeCount;
        outputUpgradeCount = preset.outputUpgradeCount;
        includeAutomationUpgrade = preset.includeAutomationUpgrade;
        baseCost = preset.baseCost;
        costMultiplier = preset.costMultiplier;
        rateMultiplier = preset.rateMultiplier;
        outputMultiplier = preset.outputMultiplier;
        selectedCurrency = preset.currencyType;
        rateName = preset.rateName;
        rateDescription = preset.rateDescription;
        rateIcon = preset.rateIcon;
        outputName = preset.outputName;
        outputDescription = preset.outputDescription;
        outputIcon = preset.outputIcon;
        automationName = preset.automationName;
        automationDescription = preset.automationDescription;
        automationIcon = preset.automationIcon;

    }

    private void CreateUpgrade(string name, string description, UpgradeType type, GeneratorData generator, double cost, float value, int index, Sprite icon, GeneratorData prereq = null, int prereqCount = 0)
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
