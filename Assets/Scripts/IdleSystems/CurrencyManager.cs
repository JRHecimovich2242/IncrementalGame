using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager>
{
    public const int CURRENCY_SIG_FIGS = 3;
    public const int SCIENTIFIC_NOTATION_THRESHOLD = 1000000000;
    

    public static string ConvertHugeIntCurrencyToString(HugeInt value)
    {
        if (value > CurrencyManager.SCIENTIFIC_NOTATION_THRESHOLD)
        {
            return value.ToScientificString(CurrencyManager.CURRENCY_SIG_FIGS);
        }
        else
        {
            return value.ToString();
        }
    }

    private readonly Dictionary<CurrencyType, ICurrency> currencies = new();
    public event Action<CurrencyType, HugeInt> OnCurrencyChanged;

    public void AddNewCurrency(ICurrency newCurrency)
    {
        if(currencies.ContainsKey(newCurrency.Type))
        {
            Debug.LogErrorFormat("Tried to add a duplicate currency with name \"{0}\" to currency dict.", new object[] { newCurrency.Type.ToString() });
            return;
        }

        currencies.Add(newCurrency.Type, newCurrency);
        OnCurrencyChanged?.Invoke(newCurrency.Type, newCurrency.Amount);
    }

    public void ModifyCurrency(CurrencyType currencyType, HugeInt value)
    {
        if (!currencies.ContainsKey(currencyType))
        {
            Debug.LogErrorFormat("Tried to modify currency with name \"{0}\" which did not exist in currency dict.", new object[] { currencyType.ToString() });
            return;
        }

        if(value > 0)
        {
            currencies[currencyType].Add(value);
        }
        else
        {
            currencies[currencyType].Spend(value);
        }

        OnCurrencyChanged?.Invoke(currencyType, GetCurrency(currencyType));
    }

    public bool TrySpendCurrency(CurrencyType currencyType, HugeInt value)
    {
        if (!currencies.ContainsKey(currencyType))
        {
            return false;
        }

        bool spendSuccessful = currencies[currencyType].Spend(value);

        if (spendSuccessful)
        {
            OnCurrencyChanged?.Invoke(currencyType, GetCurrency(currencyType));
        }

        return spendSuccessful;
    }

    public HugeInt GetCurrency(CurrencyType currencyType)
    { 
        return currencies.TryGetValue(currencyType, out ICurrency currency) ? currency.Amount : 0; 
    }

    public Dictionary<CurrencyType, HugeInt> GetAll()
    {
        var copy = new Dictionary<CurrencyType, HugeInt>(currencies.Count);

        foreach (var kvp in currencies)
        {
            copy[kvp.Key] = kvp.Value.Amount;
        }

        return copy;
    }

    public void LoadAll(Dictionary<string, HugeInt> currencyDict)
    {
        foreach (var kvp in currencyDict)
        {
            if (Enum.TryParse(kvp.Key, out CurrencyType loadedType))
            {
                SimpleCurrency newCurrency = new(loadedType, kvp.Value);
                AddNewCurrency(newCurrency);
            }
        }
    }
    public void LoadAll(Dictionary<CurrencyType, HugeInt> currencyDict)
    {
        foreach (var kvp in currencyDict)
        {
            SimpleCurrency newCurrency = new(kvp.Key, kvp.Value);
            AddNewCurrency(newCurrency);
        }
    }
}
