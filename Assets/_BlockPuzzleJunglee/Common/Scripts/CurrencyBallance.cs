using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CurrencyBallance : MonoBehaviour {
    private void Start()
    {
        UpdateBalance();
        CurrencyManager.onBalanceChanged += OnBalanceChanged;
    }

    private void UpdateBalance()
    {
        gameObject.SetText(CurrencyManager.GetBalance().ToString());
    }

    private void OnBalanceChanged()
    {
        UpdateBalance();
    }

    private void OnDestroy()
    {
        CurrencyManager.onBalanceChanged -= OnBalanceChanged;
    }
}
