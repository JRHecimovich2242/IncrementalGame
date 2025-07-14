using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradePurchaseView : MonoBehaviour
{
    [SerializeField] private TMP_Text upgradeNameText = null;
    [SerializeField] private TMP_Text upgradeCostText = null;
    [SerializeField] private TMP_Text upgradeDescriptionText = null;
    [SerializeField] private Image upgradeIconImage = null;
    [SerializeField] private Selectable purchaseUpgradeSelectable = null;

    private UpgradeData _upgradeData = null;
    public UpgradeData Data => _upgradeData;
    private bool CurrentlyAffordable { get { return purchaseUpgradeSelectable.interactable; } }
    public void InitializeUpgradeView(UpgradeData upgradeData)
    {
        _upgradeData = upgradeData;

        if(upgradeNameText != null)
        {
            upgradeNameText.text = upgradeData.UpgradeName;
        }

        if(upgradeCostText != null)
        {
            upgradeCostText.text = _upgradeData.Cost.ToString();
            // TODO: Icon based on currency needed
        }

        if(upgradeDescriptionText != null)
        {
            upgradeDescriptionText.text = _upgradeData.UpgradeDescription;
        }

        // Default to unaffordable state
        // TODO: Ensure this happens before OnEnable - if not, rethink this logic. Is it worth avoiding a "can afford" bool here?
        if(upgradeIconImage != null)
        {
            upgradeIconImage.sprite = _upgradeData.UpgradeVisuals.UpgradeIcon;
        }
        
        // Only worry about selectable sprite state if there are overrides in the upgrade data, otherwise keep defaults
        if(purchaseUpgradeSelectable != null && _upgradeData.UpgradeVisuals.SelectableSprite != null) 
        {
            purchaseUpgradeSelectable.spriteState = _upgradeData.UpgradeVisuals.AffordableState;
            purchaseUpgradeSelectable.interactable = false;
        }
        CheckAffordability(_upgradeData.CostType, CurrencyManager.Instance.GetCurrency(_upgradeData.CostType));
        RefreshVisibiltiy();
    }

    public void RefreshVisibiltiy()
    {
        bool canDisplay = _upgradeData.CanDisplay();

        if (canDisplay != gameObject.activeSelf)
        {
            gameObject.SetActive(canDisplay);
        }
    }

    private void OnEnable()
    {
        if(_upgradeData != null)
        {
            CheckAffordability(_upgradeData.CostType, CurrencyManager.Instance.GetCurrency(_upgradeData.CostType));
        }
    }

    public void CheckAffordability(CurrencyType typeToCheck, double currValue)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        if(_upgradeData.CostType == typeToCheck)
        {
            SetAffordability(currValue >= _upgradeData.Cost);
        }
    }

    private void SetAffordability(bool canAfford)
    {
        if(canAfford != CurrentlyAffordable)
        {
            purchaseUpgradeSelectable.interactable = canAfford; 
            upgradeIconImage.color = canAfford ? _upgradeData.UpgradeVisuals.AffordableColor : _upgradeData.UpgradeVisuals.UnaffordableColor;
        }
    }
}
