using TMPro;
using UnityEngine;

public class PurchaseQuantityToggle : MonoBehaviour
{
    [SerializeField] private int quantity = 1;
    private PurchaseQuantityController _purchaseQuantityController;

    private void Awake()
    {
        _purchaseQuantityController = GetComponentInParent<PurchaseQuantityController>();
    }

    public void OnToggleStateChanged(bool isOn)
    {
        if(isOn && _purchaseQuantityController != null)
        {
            _purchaseQuantityController.SetPurchaseQuantity(quantity);
        }
    }
}
