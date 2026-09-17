using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Networking;

namespace GlobalFootball
{
    // ==========================================
    // 1. GESTIONNAIRE DE MUSIQUE PERSONNELLE
    // ==========================================
    public class CustomMusicManager : MonoBehaviour
    {
        public static CustomMusicManager Instance { get; private set; }

        [Header("Audio Settings")]
        [SerializeField] private AudioSource menuAudioSource;
        public List<string> playlistFilePaths = new List<string>();
        private int currentTrackIndex = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        public void AddTrackToPlaylist(string path)
        {
            if (File.Exists(path) && !playlistFilePaths.Contains(path))
            {
                playlistFilePaths.Add(path);
            }
        }

        public void PlayNextTrack()
        {
            if (playlistFilePaths.Count == 0) return;

            currentTrackIndex = (currentTrackIndex + 1) % playlistFilePaths.Count;
            StartCoroutine(LoadAndPlayAudio(playlistFilePaths[currentTrackIndex]));
        }

        private IEnumerator LoadAndPlayAudio(string filePath)
        {
            string formattedPath = "file://" + filePath;
            AudioType type = GetAudioType(filePath);

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(formattedPath, type))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    menuAudioSource.clip = clip;
                    menuAudioSource.Play();
                }
                else
                {
                    Debug.LogError($"[GLOBAL FOOTBALL] Erreur chargement audio : {www.error}");
                }
            }
        }

        private AudioType GetAudioType(string path)
        {
            if (path.EndsWith(".mp3")) return AudioType.MPEG;
            if (path.EndsWith(".ogg")) return AudioType.OGGVORBIS;
            if (path.EndsWith(".wav")) return AudioType.WAV;
            return AudioType.UNKNOWN;
        }
    }

    // ==========================================
    // 2. MOTEUR DE COMMENTAIRES MULTILINGUES
    // ==========================================
    public class CommentaryEngine : MonoBehaviour
    {
        public enum Language { French, English, Spanish, Arabic, Wolof }
        public Language selectedLanguage = Language.French;

        [System.Serializable]
        public struct VoiceLine
        {
            public string eventID; // ex: "GOAL_LATE", "BIG_SAVE", "FOUL"
            public AudioClip audioClip;
        }

        [Header("Commentary Database")]
        [SerializeField] private AudioSource commentarySource;
        [SerializeField] private List<VoiceLine> frenchLines;
        [SerializeField] private List<VoiceLine> wolofLines;

        public void TriggerMatchEvent(string eventID, int matchMinute, int homeScore, int awayScore)
        {
            string finalEventKey = eventID;
            if (eventID == "GOAL" && matchMinute >= 85 && Mathf.Abs(homeScore - awayScore) <= 1)
            {
                finalEventKey = "GOAL_DRAMATIC_LATE";
            }

            PlayCommentaryClip(finalEventKey);
        }

        private void PlayCommentaryClip(string eventKey)
        {
            List<VoiceLine> activeList = GetActiveLanguageList();
            VoiceLine line = activeList.Find(v => v.eventID == eventKey);

            if (line.audioClip != null && !commentarySource.isPlaying)
            {
                commentarySource.clip = line.audioClip;
                commentarySource.Play();
            }
        }

        private List<VoiceLine> GetActiveLanguageList()
        {
            switch (selectedLanguage)
            {
                case Language.Wolof: return wolofLines;
                default: return frenchLines;
            }
        }
    }

    // ==========================================
    // 3. AFFICHAGE DES CARTES ÉPIQUES
    // ==========================================
    public class PlayerCard : MonoBehaviour
    {
        public enum CardRarity { Base, Highlight, Epic, Legendary }

        [System.Serializable]
        public class PlayerCardData
        {
            public string playerName;
            public int overallRating;
            public string position;
            public CardRarity rarity;
            public Sprite playerPhoto;
            public Color cardThemeColor;
            public int pace;
            public int shooting;
            public int passing;
            public int dribbling;
            public int defending;
            public int physical;
        }

        [Header("UI Components")]
        [SerializeField] private Text nameText;
        [SerializeField] private Text ratingText;
        [SerializeField] private Text positionText;
        [SerializeField] private Image playerImage;
        [SerializeField] private Image cardFrame;
        [SerializeField] private GameObject epicAuraEffect;

        public void DisplayCard(PlayerCardData data)
        {
            nameText.text = data.playerName;
            ratingText.text = data.overallRating.ToString();
            positionText.text = data.position;
            playerImage.sprite = data.playerPhoto;
            cardFrame.color = data.cardThemeColor;

            if (data.rarity == CardRarity.Epic || data.rarity == CardRarity.Legendary)
            {
                epicAuraEffect.SetActive(true);
            }
            else
            {
                epicAuraEffect.SetActive(false);
            }
        }
    }

    // ==========================================
    // 4. RENDU VISUEL ET MÉTÉO
    // ==========================================
    public class WeatherAndLighting : MonoBehaviour
    {
        public enum EnvironmentState { Day, Sunset, Night, Rainy }

        [Header("Light References")]
        [SerializeField] private Light mainDirectionalLight;
        [SerializeField] private GameObject stadiumFloodlights;
        [SerializeField] private ParticleSystem rainParticleSystem;

        [Header("PBR Field Material")]
        [SerializeField] private Material grassMaterial;

        public void SetEnvironment(EnvironmentState state)
        {
            switch (state)
            {
                case EnvironmentState.Day:
                    mainDirectionalLight.color = new Color(1f, 0.95f, 0.85f);
                    mainDirectionalLight.intensity = 1.2f;
                    stadiumFloodlights.SetActive(false);
                    rainParticleSystem.Stop();
                    grassMaterial.SetFloat("_Roughness", 0.6f);
                    break;

                case EnvironmentState.Sunset:
                    mainDirectionalLight.color = new Color(1f, 0.5f, 0.2f);
                    mainDirectionalLight.intensity = 0.9f;
                    stadiumFloodlights.SetActive(true);
                    rainParticleSystem.Stop();
                    break;

                case EnvironmentState.Night:
                    mainDirectionalLight.intensity = 0.1f;
                    stadiumFloodlights.SetActive(true);
                    rainParticleSystem.Stop();
                    break;

                case EnvironmentState.Rainy:
                    mainDirectionalLight.color = new Color(0.6f, 0.65f, 0.7f);
                    mainDirectionalLight.intensity = 0.5f;
                    stadiumFloodlights.SetActive(true);
                    rainParticleSystem.Play();
                    grassMaterial.SetFloat("_Roughness", 0.2f);
                    break;
            }
        }
    }
}
