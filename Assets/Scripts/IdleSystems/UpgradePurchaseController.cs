using System;
using UnityEngine;

public class UpgradePurchaseController : MonoBehaviour
{
    private UpgradeData _data;
    private UpgradePurchaseView _upgradeView = null;

    private void Awake()
    {
        _upgradeView = GetComponent<UpgradePurchaseView>();
    }

    public void Initialize(UpgradePurchaseView view, UpgradeData data)
    {
        _data = data;
        _upgradeView = view;
        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        GameState.Instance.OnGeneratorPurchasedAction += OnPurchaseGenerator;
    }

    private void OnPurchaseGenerator(GeneratorData data)
    {
        if(data == _data.TargetGenerator)
        {
            _upgradeView.RefreshVisibiltiy();
        }
    }

    private void OnDestroy()
    {
        if(CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        }

        if(GameState.Instance != null)
        {
            GameState.Instance.OnGeneratorPurchasedAction -= OnPurchaseGenerator;
        }
    }

    private void OnCurrencyChanged(CurrencyType type, double newValue)
    {
        _upgradeView.CheckAffordability(type, newValue);
    }

    public void PurchaseUpgrade()
    {
        if(CurrencyManager.Instance.TrySpendCurrency(_data.CostType, _data.Cost))
        {
            GeneratorEntity target = GameManager.Instance.GetGeneratorEntityMatchingData(_data.TargetGenerator);
            if(target != null)
            {
                target.ApplyUpgrade(_data);
            }
            UIManager.Instance.OnUpgradePurchased(_data);
        }
    }
}
