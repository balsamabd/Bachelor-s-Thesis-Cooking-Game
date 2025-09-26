using UnityEngine;
using System.Collections;

public class SevenSLoader : MonoBehaviour
{
    [SerializeField] private float waitBeforeTutorial = 5f;

    private void Start()
    {
        StartCoroutine(WaitAndLoad());
    }

    private IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(waitBeforeTutorial);

        // Check if human or NPC flow
        if (FeedbackSelectionUI.IsNPCFlow)
        {
            // NPC flow: skip tutorial, go directly to gameplay
            KitchenGameManager.Instance.StartGameplay(KitchenGameManager.GameMode.Gameplay);
            Loader.Load(Loader.Scene.GameScene);
        }
        else
        {
            // Human flow: start Tutorial
            KitchenGameManager.Instance.StartGameplay(KitchenGameManager.GameMode.Gameplay);
            Loader.Load(Loader.Scene.PressEToStartScene);
        }
    }
}
