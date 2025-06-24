using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SP_Error : MonoBehaviour
{
    private SP_ScoreManager scoreManager;
    public Image errorImage;           // Reference to the UI Image
    public AudioSource errorSound;     // Reference to the AudioSource
    public float fadeDuration = 1.0f;  // Duration in seconds for fading
    private Coroutine imageFadeCoroutine;
    private Coroutine soundFadeCoroutine;
    public bool IsAI;

    private void Start()
    {
        scoreManager = FindAnyObjectByType<SP_ScoreManager>();
        if (scoreManager == null)
        {
            Debug.LogError($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] SP_ScoreManager not found in scene!");
        }

        // Initialize image to be invisible
        if (errorImage != null)
        {
            Color color = errorImage.color;
            color.a = 0f;
            errorImage.color = color;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (scoreManager == null)
        {
            Debug.LogWarning($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Error] ScoreManager not assigned!");
            return;
        }

        if (other.gameObject.CompareTag("Player1"))
        {
            scoreManager.DecrementPlayer1Score();
            ActivateErrorEffects();
        }
        else if (other.gameObject.CompareTag("Player2"))
        {
            scoreManager.DecrementPlayer2Score();
            ActivateErrorEffects();
        }
    }

    private void ActivateErrorEffects()
    {
        if (!IsAI)
        {
            // Activate and fade image
            if (errorImage != null)
            {
                if (imageFadeCoroutine != null)
                {
                    StopCoroutine(imageFadeCoroutine);
                }
                errorImage.color = new Color(errorImage.color.r, errorImage.color.g, errorImage.color.b, 1f);
                imageFadeCoroutine = StartCoroutine(FadeImage(errorImage));
            }

            // Activate and fade sound
            if (errorSound != null)
            {
                if (soundFadeCoroutine != null)
                {
                    StopCoroutine(soundFadeCoroutine);
                }
                errorSound.volume = 1f;
                errorSound.Play();
                soundFadeCoroutine = StartCoroutine(FadeSound(errorSound));
            }
        }

    }

    private IEnumerator FadeImage(Image image)
    {
        float time = 0f;
        Color color = image.color;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, time / fadeDuration);
            image.color = color;
            yield return null;
        }
        color.a = 0f;
        image.color = color;
    }

    private IEnumerator FadeSound(AudioSource sound)
    {
        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            sound.volume = Mathf.Lerp(1f, 0f, time / fadeDuration);
            yield return null;
        }
        sound.volume = 0f;
        sound.Stop();
    }
}