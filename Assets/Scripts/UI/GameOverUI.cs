using System;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI recipesDeliveredText;

    private void Awake()
    {
        // Subscribe immediately
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged += OnKitchenStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged -= OnKitchenStateChanged;
        }
    }

    private void Start()
    {
        // Update UI immediately if game is already over
        UpdateUI();
    }

    private void OnKitchenStateChanged(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (KitchenGameManager.Instance != null && KitchenGameManager.Instance.IsGameOver())
        {
            Show();

            if (recipesDeliveredText != null && DeliveryManager.Instance != null)
            {
                recipesDeliveredText.text = DeliveryManager.Instance.GetSuccessfulRecipesAmount().ToString();
            }

            GameLogger.SaveResults();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
