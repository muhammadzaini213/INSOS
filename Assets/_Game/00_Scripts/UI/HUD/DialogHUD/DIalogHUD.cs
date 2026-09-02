using System.Collections;
using UnityEngine;
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

        private DialogPane dialogPane;

        private bool isLast;
        private bool isTyping;

        private string currentDialog;
        private Coroutine typingCoroutine;

        private void Awake()
        {
            if (dialogUIPrefab == null)
                dialogUIPrefab = gameObject;

            dialogPane = GetComponent<DialogPane>();
        }

        private void Update()
        {
            if (!allowSkip)
                return;

            if (!dialogUIPrefab.activeSelf)
                return;

            // if (Input.GetKeyDown(KeyCode.Space))
            // {
            //     SkipDialog();
            // }
        }

        public void Show()
        {
            Pause.On("Dialog");

            dialogUIPrefab.SetActive(true);

            onShow?.Invoke();
        }

        public void Hide()
        {
            Pause.Off("Dialog");

            dialogUIPrefab.SetActive(false);

            onHide?.Invoke();
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

            onSkip?.Invoke();

            Hide();
        }

        public void SetDialog(
            string characterName,
            string dialog,
            bool isLast)
        {
            nameText.text = characterName;

            currentDialog = dialog;
            this.isLast = isLast;

            nextDialogClue.SetActive(false);

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }

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

            // Typing selesai untuk dialog ini.
            onTypingComplete?.Invoke();

            // Hanya tampilkan clue kalau masih ada dialog berikutnya.
            if (!isLast)
            {
                nextDialogClue.SetActive(true);
            }

            // Kalau isLast, JANGAN invoke onDialogComplete di sini.
            // Player harus menekan NextDialog() sekali lagi.
        }

        public void NextDialog()
        {
            // =====================================================
            // 1. MASIH TYPING
            //    → Skip typing, tapi BELUM lanjut dialog.
            // =====================================================
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

                onTypingComplete?.Invoke();

                if (!isLast)
                {
                    nextDialogClue.SetActive(true);
                }

                return;
            }

            // =====================================================
            // 2. SUDAH SELESAI TYPING + DIALOG TERAKHIR
            //    → Input berikutnya baru menyelesaikan dialog.
            // =====================================================
            if (isLast)
            {
                onDialogComplete?.Invoke();

                isLast = false;

                Hide();

                return;
            }

            // =====================================================
            // 3. SUDAH SELESAI TYPING + MASIH ADA DIALOG
            //    → Lanjut ke dialog berikutnya.
            // =====================================================
            dialogPane.NextDialog();
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
            if (audioSource == null)
                return;

            audioSource.Stop();
        }
    }
}