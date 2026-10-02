using UnityEngine;
using UnityEngine.UI;

public class DetectionMeter : MonoBehaviour
{
    [Header("UI")]
    public Slider detectionSlider;
    public TMPro.TMP_Text stateText;

    [Header("Detection")]
    public EnemyPatrol[] enemies;
    public SecurityCamera[] securityCameras;

    [Header("Meter Smoothing")]
    public float fillSpeed = 3f;

    private float targetValue = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pursuitSound;

    private EnemyPatrol.EnemyState previousHighestState =
        EnemyPatrol.EnemyState.Patrol;

    void Start()
    {
        detectionSlider.value = 0f;

        // Automatically find all enemies and security cameras.
        enemies = FindObjectsByType<EnemyPatrol>(FindObjectsSortMode.None);
        securityCameras = FindObjectsByType<SecurityCamera>(FindObjectsSortMode.None);
    }

    void Update()
    {
        targetValue = GetHighestDetection();

        detectionSlider.value = Mathf.MoveTowards(
            detectionSlider.value,
            targetValue,
            fillSpeed * Time.deltaTime
        );

        UpdateStateText();
        CheckForPursuitSound();
    }

    float GetHighestDetection()
    {
        float highestDetection = 0f;

        // Check enemies.
        foreach (EnemyPatrol enemy in enemies)
        {
            if (enemy == null)
                continue;

            float enemyDetection = enemy.GetDetectionLevel();

            if (enemyDetection > highestDetection)
            {
                highestDetection = enemyDetection;
            }
        }

        // Check security cameras.
        foreach (SecurityCamera camera in securityCameras)
        {
            if (camera == null)
                continue;

            if (camera.playerDetected)
            {
                highestDetection = 1f;
            }
        }

        return highestDetection;
    }
    void UpdateStateText()
    {
        EnemyPatrol.EnemyState highestState =
            EnemyPatrol.EnemyState.Patrol;

        foreach (EnemyPatrol enemy in enemies)
        {
            if (enemy == null)
                continue;

            if ((int)enemy.currentState > (int)highestState)
            {
                highestState = enemy.currentState;
            }
        }

        stateText.text = highestState.ToString();
    }
    void CheckForPursuitSound()
    {
        EnemyPatrol.EnemyState highestState =
            EnemyPatrol.EnemyState.Patrol;

        foreach (EnemyPatrol enemy in enemies)
        {
            if (enemy == null)
                continue;

            EnemyPatrol.EnemyState state = enemy.currentState;

            if (GetStatePriority(state) > GetStatePriority(highestState))
            {
                highestState = state;
            }
        }

        if (highestState == EnemyPatrol.EnemyState.Pursuing &&
            previousHighestState != EnemyPatrol.EnemyState.Pursuing)
        {
            if (audioSource != null && pursuitSound != null)
            {
                audioSource.PlayOneShot(pursuitSound);
            }
        }

        previousHighestState = highestState;
    }
    int GetStatePriority(EnemyPatrol.EnemyState state)
    {
        switch (state)
        {
            case EnemyPatrol.EnemyState.Pursuing:
                return 4;

            case EnemyPatrol.EnemyState.Alerted:
                return 3;

            case EnemyPatrol.EnemyState.Detecting:
                return 2;

            case EnemyPatrol.EnemyState.Patrol:
                return 1;

            case EnemyPatrol.EnemyState.Returning:
                return 0;

            default:
                return 0;
        }
    }
}