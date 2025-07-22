using System;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseQuantityController : Singleton<PurchaseQuantityController>
{
    public const int PURCHASE_ONE = 1; // Purchase specific int val?
    public const int PURCHASE_TEN = 10;
    public const int PURCHASE_ONE_HUNDRED = 100;
    public const int PURCHASE_MAX_POSSIBLE = int.MaxValue;
    public int CurrPurchaseQuantity { get; private set; }

    public event Action<int> OnPurchaseQuantityChanged;

    private void Start()
    {
        UpdatePurchaseQuantity();
    }

    public void SetPurchaseQuantity(int quantity)
    {
        CurrPurchaseQuantity = quantity;
        UpdatePurchaseQuantity();
    }

    private void UpdatePurchaseQuantity()
    {
        OnPurchaseQuantityChanged?.Invoke(CurrPurchaseQuantity);
    }
}
