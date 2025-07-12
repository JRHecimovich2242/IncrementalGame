using System;
using System.Reflection;
using UnityEngine;

public class GeneratorController : MonoBehaviour
{
    private GeneratorEntity _model;
    private GeneratorView _view;
    private GeneratorDirtyFlags pendingFlags;

    public void Initialize(GeneratorEntity model, GeneratorView view)
    {
        _model = model;
        _model.OnDirty += OnModelDirty;

        _view = view;
        _view.SetIconSprite(model.Data.Icon);
        _view.SetName(model.Id);
        // TODO: Additional visuals

        pendingFlags = GeneratorDirtyFlags.All;
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

        if (pendingFlags.HasFlag(GeneratorDirtyFlags.OwnedCount))
        {
            _view.SetNumOwned(_model.NumOwned);
            _view.SetCostText(_model.GetCostOfNextPurchase());
            _view.UpdateRateText(_model.CurrentAmount, _model.TimeToGenerate);
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

    private void OnModelDirty(GeneratorDirtyFlags flags)
    {
        pendingFlags |= flags;
    }

    public void OnClickPurchase(int toPurchase)
    {
        _model.Purchase(toPurchase);
    }
}
