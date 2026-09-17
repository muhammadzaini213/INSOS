using System.Collections;
using Slafurry.System.Scene;
using UnityEngine;

namespace Slafurry.System.Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField]
        private MusicData musicData;

        [SerializeField]
        private AudioSource musicSource;

        private Coroutine _currentFadeCoroutine;

        public int Priority => 1;

        void OnDisable()
        {
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.OnSceneLoadCompleted -= OnSceneLoaded;
        }

        public void Subscribe()
        {
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.OnSceneLoadCompleted += OnSceneLoaded;
        }

        private void OnSceneLoaded(string sceneName)
        {
            PlaySceneMusic(sceneName);
        }

        /// <summary>
        /// Play musik berdasarkan scene name (pake mapping dari MusicData).
        /// </summary>
        public void PlaySceneMusic(string sceneName = null)
        {
            if (sceneName == null)
                sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

            string trackName = musicData != null ? musicData.GetTrackName(sceneName) : sceneName;

            Debug.Log($"[MusicPlayer] Scene: '{sceneName}' → Track: '{trackName}'");

            if (musicData != null && musicData.GetClipFromName(trackName) != null)
                PlayMusic(trackName);
            else
                Debug.LogWarning($"[MusicPlayer] No clip for '{trackName}'");
        }

        public void PlayMusic(string trackName, float fadeDuration = 0.5f)
        {
            if (musicData == null || musicSource == null)
                return;

            MusicTrack track = musicData.GetTrack(trackName);
            if (track.clip == null)
                return;

            // Sama dengan yang sedang main — skip
            if (musicSource.clip == track.clip && musicSource.isPlaying)
                return;

            if (_currentFadeCoroutine != null)
                StopCoroutine(_currentFadeCoroutine);

            _currentFadeCoroutine = StartCoroutine(
                AnimateMusicCrossfade(track.clip, track.volume, fadeDuration)
            );
        }

        public void StopMusic(float fadeDuration = 0.5f)
        {
            if (musicSource == null)
                return;

            if (_currentFadeCoroutine != null)
                StopCoroutine(_currentFadeCoroutine);

            if (fadeDuration <= 0f)
            {
                musicSource.Stop();
                musicSource.clip = null;
                musicSource.volume = 0f;
                _currentFadeCoroutine = null;
                return;
            }

            _currentFadeCoroutine = StartCoroutine(FadeOutAndStop(fadeDuration));
        }

        private IEnumerator FadeOutAndStop(float fadeDuration)
        {
            float startVolume = musicSource.volume;
            float percent = 0f;

            while (percent < 1f)
            {
                percent += Time.unscaledDeltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, percent);
                yield return null;
            }

            musicSource.Stop();
            musicSource.clip = null;
            musicSource.volume = startVolume;

            _currentFadeCoroutine = null;
        }

        private IEnumerator AnimateMusicCrossfade(
            AudioClip nextTrack,
            float targetVolume,
            float fadeDuration = 0.5f
        )
        {
            float startVolume = musicSource.volume;
            float percent = 0;
            while (percent < 1)
            {
                percent += Time.unscaledDeltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
                yield return null;
            }

            musicSource.clip = nextTrack;
            musicSource.loop = true;
            musicSource.Play();

            percent = 0;
            while (percent < 1)
            {
                percent += Time.unscaledDeltaTime / fadeDuration;
                musicSource.volume = Mathf.Lerp(0, targetVolume, percent);
                yield return null;
            }

            musicSource.volume = targetVolume;
            _currentFadeCoroutine = null;
        }
    }
}
