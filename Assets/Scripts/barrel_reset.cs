using UnityEngine;
using TMPro;
using System.Collections;

public class BarrelResetSequence : MonoBehaviour
{
    [Header("Fade")]
    public CanvasGroup fadePanel;
    public float fadeToBlackTime = 2f;
    public float fadeFromBlackTime = 2f;

    [Header("Message")]
    public TextMeshProUGUI messageText;
    public float messageDisplayTime = 2f;

    private bool sequenceRunning = false;

    void Start()
    {
        // Make sure the screen starts completely clear.
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(true);
        }

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }
    }

    public void BeginResetSequence()
    {
        if (sequenceRunning)
            return;

        StartCoroutine(ResetSequence());
    }

    IEnumerator ResetSequence()
    {
        sequenceRunning = true;

        // -------------------------
        // FADE TO BLACK
        // -------------------------

        yield return StartCoroutine(
            FadeCanvas(0f, 1f, fadeToBlackTime)
        );

        // -------------------------
        // RESET ENEMIES
        // -------------------------

        EnemyPatrol[] enemies = FindObjectsByType<EnemyPatrol>(
            FindObjectsSortMode.None
        );

        foreach (EnemyPatrol enemy in enemies)
        {
            enemy.ResetToPatrol();
        }

        Debug.Log("The enemies have returned to Patrol.");

        // -------------------------
        // SHOW MESSAGE
        // -------------------------

        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);

            messageText.text =
                "The guards could not find you.\n" +
                "They've gone back to their initial posts.";
        }

        yield return new WaitForSeconds(messageDisplayTime);

        // -------------------------
        // HIDE MESSAGE
        // -------------------------

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        // -------------------------
        // FADE BACK IN
        // -------------------------

        yield return StartCoroutine(
            FadeCanvas(1f, 0f, fadeFromBlackTime)
        );

        sequenceRunning = false;
    }

    IEnumerator FadeCanvas(float startAlpha, float endAlpha, float duration)
    {
        if (fadePanel == null)
            yield break;

        float timer = 0f;

        fadePanel.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float progress = timer / duration;

            fadePanel.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                progress
            );

            yield return null;
        }

        fadePanel.alpha = endAlpha;
    }
}