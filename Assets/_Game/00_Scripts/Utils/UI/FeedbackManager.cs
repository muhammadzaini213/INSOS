using System.Collections;
using Slafurry.System.Audio;
using UnityEngine;
using UnityEngine.InputSystem;

public class FeedbackManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Canvas canvas;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject correctPrefab;

    [SerializeField]
    private GameObject wrongPrefab;

    [Header("Audio")]
    [SerializeField]
    private string correctSFXCategory = "Feedback";

    [SerializeField]
    private string correctSFXEffect = "Benar";

    [SerializeField]
    private string wrongSFXCategory = "Feedback";

    [SerializeField]
    private string wrongSFXEffect = "Salah";

    [Header("Pool")]
    [SerializeField]
    private int poolSize = 4;

    [Header("Animation")]
    [SerializeField]
    private float popScale = 1.3f;

    [SerializeField]
    private float popDuration = 0.15f;

    [SerializeField]
    private float shrinkDuration = 0.15f;

    [SerializeField]
    private float fadeDuration = 0.25f;

    [SerializeField]
    private float holdDuration = 0.1f;

    [SerializeField]
    private AnimationCurve popCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private bool useUnscaledTime = true;

    private Pool _correctPool;
    private Pool _wrongPool;
    private Camera _cam;
    private Vector2 _lastPointerPos;

    public static FeedbackManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        _cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        _correctPool = new Pool(correctPrefab, canvas.transform, poolSize);
        _wrongPool = new Pool(wrongPrefab, canvas.transform, poolSize);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        Vector2 pos = ReadPointerPosition();
        if (pos.sqrMagnitude > 0.01f)
            _lastPointerPos = pos;
    }

    public void ShowCorrect()
    {
        Audio.PlaySFX2D(correctSFXCategory, correctSFXEffect);
        ShowAt(_correctPool, GetPointerPosition());
    }

    public void ShowCorrectAt(Vector2 screenPos)
    {
        Audio.PlaySFX2D(correctSFXCategory, correctSFXEffect);
        ShowAt(_correctPool, screenPos);
    }

    public void ShowCorrectAtWorld(Vector3 worldPos)
    {
        Audio.PlaySFX2D(correctSFXCategory, correctSFXEffect);
        ShowAt(_correctPool, WorldToScreen(worldPos));
    }

    public void ShowWrong()
    {
        Audio.PlaySFX2D(wrongSFXCategory, wrongSFXEffect);
        ShowAt(_wrongPool, GetPointerPosition());
    }

    public void ShowWrongAt(Vector2 screenPos)
    {
        Audio.PlaySFX2D(wrongSFXCategory, wrongSFXEffect);
        ShowAt(_wrongPool, screenPos);
    }

    public void ShowWrongAtWorld(Vector3 worldPos)
    {
        Audio.PlaySFX2D(wrongSFXCategory, wrongSFXEffect);
        ShowAt(_wrongPool, WorldToScreen(worldPos));
    }

    private void ShowAt(Pool pool, Vector2 screenPos)
    {
        var item = pool.Get();
        if (item == null)
            return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            _cam,
            out localPos
        );

        item.RT.anchoredPosition = localPos;
        item.RT.localScale = Vector3.zero;
        item.CG.alpha = 1f;
        item.RT.gameObject.SetActive(true);

        StartCoroutine(PopRoutine(item));
    }

    private Vector2 WorldToScreen(Vector3 worldPos)
    {
        if (_cam == null)
            _cam = Camera.main;
        return _cam.WorldToScreenPoint(worldPos);
    }

    private IEnumerator PopRoutine(Pool.Item item)
    {
        Vector3 baseScale = Vector3.one;

        float t = 0f;
        while (t < popDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = popCurve.Evaluate(Mathf.Clamp01(t / popDuration));
            item.RT.localScale = Vector3.LerpUnclamped(baseScale, baseScale * popScale, p);
            yield return null;
        }

        t = 0f;
        while (t < shrinkDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float p = popCurve.Evaluate(Mathf.Clamp01(t / shrinkDuration));
            item.RT.localScale = Vector3.LerpUnclamped(baseScale * popScale, baseScale, p);
            yield return null;
        }

        item.RT.localScale = baseScale;

        if (holdDuration > 0f)
            yield return useUnscaledTime
                ? new WaitForSecondsRealtime(holdDuration)
                : new WaitForSeconds(holdDuration);

        t = 0f;
        while (t < fadeDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            item.CG.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        item.CG.alpha = 0f;
        item.RT.gameObject.SetActive(false);
    }

    private Vector2 GetPointerPosition()
    {
        return _lastPointerPos;
    }

    private Vector2 ReadPointerPosition()
    {
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasReleasedThisFrame)
                return Mouse.current.position.ReadValue();
        }

        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed || touch.press.wasReleasedThisFrame)
                    return touch.position.ReadValue();
            }
        }

        return _lastPointerPos;
    }

    [System.Serializable]
    private class Pool
    {
        public class Item
        {
            public RectTransform RT;
            public CanvasGroup CG;
        }

        private Item[] _items;
        private int _nextIndex;

        public Pool(GameObject prefab, Transform parent, int size)
        {
            _items = new Item[size];
            for (int i = 0; i < size; i++)
            {
                GameObject go = Instantiate(prefab, parent);
                go.SetActive(false);

                CanvasGroup cg = go.GetComponent<CanvasGroup>();
                if (cg == null)
                    cg = go.AddComponent<CanvasGroup>();

                _items[i] = new Item { RT = go.GetComponent<RectTransform>(), CG = cg };
            }
        }

        public Item Get()
        {
            for (int i = 0; i < _items.Length; i++)
            {
                int idx = (_nextIndex + i) % _items.Length;
                if (!_items[idx].RT.gameObject.activeSelf)
                {
                    _nextIndex = (idx + 1) % _items.Length;
                    return _items[idx];
                }
            }

            Item stolen = _items[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _items.Length;
            return stolen;
        }
    }
}
