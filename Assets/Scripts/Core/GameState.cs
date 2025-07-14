using System.Collections.Generic;
using System;
using UnityEngine;

public class GameState : Singleton<GameState>
{
    public Dictionary<GeneratorData, int> OwnedGeneratorDict = new Dictionary<GeneratorData, int>();
    public List<UpgradeData> PurchasedUpgrades { get; private set; } = new();

    public Action<GeneratorData> OnGeneratorPurchasedAction;

    public void OnPurchaseGenerator(GeneratorData data)
    {
        if (!OwnedGeneratorDict.ContainsKey(data))
        {
            OwnedGeneratorDict.Add(data, 0);
        }

        OwnedGeneratorDict[data]++;

        OnGeneratorPurchasedAction?.Invoke(data);
    }

    public void OnPurchaseUpgrade(UpgradeData data)
    {
        PurchasedUpgrades.Add(data);
    }

    private void Awake()
    {
        LoadGameState();
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        SaveGameState();
    }

    public void LoadGameState()
    {
        // Check for saved game state

        // If saved state found
        // Load from saved data AND account for time passed since closing game

        // Otherwise, initialize default state
    }

    public void SaveGameState()
    {
        // Save purchased generators (ID and amount)

        // Save purchased upgrades (ID)

        // Save currencies

        // Record time of exit
    }
}
