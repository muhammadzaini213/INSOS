using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Game.Dialog;
using Slafurry.System.Pause;

namespace Game.UI.HUD
{
    public class DialogHUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI dialogText;
        [SerializeField] private Image spriteImage;
        [SerializeField] private GameObject nextDialogClue;
        [SerializeField] private GameObject dialogUIPrefab;

        [Header("Type Effect")]
        [SerializeField] private float typingSpeed = 0.03f;

        [Header("SFX")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip typeLoopSfx;

        [Header("Options")]
        [SerializeField] private bool allowSkip = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onShow;
        [SerializeField] private UnityEvent onDialogStart;
        [SerializeField] private UnityEvent onTypingComplete;
        [SerializeField] private UnityEvent onDialogComplete;
        [SerializeField] private UnityEvent onHide;
        [SerializeField] private UnityEvent onSkip;

        public event Action OnShow;
        public event Action OnDialogStart;
        public event Action OnTypingComplete;
        public event Action OnDialogComplete;
        public event Action OnHide;
        public event Action OnSkip;

        private DialogBucket currentBucket;
        private int currentIndex;
        private bool isLast;
        private bool isTyping;
        private string currentDialog;
        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (dialogUIPrefab == null)
                dialogUIPrefab = gameObject;
        }

        private void Update()
        {
            if (!allowSkip || !dialogUIPrefab.activeSelf)
                return;
        }

        public void StartDialog(DialogBucket bucket)
        {
            currentBucket = bucket;
            currentIndex = 0;

            Show();
            ShowCurrentDialog();
        }

        public void Show()
        {
            Pause.On("Dialog");
            dialogUIPrefab.SetActive(true);
            OnShow?.Invoke();
            onShow?.Invoke();
        }

        public void Hide()
        {
            Pause.Off("Dialog");
            dialogUIPrefab.SetActive(false);
            OnHide?.Invoke();
            onHide?.Invoke();
        }

        public void NextDialog()
        {
            if (currentBucket == null)
                return;

            if (isTyping)
            {
                if (typingCoroutine != null)
                {
                    StopCoroutine(typingCoroutine);
                    typingCoroutine = null;
                }

                StopTypeSfx();
                dialogText.text = currentDialog;
                isTyping = false;

                OnTypingComplete?.Invoke();
                onTypingComplete?.Invoke();

                if (!isLast)
                    nextDialogClue.SetActive(true);

                return;
            }

            if (isLast)
            {
                OnDialogComplete?.Invoke();
                onDialogComplete?.Invoke();
                isLast = false;
                Hide();
                return;
            }

            currentIndex++;
            ShowCurrentDialog();
        }

        public void SkipDialog()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            StopTypeSfx();
            isTyping = false;
            isLast = false;

            OnSkip?.Invoke();
            onSkip?.Invoke();

            Hide();
        }

        private void ShowCurrentDialog()
        {
            Dialog dialog = currentBucket.dialogs[currentIndex];
            isLast = currentIndex >= currentBucket.dialogs.Length - 1;

            nameText.text = dialog.name;
            currentDialog = dialog.dialog;

            if (spriteImage != null)
            {
                if (dialog.sprite != null)
                {
                    spriteImage.sprite = dialog.sprite;
                    spriteImage.gameObject.SetActive(true);
                }
                else
                {
                    spriteImage.gameObject.SetActive(false);
                }
            }

            nextDialogClue.SetActive(false);

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

            OnDialogStart?.Invoke();
            onDialogStart?.Invoke();

            typingCoroutine = StartCoroutine(TypeDialog());
        }

        private IEnumerator TypeDialog()
        {
            isTyping = true;
            dialogText.text = "";

            PlayTypeSfx();

            foreach (char c in currentDialog)
            {
                dialogText.text += c;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }

            StopTypeSfx();
            dialogText.text = currentDialog;
            isTyping = false;
            typingCoroutine = null;

            OnTypingComplete?.Invoke();
            onTypingComplete?.Invoke();

            if (!isLast)
                nextDialogClue.SetActive(true);
        }

        private void PlayTypeSfx()
        {
            if (audioSource == null || typeLoopSfx == null)
                return;

            audioSource.clip = typeLoopSfx;
            audioSource.loop = true;
            audioSource.Play();
        }

        private void StopTypeSfx()
        {
            if (audioSource != null)
                audioSource.Stop();
        }
    }
}
