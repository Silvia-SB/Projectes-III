using UnityEngine;

[CreateAssetMenu(fileName = "AudioProfile", menuName = "Scriptable Objects/AudioProfile")]
public class AudioProfile : ScriptableObject
{
    [System.Serializable]
    public struct AudioConfig
    {
        public AudioClip clip;
        [Min(0f)]
        public float startTime;
        [Min(0f)]
        public float endTime;
        
        [Range(0f, 1f)]
        public float volume;
    }
    
    [SerializeField] private AudioConfig normalHit;
    [SerializeField] private AudioConfig fireHit;
    [SerializeField] private AudioConfig electricHit;
    [SerializeField] private AudioConfig fireExplosion;
    [SerializeField] private AudioConfig electricExplosion;
    

    public AudioConfig NormalHit => normalHit;

    public AudioConfig FireHit => fireHit;

    public AudioConfig ElectricHit => electricHit;

    public AudioConfig FireExplosion => fireExplosion;

    public AudioConfig ElectricExplosion => electricExplosion;

}
