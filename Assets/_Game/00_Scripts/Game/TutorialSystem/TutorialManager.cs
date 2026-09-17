using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Slafurry.Game.TutorialSystem
{
    public class TutorialManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private IdleWatcher idleWatcher;

        [Header("Steps (urutan sesuai list, index 0 = step pertama)")]
        [Tooltip(
            "Drag GameObject parent yang punya TutorialDragUI / TutorialClickUI di children-nya"
        )]
        [SerializeField]
        private List<GameObject> steps = new List<GameObject>();

        [Header("Events")]
        [SerializeField]
        private UnityEvent onTutorialCompleted;

        private int _currentIndex = 0;
        private bool _isCompleted;
        private ITutorialStep[] _cachedSteps;

        public bool IsCompleted => _isCompleted;
        public int CurrentStep => _currentIndex;
        public int TotalSteps => steps.Count;

        private void Awake()
        {
            CacheSteps();
        }

        private void CacheSteps()
        {
            _cachedSteps = new ITutorialStep[steps.Count];
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i] == null)
                {
                    Debug.LogWarning($"[TutorialManager] Step [{i}] null di Inspector.");
                    continue;
                }

                _cachedSteps[i] = steps[i].GetComponentInChildren<ITutorialStep>(true);
                if (_cachedSteps[i] == null)
                {
                    Debug.LogWarning(
                        $"[TutorialManager] Step [{i}] '{steps[i].name}' tidak punya ITutorialStep di children."
                    );
                }
            }
        }

        private void OnEnable()
        {
            if (idleWatcher == null)
                return;
            idleWatcher.OnIdle.AddListener(ShowCurrentStep);
            idleWatcher.OnActive.AddListener(StopAllSteps);
        }

        private void OnDisable()
        {
            if (idleWatcher == null)
                return;
            idleWatcher.OnIdle.RemoveListener(ShowCurrentStep);
            idleWatcher.OnActive.RemoveListener(StopAllSteps);
        }

        private void ShowCurrentStep()
        {
            if (_isCompleted)
                return;

            if (_currentIndex < 0 || _currentIndex >= steps.Count)
            {
                Debug.LogWarning(
                    $"[TutorialManager] Index {_currentIndex} out of range (count={steps.Count})."
                );
                return;
            }

            if (steps[_currentIndex] == null)
            {
                Debug.LogWarning($"[TutorialManager] Step [{_currentIndex}] null di Inspector.");
                return;
            }

            var step = _cachedSteps[_currentIndex];
            if (step == null)
            {
                Debug.LogWarning(
                    $"[TutorialManager] Step [{_currentIndex}] '{steps[_currentIndex].name}' tidak punya ITutorialStep."
                );
                return;
            }

            Debug.Log(
                $"[TutorialManager] Play step [{_currentIndex}]: {steps[_currentIndex].name} ({step.GetType().Name})"
            );
            step.Play();
        }

        private void StopAllSteps()
        {
            for (int i = 0; i < _cachedSteps.Length; i++)
            {
                _cachedSteps[i]?.Stop();
            }
        }

        /// <summary>
        /// Panggil ini saat user berhasil menyelesaikan aksi tutorial saat ini.
        /// </summary>
        public void NextStep()
        {
            StopAllSteps();

            if (_currentIndex >= steps.Count - 1)
            {
                Debug.Log("[TutorialManager] Tutorial completed!");
                _isCompleted = true;
                onTutorialCompleted?.Invoke();
                return;
            }

            Debug.Log($"[TutorialManager] NextStep: {_currentIndex} → {_currentIndex + 1}");
            _currentIndex++;
            ShowCurrentStep();
        }

        /// <summary>
        /// Loncat langsung ke step tertentu (misal kalau tutorial di-skip/ulang).
        /// </summary>
        public void SetStep(int index)
        {
            StopAllSteps();
            _currentIndex = Mathf.Clamp(index, 0, steps.Count - 1);
            _isCompleted = false;
            ShowCurrentStep();
        }

        /// <summary>
        /// Reset tutorial dari awal.
        /// </summary>
        public void Restart()
        {
            StopAllSteps();
            _currentIndex = 0;
            _isCompleted = false;
            ShowCurrentStep();
        }
    }
}
