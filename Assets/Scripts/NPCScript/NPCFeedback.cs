using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;

public class NPCFeedback : MonoBehaviour
{
    [Header("UI & Audio")]
    [SerializeField] private TMP_Text chatBubbleText;
    [SerializeField] private GameObject chatBubble;
    [SerializeField] private string[] dialogue;
    [SerializeField] private AudioClip npcTalkSound;

    [Header("Timing")]
    [SerializeField] private Vector2 randomVisibleRange = new Vector2(7f, 10f);  // how long the bubble stays visible
    [SerializeField] private Vector2 betweenLineDelayRange = new Vector2(30f, 40f); // silence between lines
    [SerializeField] private float waitAfterWaypoint0 = 10f;
    [SerializeField] private float initialFirstLineDelay = 20f;

    private NPCController npcController;
    private AudioSource audioSource;
    private bool justCameFromWaypoint0 = false;
    private bool didInitialSpeechDelay = false;

    private int dialogueIndex = 0; // track current line
    private bool dialogueFinished = false; // stop once done

    private void Start()
    {
        npcController = GetComponent<NPCController>();
        if (npcController == null)
        {
            Debug.LogError("NPCController not found on NPCFeedback object!");
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();

        if (chatBubble == null || chatBubbleText == null)
        {
            Debug.LogError("Chat bubble or text reference not set in NPCFeedback!");
            enabled = false;
            return;
        }

        chatBubble.SetActive(false);
        StartCoroutine(SpeakDialogueInOrder());
    }

    private void Update()
    {
        UpdateChatBubblePosition();
    }

    private void UpdateChatBubblePosition()
    {
        if (Camera.main == null || chatBubble == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 3.5f, 0));
        chatBubble.transform.position = screenPos;
    }

    private IEnumerator SpeakDialogueInOrder()
    {
        while (true)
        {
            // Stop if dialogue is finished
            if (dialogueFinished)
            {
                chatBubble.SetActive(false);
                yield break; // completely stop coroutine
            }

            // Only speak in gameplay mode
            if (KitchenGameManager.Instance.CurrentMode != KitchenGameManager.GameMode.Gameplay)
            {
                chatBubble.SetActive(false);
                yield return null;
                continue;
            }

            // Speak only when NPC is not moving and near waypoint 1 or 2
            if (!npcController.IsMoving)
            {
                int nearestIndex = GetNearestWaypointIndex();
                if (nearestIndex == 1 || nearestIndex == 2)
                {
                    if (justCameFromWaypoint0)
                    {
                        yield return new WaitForSeconds(waitAfterWaypoint0);
                        justCameFromWaypoint0 = false;
                    }

                    if (!didInitialSpeechDelay)
                    {
                        yield return new WaitForSeconds(initialFirstLineDelay);
                        didInitialSpeechDelay = true;
                    }

                    // Speak next dialogue line
                    if (dialogueIndex < dialogue.Length)
                    {
                        string dialogueMessage = dialogue[dialogueIndex];
                        dialogueIndex++;

                        ShowDialogue(dialogueMessage);

                        // Stay visible
                        float visibleTime = Random.Range(randomVisibleRange.x, randomVisibleRange.y);
                        yield return new WaitForSeconds(visibleTime);

                        chatBubble.SetActive(false);

                        // Pause before next line
                        float silence = Random.Range(betweenLineDelayRange.x, betweenLineDelayRange.y);
                        yield return new WaitForSeconds(silence);
                    }
                    else
                    {
                        // No more lines
                        dialogueFinished = true;
                        chatBubble.SetActive(false);
                        yield break; // stop coroutine forever
                    }
                }
            }

            yield return null;
        }
    }

    // Helper to find nearest waypoint
    private int GetNearestWaypointIndex()
    {
        int nearest = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < npcController.Waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, npcController.Waypoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
        }

        return nearest;
    }

    private void ShowDialogue(string message)
    {
        if (string.IsNullOrEmpty(message)) return;

        chatBubble.SetActive(true);
        chatBubbleText.text = message;

        if (npcTalkSound != null && audioSource != null)
            audioSource.PlayOneShot(npcTalkSound);
    }

    public void CameFromWaypoint0()
    {
        justCameFromWaypoint0 = true;
    }
}

