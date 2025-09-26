using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private RecipeListSO recipeListSO;
    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    private int successfulRecipesAmount;
    private int failedRecipesAmount;

    private void Awake()
    {
        Instance = this;
        waitingRecipeSOList = new List<RecipeSO>();
        spawnRecipeTimer = spawnRecipeTimerMax;
    }

    private void Update()
    {
        spawnRecipeTimer -= Time.deltaTime;

        while (spawnRecipeTimer <= 0f)
        {
            spawnRecipeTimer += spawnRecipeTimerMax;

            if (waitingRecipeSOList.Count < waitingRecipesMax)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                waitingRecipeSOList.Add(waitingRecipeSO);

                if (KitchenGameManager.Instance != null &&
                    KitchenGameManager.Instance.IsGamePlaying() &&
                    KitchenGameManager.Instance.CurrentMode == KitchenGameManager.GameMode.Gameplay)
                {
                    GameLogger.LogOrderSpawned();
                }

                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateMatches = true;
                foreach (KitchenObjectSO recipeSO in waitingRecipeSO.kitchenObjectSOList)
                {
                    if (!plateKitchenObject.GetKitchenObjectSOList().Contains(recipeSO))
                    {
                        plateMatches = false;
                        break;
                    }
                }

                if (plateMatches)
                {
                    // Correct recipe
                    successfulRecipesAmount++;
                    waitingRecipeSOList.RemoveAt(i);

                    if (KitchenGameManager.Instance != null &&
                        KitchenGameManager.Instance.IsGamePlaying() &&
                        KitchenGameManager.Instance.CurrentMode == KitchenGameManager.GameMode.Gameplay)
                    {
                        GameLogger.LogResult(true);
                    }

                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
                    return;
                }
            }
        }

        // Wrong recipe
        failedRecipesAmount++;
        if (KitchenGameManager.Instance != null &&
            KitchenGameManager.Instance.IsGamePlaying() &&
            KitchenGameManager.Instance.CurrentMode == KitchenGameManager.GameMode.Gameplay)
        {
            GameLogger.LogResult(false);
        }

        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList() => waitingRecipeSOList;
    public int GetSuccessfulRecipesAmount() => successfulRecipesAmount;
    public int GetFailedRecipesAmount() => failedRecipesAmount;
}
