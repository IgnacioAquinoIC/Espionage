using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class HideBarrel : MonoBehaviour
{
    [Header("Interaction")]
    public float interactionDistance = 5f;
    public Key interactionKey = Key.E;

    [Header("Hiding")]
    public float hideTime = 10f;

    [Header("Camera")]
    public Camera barrelCamera;

    [Header("Reset Sequence")]
    public BarrelResetSequence resetSequence;

    [Header("UI")]
    public TMPro.TMP_Text hidePrompt;

    private PlayerMovement playerMovement;
    private Transform playerTransform;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool playerInside = false;
    private Coroutine hideCoroutine;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("HideBarrel could not find the Player.");
            return;
        }

        playerTransform = playerObject.transform;
        playerMovement = playerObject.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("Player does not have a PlayerMovement component.");
        }
    }

    void Update()
    {
        UpdateHidePrompt();

        if (playerMovement == null)
            return;

        if (playerInside)
        {
            if (Keyboard.current[interactionKey].wasPressedThisFrame)
            {
                ExitBarrel();
            }

            return;
        }

        if (Keyboard.current[interactionKey].wasPressedThisFrame)
        {
            TryEnterBarrel();
        }
    }

    void TryEnterBarrel()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("No Main Camera found.");
            return;
        }

        Ray ray = new Ray(
            cam.transform.position,
            cam.transform.forward
        );

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            interactionDistance
        );

        bool hitBarrel = false;

        foreach (RaycastHit hit in hits)
        {
            // Ignore the player's own collider.
            if (hit.collider.CompareTag("Player"))
                continue;

            Debug.Log("E ray hit: " + hit.collider.name);

            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform))
            {
                hitBarrel = true;
                break;
            }
        }

        if (!hitBarrel)
        {
            Debug.Log("E was pressed, but the barrel was not hit.");
            return;
        }

        Debug.Log("Barrel interaction detected.");

        EnterBarrel();
    }

    bool CanHide()
    {
        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(
            FindObjectsSortMode.None
        );

        foreach (EnemyPatrol enemy in enemies)
        {
            if (enemy.CanSeePlayer())
            {
                return false;
            }
        }

        return true;
    }
    void UpdateHidePrompt()
    {
        if (playerInside)
        {
            hidePrompt.gameObject.SetActive(false);
            return;
        }

        Camera cam = Camera.main;

        if (cam == null)
        {
            hidePrompt.gameObject.SetActive(false);
            return;
        }

        Ray ray = new Ray(
            cam.transform.position,
            cam.transform.forward
        );

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            interactionDistance
        );

        bool hitBarrel = false;

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Player"))
                continue;

            if (hit.collider.gameObject == gameObject ||
                hit.collider.transform.IsChildOf(transform))
            {
                hitBarrel = true;
                break;
            }
        }

        hidePrompt.gameObject.SetActive(hitBarrel);
    }

    void EnterBarrel()
    {
        playerInside = true;

        originalPosition = playerTransform.position;
        originalRotation = playerTransform.rotation;

        playerMovement.movementLocked = true;

        playerMovement.rigidbody.linearVelocity = Vector3.zero;
        playerMovement.rigidbody.angularVelocity = Vector3.zero;

        Renderer[] renderers =
            playerTransform.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        Debug.Log("BEFORE SWITCH - Player Camera: " + playerMovement.playerCamera.enabled);
        Debug.Log("BEFORE SWITCH - Barrel Camera: " + barrelCamera.enabled);

        playerMovement.playerCamera.enabled = false;
        barrelCamera.enabled = true;

        Debug.Log("AFTER SWITCH - Player Camera: " + playerMovement.playerCamera.enabled);
        Debug.Log("AFTER SWITCH - Barrel Camera: " + barrelCamera.enabled);

        playerMovement.inBarrel = true;
        playerMovement.currentBarrelCamera = barrelCamera;

        Debug.Log("Player entered barrel.");

        hideCoroutine = StartCoroutine(HideTimer());
    }

    IEnumerator HideTimer()
    {
        yield return new WaitForSeconds(hideTime);

        if (resetSequence != null)
        {
            resetSequence.BeginResetSequence();
        }
        else
        {
            Debug.LogWarning("No BarrelResetSequence assigned.");
        }

        hideCoroutine = null;
    }

    void ExitBarrel()
    {
        playerInside = false;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        playerTransform.position = originalPosition;
        playerTransform.rotation = originalRotation;

        Renderer[] renderers =
            playerTransform.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = true;
        }

        barrelCamera.enabled = false;
        playerMovement.playerCamera.enabled = true;

        playerMovement.inBarrel = false;
        playerMovement.currentBarrelCamera = null;

        playerMovement.movementLocked = false;

        Debug.Log("Player exited barrel.");
    }
}