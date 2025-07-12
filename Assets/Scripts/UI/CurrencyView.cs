using TMPro;
using UnityEngine;

public class CurrencyView : MonoBehaviour
{
    [SerializeField] private CurrencyType _currencyType;
    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private string displayPrefix = string.Empty;
    [SerializeField] private string displaySuffix = string.Empty;


    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
    }

    private void OnCurrencyChanged(CurrencyType changedType, double value)
    {
        if(changedType == _currencyType)
        {
            Refresh(value);
        }
    }

    public void Refresh(double value)
    {
        currencyText.text = displayPrefix + value.ToString() + displaySuffix;
    }
}
