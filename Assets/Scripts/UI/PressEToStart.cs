using UnityEngine;

public class PressEToStart : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            KitchenGameManager.Instance.StartGameplay(KitchenGameManager.GameMode.Gameplay);
            Loader.Load(Loader.Scene.GameScene);
        }
    }
}
