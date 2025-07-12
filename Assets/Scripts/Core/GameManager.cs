using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{

    [Header("Data")]
    [SerializeField] private List<GeneratorData> generatorDataList;

    [Header("References")]
    [SerializeField] private UIManager uiManager;

    private List<GeneratorEntity> generatorEntities = new();
    public bool GameActive { get; private set; } = false;

    private void Start()
    {
        BeginGame();
    }

    private void BeginGame()
    {
        InitializeGenerators();

        // TODO: Load saved data

        GameActive = true;
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        // TODO: Save data
    }

    private void InitializeGenerators()
    {
        foreach (var data in generatorDataList)
        {
            GeneratorEntity newEntity = new GeneratorEntity(data);
            generatorEntities.Add(newEntity);

            uiManager.SpawnGeneratorUI(newEntity);
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
}
