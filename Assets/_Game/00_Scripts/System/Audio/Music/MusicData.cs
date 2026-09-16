using System.Collections.Generic;
using Slafurry.Utils.Attributes;
using UnityEngine;

namespace Slafurry.System.Audio
{
    [GameAssetCreator("Audio/Music", "Music Data", order: 1)]
    [CreateAssetMenu(fileName = "New Music Data", menuName = "Audio/Music Data", order = 1)]
    public class MusicData : ScriptableObject
    {
        [Header("Tracks")]
        public MusicTrack[] tracks;

        [Header("Scene Mapping (scene name → track name)")]
        public SceneTrack[] sceneTracks;

        private Dictionary<string, string> _sceneToTrackMap;

        public void BuildMap()
        {
            _sceneToTrackMap = new Dictionary<string, string>();
            foreach (var st in sceneTracks)
            {
                if (!string.IsNullOrEmpty(st.sceneName))
                    _sceneToTrackMap[st.sceneName] = st.trackName;
            }
        }

        public string GetTrackName(string sceneName)
        {
            if (_sceneToTrackMap == null)
                BuildMap();

            // 1. Exact match
            if (_sceneToTrackMap.TryGetValue(sceneName, out string trackName))
                return trackName;

            // 2. Substring match — scene name mengandung key (contoh: "01_Section 1" mengandung "Section 1")
            foreach (var kvp in _sceneToTrackMap)
            {
                if (sceneName.Contains(kvp.Key))
                    return kvp.Value;
            }

            // 3. Fallback: nama scene sendiri
            return sceneName;
        }

        public MusicTrack GetTrack(string trackName)
        {
            foreach (var track in tracks)
            {
                if (track.trackName == trackName)
                    return track;
            }
            return default;
        }

        public AudioClip GetClipFromName(string trackName)
        {
            return GetTrack(trackName).clip;
        }
    }
}
