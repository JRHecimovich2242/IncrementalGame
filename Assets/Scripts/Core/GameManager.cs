using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    private List<GeneratorEntity> generatorEntities = new();
    private static readonly string ClickerGeneratorName = "Clicker";
    public bool GameActive { get; private set; } = false;

    private void Start()
    {
        BeginGame();
    }

    private void BeginGame()
    {
        var loadedData = GameState.Instance.LoadGameState();

        TutorialObserver tutorialObserver = FindFirstObjectByType<TutorialObserver>();
        if(tutorialObserver != null)
        {
            tutorialObserver.TryBeginTutorial(loadedData != null);
        }

        InitializeGenerators(loadedData);
        GameActive = true;
    }

    private void InitializeGenerators(SaveData loadFrom)
    {
        List<GeneratorData> generatorDataList = Resources.LoadAll<GeneratorData>("Generators/").ToList();
        generatorDataList.Sort((a,b) => a.BaseCost.CompareTo(b.BaseCost));

        foreach (var data in generatorDataList)
        {
            GeneratorEntity newEntity = new GeneratorEntity(data);
            generatorEntities.Add(newEntity);

            if(data.GeneratorId == "Clicker")
            {
                UIManager.Instance.SetupClickerUI(newEntity);
            }
            else
            {
                UIManager.Instance.SpawnGeneratorUI(newEntity);
            }

            if(GameState.Instance.OwnedGeneratorDict.TryGetValue(data, out HugeInt savedValue))
            {
                if(savedValue > HugeInt.Zero)
                {
                    newEntity.SetNumOwned(savedValue);
                }
            }
        }

        if(GameState.Instance.OwnedGeneratorDict.Count == 0)
        {
            // If first startup, init with one clicker generator
            generatorEntities.Find(x => x.Id == ClickerGeneratorName).SetNumOwned(HugeInt.One);
        }

        List<UpgradeData> upgradeDataList = Resources.LoadAll<UpgradeData>("Upgrades/").ToList();
        upgradeDataList.Sort((a, b) => a.Cost.CompareTo(b.Cost));

        foreach(var upgradeData in upgradeDataList)
        {
            if (GameState.Instance.PurchasedUpgrades.Contains(upgradeData))
            {
                GeneratorEntity targetEntity = GetGeneratorEntityMatchingData(upgradeData.TargetGenerator);

                if(targetEntity != null)
                {
                    targetEntity.ApplyUpgrade(upgradeData, shouldNotify: false);
                }
            }
            else
            {
                UIManager.Instance.SpawnUpgradeUI(upgradeData);
            }
        }

        // Apply loaded generator data / progress
        if (loadFrom != null)
        {
            foreach(GeneratorEntity entity in generatorEntities)
            {
                if (entity.IsActive)
                {
                    GeneratorSaveEntry saveEntry = loadFrom.generators.Find(x => x.GeneratorId == entity.Id);
                    if (saveEntry != null)
                    {
                        entity.LoadSavedInfo(saveEntry);
                    }
                }
            }
        }
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
