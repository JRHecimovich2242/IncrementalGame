using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private List<GeneratorEntity> generatorEntities = new();
    public bool GameActive { get; private set; } = false;

    private void Start()
    {
        BeginGame();
    }

    private void BeginGame()
    {
        // TODO: Load saved data

        InitializeCurrencies();

        InitializeGenerators();

        GameActive = true;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        // TODO: Save data
    }

    private void InitializeGenerators()
    {
        List<GeneratorData> generatorDataList = Resources.LoadAll<GeneratorData>("Generators/").ToList();
        generatorDataList.Sort((a,b) => a.BaseCost.CompareTo(b.BaseCost));

        foreach (var data in generatorDataList)
        {
            GeneratorEntity newEntity = new GeneratorEntity(data);
            generatorEntities.Add(newEntity);

            UIManager.Instance.SpawnGeneratorUI(newEntity);
        }

        List<UpgradeData> upgradeDataList = Resources.LoadAll<UpgradeData>("Upgrades/").ToList();
        upgradeDataList.Sort((a, b) => a.Cost.CompareTo(b.Cost));

        foreach(var upgradeData in upgradeDataList)
        {
            UIManager.Instance.SpawnUpgradeUI(upgradeData);
        }
    }
    
    private void InitializeCurrencies()
    {
        // TODO: Move to GameState load behavior
        // TODO: Init with saved data

        // Basic Currency (Scrap)
        SimpleCurrency basicCurrency = new SimpleCurrency(CurrencyType.Basic, 0);
        CurrencyManager.Instance.AddNewCurrency(basicCurrency);
    }

    private void Update()
    {
        if (GameActive)
        {
            TickGenerators(Time.deltaTime);
        }
    }

    private void TickGenerators(float deltaTime)
    {
        foreach(var entity in generatorEntities)
        {
            entity.Tick(deltaTime);
        }
    }

    public GeneratorEntity GetGeneratorEntityMatchingData(GeneratorData data)
    {
        return generatorEntities.Find(x => x.Data == data);
    }
}
