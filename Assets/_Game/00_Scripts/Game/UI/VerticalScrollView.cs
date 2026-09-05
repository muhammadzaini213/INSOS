using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Slafurry.Game.UI
{
    public class VerticalScrollView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform content;

        [SerializeField]
        private Button upButton;

        [SerializeField]
        private Button downButton;

        [Header("Layout")]
        [SerializeField]
        private int visibleCount = 3;

        [Header("Events")]
        [SerializeField]
        private UnityEvent onIndexChanged;

        public event Action OnIndexChanged;

        private int currentIndex;
        private int lastChildCount;

        public int ChildCount => content != null ? content.childCount : 0;

        private void Awake()
        {
            if (content == null)
                content = transform.Find("Panel") ?? transform.GetChild(0);

            SetupButtons();
        }

        private void OnEnable()
        {
            lastChildCount = ChildCount;
            ClampIndex();
            UpdateVisibility();
            UpdateButtons();
        }

        private void LateUpdate()
        {
            if (content == null)
                return;

            int count = content.childCount;
            if (count != lastChildCount)
            {
                lastChildCount = count;
                ClampIndex();
                UpdateVisibility();
                UpdateButtons();
            }
        }

        public void ScrollUp()
        {
            if (currentIndex <= 0)
                return;
            currentIndex--;
            Apply();
        }

        public void ScrollDown()
        {
            if (currentIndex >= ChildCount - visibleCount)
                return;
            currentIndex++;
            Apply();
        }

        public void ScrollToIndex(int index)
        {
            currentIndex = Mathf.Clamp(index, 0, Mathf.Max(0, ChildCount - visibleCount));
            Apply();
        }

        public void ResetToTop()
        {
            currentIndex = 0;
            Apply();
        }

        public void Refresh()
        {
            lastChildCount = ChildCount;
            ClampIndex();
            UpdateVisibility();
            UpdateButtons();
        }

        private void SetupButtons()
        {
            if (upButton != null)
                upButton.onClick.AddListener(ScrollUp);

            if (downButton != null)
                downButton.onClick.AddListener(ScrollDown);
        }

        private void ClampIndex()
        {
            int max = Mathf.Max(0, ChildCount - visibleCount);
            currentIndex = Mathf.Clamp(currentIndex, 0, max);
        }

        private void Apply()
        {
            ClampIndex();
            UpdateVisibility();
            UpdateButtons();
            OnIndexChanged?.Invoke();
            onIndexChanged?.Invoke();
        }

        private void UpdateVisibility()
        {
            if (content == null)
                return;

            int end = currentIndex + visibleCount;
            for (int i = 0; i < content.childCount; i++)
                content.GetChild(i).gameObject.SetActive(i >= currentIndex && i < end);
        }

        private void UpdateButtons()
        {
            int max = Mathf.Max(0, ChildCount - visibleCount);

            if (upButton != null)
                upButton.interactable = currentIndex > 0;

            if (downButton != null)
                downButton.interactable = currentIndex < max;
        }
    }
}
