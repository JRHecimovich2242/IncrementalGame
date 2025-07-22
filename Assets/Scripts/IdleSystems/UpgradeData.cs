using UnityEngine;
using UnityEngine.UI;

public enum UpgradeType
{
    RateUpgrade = 0,
    AmountUpgrade = 1,
    Automation = 2,
}

public enum DisplayConditionType
{
    None,
    GeneratorOwnedCount
}

[System.Serializable]
public struct UpgradeDisplayCondition
{
    public DisplayConditionType conditionType;
    public GeneratorData RequiredGenerator;
    public int requiredOwnedCount;

    public bool IsMet()
    {
        switch (conditionType)
        {
            case DisplayConditionType.GeneratorOwnedCount:
                {
                    // Query current game state
                    return requiredOwnedCount <= 0 || (GameState.Instance.OwnedGeneratorDict.ContainsKey(RequiredGenerator) && GameState.Instance.OwnedGeneratorDict[RequiredGenerator] >= requiredOwnedCount);
                }
            case DisplayConditionType.None:
            default:
                {
                    return true;
                }
        }
    }
}

[System.Serializable]
public struct UpgradeVisuals
{
    public Sprite UpgradeIcon;
    public Sprite SelectableSprite;
    public Sprite HighlightedSprite;
    public Sprite SelectedSprite;
    public Sprite PressedSprite;
    public Color AffordableColor { get { return Color.white; } }
    public Color UnaffordableColor { get { return Color.gray; } }

    private SpriteState CreateSpriteState(Sprite highlighted, Sprite selected, Sprite pressed, Sprite disabled)
    {
        SpriteState toReturn = new()
        {
            selectedSprite = selected,
            highlightedSprite = highlighted,
            pressedSprite = pressed,
            disabledSprite = disabled
        };

        return toReturn;
    }

    public SpriteState AffordableState
    {
        get
        {
            return CreateSpriteState(HighlightedSprite, SelectedSprite, PressedSprite, null);
        }
    }
}

[CreateAssetMenu(fileName ="New Upgrade", menuName = "Idle/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string UpgradeName = string.Empty;
    public string UpgradeDescription = string.Empty;
    public CurrencyType CostType;
    public double Cost;
    public GeneratorData TargetGenerator = null;
    public UpgradeType Type = UpgradeType.RateUpgrade;
    public UpgradeDisplayCondition DisplayCondition;
    public double UpgradeValue = 0;
    public UpgradeVisuals UpgradeVisuals;

    public bool CanDisplay()
    {
        return DisplayCondition.IsMet() && TargetGenerator.IsOwned;
    }
}
