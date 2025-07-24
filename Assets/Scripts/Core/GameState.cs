using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class GeneratorSaveEntry
{
    public string GeneratorId;
    public HugeInt Count;
    public long LastActivationTimeBinary;
    public double GenerationProgressOnQuit;
}

[Serializable]
public class UpgradeSaveEntry
{
    public string UpgradeId;
}

[Serializable]
public class CurrencySaveEntry
{
    public string CurrencyId;
    public HugeInt Amount;
}

[Serializable]
public class SaveData
{
    public List<GeneratorSaveEntry> generators = new();
    public List<UpgradeSaveEntry> upgrades = new();
    public List<CurrencySaveEntry> currencies = new();
    public List<CurrencySaveEntry> lifetimeCurrencies = new();
    public long lastSaveTimeBin;
}

public class GameState : Singleton<GameState>
{
    [Header("Default State (used if no save found)")]
    [SerializeField] private DefaultGameState defaultGameState;

    public Dictionary<GeneratorData, HugeInt> OwnedGeneratorDict = new();
    [HideInInspector] public List<UpgradeData> PurchasedUpgrades = new();
    public TimeSpan OfflineElapsed { get; private set; }

    public Action<GeneratorData> OnGeneratorPurchasedAction;
    public Action<UpgradeData> OnUpgradePurchasedAction;
    private Dictionary<CurrencyType, HugeInt> _currencySnapshot = new();
    private Dictionary<CurrencyType, HugeInt> _lifetimeCurrencySnapshot = new();
    private GameManager _gameManager = null;
    private DateTime _startupTime;
    // Path to our savefile:
    private string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, "savegame.json");

    public void OnPurchaseGenerator(GeneratorData data, long numPurchased)
    {
        OnPurchaseGenerator(data, (HugeInt)numPurchased);
    }

    public void OnPurchaseGenerator(GeneratorData data, HugeInt numPurchased)
    {
        if (!OwnedGeneratorDict.ContainsKey(data))
        {
            OwnedGeneratorDict.Add(data, HugeInt.Zero);
        }

        OwnedGeneratorDict[data] += numPurchased;

        OnGeneratorPurchasedAction?.Invoke(data);
    }

    public void OnPurchaseUpgrade(UpgradeData data)
    {
        if (PurchasedUpgrades.Contains(data))
        {
            Debug.LogWarning("Trying to purchase duplicate upgrade with Name " + data.UpgradeName);
            return;
        }

        PurchasedUpgrades.Add(data);

        OnUpgradePurchasedAction?.Invoke(data);
    }

    protected override void OnApplicationQuit()
    {
        SaveGameState();
        base.OnApplicationQuit();
    }

    public SaveData LoadGameState()
    {
        _startupTime = DateTime.Now;

        if (!File.Exists(SaveFilePath))
        {
            // First run: nothing to load
            OfflineElapsed = TimeSpan.Zero;
            LoadDefaultState();
            return null;
        }

        try
        {
            var json = File.ReadAllText(SaveFilePath);
            var data = JsonUtility.FromJson<SaveData>(json);

            // --- Rehydrate Generators ---
            OwnedGeneratorDict.Clear();
            // assume you placed all your GeneratorData in Resources/Generators
            var allGens = Resources.LoadAll<GeneratorData>("Generators");
            foreach (var entry in data.generators)
            {
                var gen = Array.Find(allGens, g => g.GeneratorId == entry.GeneratorId);
                if (gen != null)
                    OwnedGeneratorDict[gen] = entry.Count;
            }

            // --- Rehydrate Upgrades ---
            PurchasedUpgrades.Clear();
            var allUps = Resources.LoadAll<UpgradeData>("Upgrades");
            foreach (var entry in data.upgrades)
            {
                var up = Array.Find(allUps, u => u.UpgradeName == entry.UpgradeId);

                if (up != null)
                {
                    PurchasedUpgrades.Add(up);
                }
            }

            // --- Rehydrate Currencies ---
            var currencyDict = data.currencies.ToDictionary(c => c.CurrencyId, c => c.Amount);
            CurrencyManager.Instance.LoadAll(currencyDict);

            // --- LoadLifetimeStatistics ---
            var lifetimeCurrencies = data.lifetimeCurrencies.ToDictionary(c => c.CurrencyId, c => c.Amount);
            foreach (var entry in lifetimeCurrencies)
            {
                if(Enum.TryParse(entry.Key, out CurrencyType type))
                {
                    _lifetimeCurrencySnapshot.Add(type, entry.Value);
                }
            }

            // --- Compute Offline Time ---
            var last = DateTime.FromBinary(data.lastSaveTimeBin);
            OfflineElapsed = DateTime.UtcNow - last;
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load savegame: {e}");
            OfflineElapsed = TimeSpan.Zero;
            LoadDefaultState();
            return null;
        }
    }

    private void LoadDefaultState()
    {
        // --- Generators ---
        OwnedGeneratorDict.Clear();
        foreach (var def in defaultGameState.defaultGenerators)
        {
            if (def.generator != null)
                OwnedGeneratorDict[def.generator] = def.ownedCount;
        }

        // --- Currencies ---
        var currencyDict = new Dictionary<CurrencyType, HugeInt>();
        foreach (var def in defaultGameState.defaultCurrencies)
        {
            currencyDict[def.currencyType] = def.amount;
        }
        CurrencyManager.Instance.LoadAll(currencyDict);

        // --- Upgrades ---
        PurchasedUpgrades.Clear();
        foreach (var def in defaultGameState.defaultUpgrades)
        {
            if (def.upgrade != null)
                PurchasedUpgrades.Add(def.upgrade);
        }
    }

    private long GetLastActivationTimeDateTimeBinary(float time)
    {
        DateTime dateTimeOfLastActivation = _startupTime.AddSeconds((double)time);

        return dateTimeOfLastActivation.ToBinary();
    }

    public void SaveGameState()
    {
        var data = new SaveData();

        // --- Save Generators ---
        foreach (var kvp in OwnedGeneratorDict)
        {
            GeneratorEntity entity = _gameManager.GetGeneratorEntityMatchingData(kvp.Key);

            data.generators.Add(new GeneratorSaveEntry
            {
                GeneratorId = kvp.Key.GeneratorId,
                Count = kvp.Value,
                GenerationProgressOnQuit = entity != null ? entity.CurrentGenerationProgress : 0,
                LastActivationTimeBinary = entity != null ? GetLastActivationTimeDateTimeBinary(entity.ActivationTime) : 0,
            });
        }

        // --- Save Upgrades ---
        foreach (var up in PurchasedUpgrades)
        {
            data.upgrades.Add(new UpgradeSaveEntry
            {
                UpgradeId = up.UpgradeName
            });
        }

        // --- Save Currencies ---
        foreach (var kvp in _currencySnapshot)
        {
            data.currencies.Add(new CurrencySaveEntry
            {
                CurrencyId = kvp.Key.ToString(),
                Amount = kvp.Value
            });
        }

        // --- Timestamp ---
        data.lastSaveTimeBin = DateTime.UtcNow.ToBinary();

        try
        {
            var json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save savegame: {e}");
        }
    }

    private void Start()
    {
        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;

        _gameManager = GameManager.Instance;
    }

    private void OnDisable()
    {
        if(CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
        }
    }

    private void OnCurrencyChanged(CurrencyType type, HugeInt value)
    {
        if(!_currencySnapshot.ContainsKey(type))
        {
            _currencySnapshot.Add(type, HugeInt.Zero);
        }
        else if(value > _currencySnapshot[type])
        {
            // Currency gained, update lifetime snapshot
            if (!_lifetimeCurrencySnapshot.ContainsKey(type))
            {
                _lifetimeCurrencySnapshot.Add(type, HugeInt.Zero);
            }

            _currencySnapshot[type] = value - _currencySnapshot[type];
        }

        _currencySnapshot[type] = value;
    }
}
