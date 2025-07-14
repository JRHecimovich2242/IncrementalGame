using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanel : MonoBehaviour
{
    [SerializeField] private GameObject upgradeViewPrefab = null;
    [SerializeField] private GameObject upgradeParent = null;
    private List<UpgradePurchaseView> _views = new();
    // Shows the collection of all upgrades
    // Get upgrade data from GeneratorController and create UpgradeViews for each

    private void Awake()
    {
        // Clear any placeholder child objects
        for(int i = upgradeParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(upgradeParent.transform.GetChild(i).gameObject);
        }
    }

    public UpgradePurchaseView SpawnUpgradeViewObject()
    {
        GameObject go = Instantiate(upgradeViewPrefab, upgradeParent.transform);
        UpgradePurchaseView view = go.GetComponent<UpgradePurchaseView>();
        _views.Add(view);
        return view;
    }

    public void RefreshUpgradeVisibility()
    {
        foreach (var view in _views)
        {
            view.RefreshVisibiltiy();
        }
    }

    public void RefreshUpgradeAffordability(CurrencyType typeToCheck, double currencyHeld)
    {
        foreach (var view in _views)
        {
            view.CheckAffordability(typeToCheck, currencyHeld);
        }
    }

    public bool RemoveUpgradeViewMatchingData(UpgradeData upgradeData)
    {
        // Find and remove upgrade view
        // Return whether or not it was found and removed
        UpgradePurchaseView view = _views.Find(x => x.Data == upgradeData);
        if (view != null)
        {
            _views.Remove(view);
            Destroy(view.gameObject);
            return true;
        }
        else
        {
            return false;
        }
    }
}
