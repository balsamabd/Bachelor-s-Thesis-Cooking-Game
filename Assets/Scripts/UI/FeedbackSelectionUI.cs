using UnityEngine;
using UnityEngine.UI;

public class FeedbackSelectionUI : MonoBehaviour
{
    [SerializeField] private Button humanButton;
    [SerializeField] private Button npcButton;

    public static bool IsNPCFlow { get; private set; } = false;

    private void Awake()
    {
        humanButton.onClick.AddListener(OnHumanSelected);
        npcButton.onClick.AddListener(OnNPCSelected);
    }

    private void OnHumanSelected()
    {
        IsNPCFlow = false;

        // Human flow: start tutorial in Gameplay scene
        KitchenGameManager.Instance.StartGameplay(KitchenGameManager.GameMode.Tutorial);

        // Load the gameplay scene where PressEToStart exists
        Loader.Load(Loader.Scene.SevenSLoadScene);
    }

    private void OnNPCSelected()
    {
        IsNPCFlow = true;

        // NPC flow: skip tutorial, start gameplay directly
        KitchenGameManager.Instance.StartGameplay(KitchenGameManager.GameMode.Gameplay);
        Loader.Load(Loader.Scene.SevenSLoadScene);
    }
}
