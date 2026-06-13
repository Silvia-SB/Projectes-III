using UnityEngine;

public class AudioManagerEnemyZone : MonoBehaviour
{
   [SerializeField] private AudioSource source;
   private static AudioManagerEnemyZone audioManager;

   public static AudioManagerEnemyZone AudioManager
   {
      get
      {
         if (audioManager == null)
         {
            audioManager = FindFirstObjectByType<AudioManagerEnemyZone>();
         }

         return audioManager;
      }
   }

   private void Awake()
   {
      audioManager = this;

      if (source == null)
         source = GetComponent<AudioSource>();

   }

   public static void PreloadClip(AudioClip clip)
   {
      if (clip == null) return;
      if (clip.loadState == AudioDataLoadState.Loaded) return;

      clip.LoadAudioData();
   }

   public void PlayClip(AudioClip clip, float volume = 1f, float pitch = 1f)
   {
      if (source == null || clip == null) return;

      PreloadClip(clip);

      source.volume = volume;
      source.pitch = pitch;
      source.spatialBlend = 0f;
      source.PlayOneShot(clip, volume);
   }
}
