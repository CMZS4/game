using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    [SerializeField] private Text keyText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpdateKeyText(int current)
    {
        if (keyText != null)
        {
            keyText.text = $"Anahtar: {current}";
        }
    }
}
