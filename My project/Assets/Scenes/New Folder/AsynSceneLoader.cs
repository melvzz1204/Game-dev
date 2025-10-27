using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AsyncSceneLoader : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Slider with min = 0, max = 100")]
    public Slider progressSlider;
    public Text progressText; // optional text for percentage
    public GameObject loadingPanel; // optional panel

    [Header("Scene Settings")]
    public string sceneToLoad = "MainMenu";
    [Range(0.1f, 30f)] public float smoothSpeed = 2f;
    [Tooltip("Time to hold at 100% before switching scene")]
    public float holdTimeAtFull = 0.5f;

    private AsyncOperation op;
    private float targetProgress = 0f;
    private float displayedProgress = 0f;

    void Start()
    {
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (progressSlider != null) progressSlider.value = 0f;
        if (progressText != null) progressText.text = "0%";

        StartCoroutine(LoadSceneCoroutine(sceneToLoad));
    }

    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            // Unity's Async progress goes 0–0.9 before activation
            targetProgress = Mathf.Clamp01(op.progress / 0.9f) * 100f; // convert to 0–100

            // Smoothly interpolate progress
            displayedProgress = Mathf.MoveTowards(displayedProgress, targetProgress, smoothSpeed * 50f * Time.deltaTime);

            // ✅ Update slider and text
            if (progressSlider != null)
                progressSlider.value = displayedProgress;

            if (progressText != null)
                progressText.text = Mathf.RoundToInt(displayedProgress) + "%";

            // ✅ Once progress reaches 100%
            if (displayedProgress >= 99.9f)
            {
                if (progressText != null)
                    progressText.text = "100%";

                yield return new WaitForSeconds(holdTimeAtFull);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
