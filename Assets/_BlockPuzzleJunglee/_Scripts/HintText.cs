using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class HintText : MonoBehaviour {

    private void Start()
    {
        GameState.hint.onValueChanged += OnValueChanged;
        OnValueChanged();
    }

    private void OnValueChanged()
    {
        GetComponent<TextMeshProUGUI>().text = GameState.hint.GetValue().ToString();
    }

    private void OnDestroy()
    {
        GameState.hint.onValueChanged -= OnValueChanged;
    }
}
