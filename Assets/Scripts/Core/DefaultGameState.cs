using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefaultGameState", menuName = "Idle/Default Game State")]
public class DefaultGameState : ScriptableObject
{
    [Serializable]
    public struct GeneratorDefault
    {
        public GeneratorData generator;
        public int ownedCount;
    }

    [Serializable]
    public struct CurrencyDefault
    {
        public CurrencyType currencyType;
        public double amount;
    }

    [Serializable]
    public struct UpgradeDefault
    {
        public UpgradeData upgrade;
    }

    [Header("Which generators you start with")]
    public List<GeneratorDefault> defaultGenerators = new();

    [Header("Which currencies you start with")]
    public List<CurrencyDefault> defaultCurrencies = new();

    [Header("Which upgrades you start with")]
    public List<UpgradeDefault> defaultUpgrades = new();
}
