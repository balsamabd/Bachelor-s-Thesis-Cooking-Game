using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryCounter : BaseCounter
{
    [SerializeField] private NPCController npcController; // Assign in gameplay scenes only

    public static DeliveryCounter Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        // No error if npcController is null — that's expected in Tutorial mode.
    }

    public override void Interact(Player player)
    {
        if (player == null || !player.HasKitchenObject()) return;

        // Only accept if the player is holding a plate
        KitchenObject ko = player.GetKitchenObject();
        if (!ko.TryGetPlate(out PlateKitchenObject plate)) return;

        // Only one object on this counter at a time (optional guard)
        if (HasKitchenObject()) return;

        // 1) Place on counter (ownership + transform handled via API)
        plate.SetKitchenObjectParent(this);

        // 2) Score immediately (success/fail + update lists)
        if (DeliveryManager.Instance != null)
        {
            DeliveryManager.Instance.DeliverRecipe(plate);
        }

        // 3) Mode-specific behavior
        var gm = KitchenGameManager.Instance;
        if (gm != null && gm.CurrentMode == KitchenGameManager.GameMode.Tutorial)
        {
            // Tutorial: no NPC — destroy the plate right away
            plate.DestroySelf();
            return;
        }

        // Gameplay: NPC does pickup + walk + destroy (visual only)
        if (npcController != null)
        {
            npcController.RecipeReady(plate);
        }
        else
        {
            // Failsafe: if NPC is missing in a gameplay scene, behave like tutorial
            Debug.LogWarning("[DeliveryCounter] Gameplay mode but NPCController not assigned. Destroying plate as fallback.");
            plate.DestroySelf();
        }
    }
}
