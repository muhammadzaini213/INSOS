using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class UIImageZoom : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float zoomTime = 3f;

    [SerializeField] private UnityEvent onZoomComplete;

    private Coroutine zoomCoroutine;

    public void ZoomIn()
    {
        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomImage());
    }

    private IEnumerator ZoomImage()
    {
        float elapsed = 0f;

        while (elapsed < zoomTime)
        {
            elapsed += Time.deltaTime;

            image.transform.localScale +=
                Vector3.one * zoomSpeed * Time.deltaTime;

            yield return null;
        }

        zoomCoroutine = null;

        onZoomComplete?.Invoke();
    }
}