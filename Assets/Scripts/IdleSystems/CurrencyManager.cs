using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : Singleton<CurrencyManager>
{
    private readonly Dictionary<CurrencyType, ICurrency> currencies = new();
    public event Action<CurrencyType, double> OnCurrencyChanged;
    public void ModifyCurrency(CurrencyType currencyType, double value)
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

    public bool TrySpendCurrency(CurrencyType currencyType, double value)
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

    public double GetCurrency(CurrencyType currencyType)
    { 
        return currencies.TryGetValue(currencyType, out ICurrency currency) ? currency.Amount : 0; 
    }
}
