using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class UIAnim : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image targetImage;

    [Header("Animation")]
    [Tooltip("Path relatif terhadap folder Resources.")]
    [SerializeField] private string resourcePath;

    [Header("Settings")]
    [SerializeField] private float fps = 12f;
    [SerializeField] private bool loop = false;
    [SerializeField] private bool playOnEnable = true;

    [Header("After Animation")]
    [SerializeField] private bool hideAfterFinish = false;

    private Sprite[] frames;
    private Coroutine animationCoroutine;

    private void OnEnable()
    {
        if (playOnEnable)
            Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    // =========================================================
    // PLAY
    // =========================================================

    public void Play()
    {
        Stop();

        if (!LoadFrames())
            return;

        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    // =========================================================
    // STOP
    // =========================================================

    public void Stop()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    // =========================================================
    // RESTART
    // =========================================================

    public void Restart()
    {
        Stop();
        Play();
    }

    // =========================================================
    // LOAD FRAMES
    // =========================================================

    private bool LoadFrames()
    {
        if (targetImage == null)
        {
            Debug.LogWarning(
                $"[{name}] UIAnim: Target Image belum diisi.",
                this
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            Debug.LogWarning(
                $"[{name}] UIAnim: Resource Path belum diisi.",
                this
            );

            return false;
        }

        Sprite[] loadedFrames =
            Resources.LoadAll<Sprite>(resourcePath);

        if (loadedFrames == null || loadedFrames.Length == 0)
        {
            Debug.LogWarning(
                $"[{name}] UIAnim: Tidak menemukan sprite di " +
                $"Resources/{resourcePath}",
                this
            );

            return false;
        }

        // Pastikan urutan berdasarkan nama file.
        frames = loadedFrames
            .OrderBy(sprite => sprite.name)
            .ToArray();

        Debug.Log(
            $"[{name}] UIAnim: {frames.Length} frames loaded " +
            $"dari Resources/{resourcePath}",
            this
        );

        return true;
    }

    // =========================================================
    // PLAY ANIMATION
    // =========================================================

    private IEnumerator PlayAnimation()
    {
        float interval = 1f / Mathf.Max(fps, 0.01f);

        do
        {
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i] == null)
                    continue;

                targetImage.sprite = frames[i];

                yield return new WaitForSeconds(interval);
            }

        } while (loop);

        animationCoroutine = null;

        if (hideAfterFinish)
        {
            targetImage.enabled = false;
        }
    }

    // =========================================================
    // SHOW / HIDE
    // =========================================================

    public void Show()
    {
        if (targetImage != null)
            targetImage.enabled = true;
    }

    public void Hide()
    {
        if (targetImage != null)
            targetImage.enabled = false;
    }

    // =========================================================
    // FPS
    // =========================================================

    public void SetFPS(float value)
    {
        fps = Mathf.Max(value, 0.01f);
    }
}