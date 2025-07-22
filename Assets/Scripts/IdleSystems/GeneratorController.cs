using System;
using System.Reflection;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    private GeneratorEntity _model;
    private GeneratorView _view;
    private GeneratorDirtyFlags pendingFlags;
    private int purchaseAmount = 1;

    public void Initialize(GeneratorEntity model, GeneratorView view)
    {
        _model = model;
        _model.OnDirty += OnModelDirty;

        _view = view;
        _view.SetIconSprite(model.Data.Icon);
        _view.SetName(model.Id);

        _view.SetObscured(_model.ShouldObscure());
        _view.SetViewActive(_model.CanShow());

        // TODO: Additional visuals

        pendingFlags = GeneratorDirtyFlags.All;
        PurchaseQuantityController.Instance.OnPurchaseQuantityChanged += OnPurchaseQuantityChanged;
        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
    }

    private void OnDestroy()
    {
        if(PurchaseQuantityController.Instance != null)
        {
            PurchaseQuantityController.Instance.OnPurchaseQuantityChanged -= OnPurchaseQuantityChanged;
        }

        if(CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        }
    }

    public void OnPurchaseQuantityChanged(int quantity)
    {
        purchaseAmount = quantity;

        RefreshCostView();
    }

    public void Tick(double deltaTime)
    {
        _model.Tick(deltaTime);
    }

    public void UIUpdate()
    {
        if (pendingFlags == GeneratorDirtyFlags.None)
        {
            return;
        }

        if (pendingFlags.HasFlag(GeneratorDirtyFlags.AmountGenerated))
        {
            _view.UpdateRateText(_model.CurrentAmount);
        }

        if (pendingFlags.HasFlag(GeneratorDirtyFlags.OwnedCount))
        {
            _view.SetNumOwned(_model.NumOwned);

            if (_view.Obscured)
            {
                _view.SetObscured(_model.ShouldObscure());
            }

            RefreshCostView();
        }

        if (pendingFlags.HasFlag(GeneratorDirtyFlags.Progress))
        {
            _view.SetProgress((float)_model.CurrentGenerationProgress);
        }

        if (pendingFlags.HasFlag(GeneratorDirtyFlags.CurrencyGenerated))
        {
            _view.DisplayGenerationCompleteVisual();
        }

        pendingFlags = GeneratorDirtyFlags.None;
    }

    private void OnCurrencyChanged(CurrencyType type, double value)
    {
        if(type == _model.Data.CostType)
        {
            RefreshCostView();

            if(!_view.gameObject.activeSelf && _model.CanShow())
            {
                _view.SetViewActive(true);
            }

            if(_view.Obscured && !_model.ShouldObscure())
            {
                _view.SetObscured(false);
            }
        }
    }

    private void RefreshCostView()
    {
        double currentCost = _model.GetCostToPurchase(purchaseAmount);
        bool canAfford = CurrencyManager.Instance.GetCurrency(_model.Data.CostType) >= currentCost;
        _view.SetCostText(currentCost, canAfford);
    }

    private void OnModelDirty(GeneratorDirtyFlags flags)
    {
        pendingFlags |= flags;
    }

    public void OnClickPurchase()
    {
        _model.Purchase(purchaseAmount);
    }

    public void OnClickActivateGenerator()
    {
        _model.Run();
    }
}
