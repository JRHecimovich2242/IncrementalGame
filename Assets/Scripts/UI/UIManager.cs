using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject generatorUIPrefab;
    [SerializeField] private Transform generatorUIParent;
    [SerializeField] private UpgradePanel upgradePanel;
    [SerializeField] private GameObject clickerGenerator = null;

    private List<GeneratorController> _generatorControllers = new();

    private void Awake()
    {
        _generatorControllers.Clear();

        for (int i = generatorUIParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(generatorUIParent.transform.GetChild(i).gameObject);
        }
    }

    public void SpawnGeneratorUI(GeneratorEntity model)
    {
        var go = Instantiate(generatorUIPrefab, generatorUIParent);
        var view = go.GetComponent<GeneratorView>();
        var controller = go.GetComponent<GeneratorController>();

        controller.Initialize(model, view);
        _generatorControllers.Add(controller);
    }

    public void SetupClickerUI(GeneratorEntity model)
    {
        if(clickerGenerator != null)
        {
            var view = clickerGenerator.GetComponent<GeneratorView>();
            var controller = clickerGenerator.GetComponent<GeneratorController>();
            controller.Initialize(model, view);
            _generatorControllers.Add(controller);
        }
    }

    public void SpawnUpgradeUI(UpgradeData data)
    {
        UpgradePurchaseView view = upgradePanel.SpawnUpgradeViewObject();
        UpgradePurchaseController controller = view.GetComponent<UpgradePurchaseController>();
        view.InitializeUpgradeView(data);
        controller.Initialize(view, data);
    }

    public void OnUpgradePurchased(UpgradeData data)
    {
        upgradePanel.RemoveUpgradeViewMatchingData(data);
    }

    private void Update()
    {
        foreach (var controller in _generatorControllers)
        {
            controller.UIUpdate();
        }
    }
}
