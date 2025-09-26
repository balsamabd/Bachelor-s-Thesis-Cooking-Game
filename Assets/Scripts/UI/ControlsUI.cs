using UnityEngine;

public class ControlsUI : MonoBehaviour
{
    [SerializeField] private GameObject pressFText; // optional UI text "Press F to continue"

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            // Load tutorial gameplay (GameScene)
            Loader.Load(Loader.Scene.GameScene);
        }
    }
}
