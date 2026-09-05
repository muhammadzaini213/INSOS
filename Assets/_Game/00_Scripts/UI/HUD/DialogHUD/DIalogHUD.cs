using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using Slafurry.System.Pause;
using Slafurry.System.Player;
using Slafurry.System.Audio;

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
        [SerializeField] private string sfxCategory = "UI";
        [SerializeField] private string typingSfxName = "Typing";

        [Header("Options")]
        [SerializeField] private bool allowSkip = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onShow;
        [SerializeField] private UnityEvent onDialogStart;
        [SerializeField] private UnityEvent onNewLine;
        [SerializeField] private UnityEvent onTypingComplete;
        [SerializeField] private UnityEvent onDialogComplete;
        [SerializeField] private UnityEvent onHide;
        [SerializeField] private UnityEvent onSkip;

        public event Action OnShow;
        public event Action OnDialogStart;
        public event Action OnNewLine;
        public event Action OnTypingComplete;
        public event Action OnDialogComplete;
        public event Action OnHide;
        public event Action OnSkip;

        private DialogBucket currentBucket;
        private int currentIndex;
        private bool isLast;
        private bool isTyping;
        private bool currentFireOnNewLine;
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
            if (currentBucket != null && currentIndex < currentBucket.dialogs.Length)
            {
                Dialog dialog = currentBucket.dialogs[currentIndex];
                if (spriteImage != null)
                {
                    Sprite sprite = dialog.GetSprite(PlayerData.IsBoy);
                    spriteImage.sprite = sprite;
                    spriteImage.gameObject.SetActive(sprite != null);
                }
            }

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
            currentFireOnNewLine = dialog.fireOnNewLine;

            if (spriteImage != null)
            {
                Sprite sprite = dialog.GetSprite(PlayerData.IsBoy);
                if (sprite != null)
                {
                    spriteImage.sprite = sprite;
                    spriteImage.gameObject.SetActive(true);
                }
                else
                {
                    spriteImage.gameObject.SetActive(false);
                }
            }

            nameText.text = dialog.name;
            currentDialog = dialog.dialog;

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

            if (currentFireOnNewLine)
            {
                OnNewLine?.Invoke();
                onNewLine?.Invoke();
            }

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
            Audio.PlaySFX2D(sfxCategory, typingSfxName, true);
        }

        private void StopTypeSfx()
        {
            Audio.StopSFX(sfxCategory, typingSfxName);
        }
    }
}
