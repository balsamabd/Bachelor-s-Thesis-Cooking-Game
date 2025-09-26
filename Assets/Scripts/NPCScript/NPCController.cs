using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour, IKitchenObjectParent
{
    [Header("Waypoints (0=Deliver/Offscreen, 1=Idle, 2=Pickup)")]
    [SerializeField] private Transform[] waypoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private Animator animator;

    [Header("Pickup")]
    [SerializeField] private Transform npcHoldPoint;
    [SerializeField] private float pickupDelay = 0.25f;

    [Header("Start Delay")]
    [SerializeField] private float initialStartDelay = 3f; // Wait at waypoint[0] before first move

    private readonly Queue<PlateKitchenObject> recipeQueue = new Queue<PlateKitchenObject>();
    private KitchenObject kitchenObject;

    private bool isMoving;
    private Transform currentTarget;
    private bool didInitialStartDelay = false;

    private Coroutine mainRoutine;

    public Transform GetKitchenObjectFollowTransform() => npcHoldPoint;
    public void SetKitchenObject(KitchenObject ko) => kitchenObject = ko;
    public KitchenObject GetKitchenObject() => kitchenObject;
    public void ClearKitchenObject() => kitchenObject = null;
    public bool HasKitchenObject() => kitchenObject != null;

    private void Awake()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged += OnGameStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (KitchenGameManager.Instance != null)
        {
            KitchenGameManager.Instance.OnStateChanged -= OnGameStateChanged;
        }
    }

    private void Start()
    {
        if (waypoints == null || waypoints.Length < 3)
        {
            Debug.LogError("[NPCController] Need 3 waypoints: 0=Deliver/Off, 1=Idle, 2=Pickup");
            enabled = false;
            return;
        }

        // Start off-screen
        transform.position = waypoints[0].position;

        // Start only if in the gameplay state
        if (KitchenGameManager.Instance == null || KitchenGameManager.Instance.CurrentMode == KitchenGameManager.GameMode.Gameplay)
        {
            mainRoutine = StartCoroutine(MainRoutine());
        }
        else
        {
            SetIdleAnim(false);
        }
    }

    private void OnGameStateChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance == null) return;

        if (KitchenGameManager.Instance.IsGameOver())
        {
            // Stop immediately
            StopMovingAndCoroutines();
        }
        else if (KitchenGameManager.Instance.CurrentMode == KitchenGameManager.GameMode.Gameplay)
        {
            // Resume if not already running
            if (mainRoutine == null)
            {
                mainRoutine = StartCoroutine(MainRoutine());
            }
        }
        else
        {
            // cowntdown: stop routine but don’t reset position/held item
            StopMovingAndCoroutines();
        }
    }

    private void StopMovingAndCoroutines()
    {
        if (mainRoutine != null)
        {
            StopCoroutine(mainRoutine);
            mainRoutine = null;
        }

        // Also stop any active MoveToWaypoint coroutine if currently in one
        isMoving = false;
        currentTarget = null;
        SetIdleAnim(false);
        // Keep whatever the NPC is holding
    }

    private IEnumerator MainRoutine()
    {
        while (true)
        {
            //gameplay mode
            if (KitchenGameManager.Instance != null &&
                KitchenGameManager.Instance.CurrentMode != KitchenGameManager.GameMode.Gameplay)
            {
                yield return null;
                continue;
            }

            // One time delay before the first move to waypoint[1]
            if (!didInitialStartDelay)
            {
                yield return new WaitForSeconds(initialStartDelay);
                didInitialStartDelay = true;
            }

            //idle
            yield return MoveToWaypoint(waypoints[1]);
            if (BreakIfNotGameplay()) yield break;

            // Wait until there is a plate queued and NPC hands are free
            while ((recipeQueue.Count == 0 || HasKitchenObject()))
            {
                if (BreakIfNotGameplay()) yield break;
                yield return null;
            }

            // Fetch next plate on the DeliveryCounter
            PlateKitchenObject plateOnCounter = recipeQueue.Dequeue();

            // Go to pickup point
            yield return MoveToWaypoint(waypoints[2]);
            if (BreakIfNotGameplay()) yield break;

            // update ownership of plate
            if (plateOnCounter != null)
            {
                plateOnCounter.SetKitchenObjectParent(this);
            }

            // Small pickup pause for animation
            yield return new WaitForSeconds(pickupDelay);
            if (BreakIfNotGameplay()) yield break;

            // Go deliver (offscreen)
            yield return MoveToWaypoint(waypoints[0]);
            if (BreakIfNotGameplay()) yield break;

            // Destroy visually
            if (HasKitchenObject())
            {
                kitchenObject.DestroySelf();
                ClearKitchenObject();
            }

            // Return to idle
            yield return MoveToWaypoint(waypoints[1]);
            if (BreakIfNotGameplay()) yield break;
        }
    }

    private bool BreakIfNotGameplay()
    {
        if (KitchenGameManager.Instance != null &&
            KitchenGameManager.Instance.CurrentMode != KitchenGameManager.GameMode.Gameplay)
        {
            // Stop the whole behavior immediately
            StopMovingAndCoroutines();
            return true;
        }
        return false;
    }

    private IEnumerator MoveToWaypoint(Transform waypoint)
    {
        currentTarget = waypoint;
        isMoving = true;
        SetIdleAnim(true);

        while (Vector3.Distance(transform.position, waypoint.position) > 0.1f)
        {
            if (KitchenGameManager.Instance != null && KitchenGameManager.Instance.IsGameOver())
            {
                break;
            }
            if (KitchenGameManager.Instance != null &&
                KitchenGameManager.Instance.CurrentMode != KitchenGameManager.GameMode.Gameplay)
            {
                break;
            }

            // Move
            transform.position = Vector3.MoveTowards(
                transform.position,
                waypoint.position,
                moveSpeed * Time.deltaTime
            );

            // Rotate toward target
            Vector3 dir = (waypoint.position - transform.position).normalized;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
            }

            yield return null;
        }

        SetIdleAnim(false);
        isMoving = false;
        currentTarget = null;
        yield return null;
    }

    // called by DeliveryCounter when a player drops a ready plate
    public void RecipeReady(PlateKitchenObject plate)
    {
        if (plate != null) recipeQueue.Enqueue(plate);
    }

    public bool IsMoving => isMoving;
    public Transform CurrentTarget => currentTarget;
    public Transform[] Waypoints => waypoints;

    private void SetIdleAnim(bool walking)
    {
        if (animator != null) animator.SetBool("IsWalking", walking);
    }
}
