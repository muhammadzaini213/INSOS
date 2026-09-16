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
            "Drag GameObject yang punya TutorialDragUI / TutorialClickUI (atau tipe lain yang implement ITutorialStep) ke sini"
        )]
        [SerializeField]
        private List<MonoBehaviour> steps = new List<MonoBehaviour>();

        [Header("Events")]
        [SerializeField]
        private UnityEvent onTutorialCompleted;

        private int _currentIndex = 0;
        private bool _isCompleted;

        public bool IsCompleted => _isCompleted;
        public int CurrentStep => _currentIndex;
        public int TotalSteps => steps.Count;

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
            var step = GetStep(_currentIndex);
            step?.Play();
        }

        private void StopAllSteps()
        {
            foreach (var s in steps)
            {
                (s as ITutorialStep)?.Stop();
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
                _isCompleted = true;
                onTutorialCompleted?.Invoke();
                return;
            }

            _currentIndex++;
            idleWatcher.ResetIdle();
        }

        /// <summary>
        /// Loncat langsung ke step tertentu (misal kalau tutorial di-skip/ulang).
        /// </summary>
        public void SetStep(int index)
        {
            StopAllSteps();
            _currentIndex = Mathf.Clamp(index, 0, steps.Count - 1);
            _isCompleted = false;
            idleWatcher.ResetIdle();
        }

        /// <summary>
        /// Reset tutorial dari awal.
        /// </summary>
        public void Restart()
        {
            StopAllSteps();
            _currentIndex = 0;
            _isCompleted = false;
            idleWatcher.ResetIdle();
        }

        private ITutorialStep GetStep(int index)
        {
            if (index < 0 || index >= steps.Count)
                return null;
            return steps[index] as ITutorialStep;
        }
    }
}
