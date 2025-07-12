using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private GameObject generatorUIPrefab;
    [SerializeField] private Transform generatorUIParent;

    private List<GeneratorController> controllers = new();
    public void SpawnGeneratorUI(GeneratorEntity model)
    {
        var go = Instantiate(generatorUIPrefab, generatorUIParent);
        var view = go.GetComponent<GeneratorView>();
        var controller = go.GetComponent<GeneratorController>();

        controller.Initialize(model, view);
        controllers.Add(controller);
    }

    private void Update()
    {
        foreach (var controller in controllers)
        {
            controller.UIUpdate();
        }
    }
}
