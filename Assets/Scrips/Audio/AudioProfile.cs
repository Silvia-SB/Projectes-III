using UnityEngine;

[CreateAssetMenu(fileName = "AudioProfile", menuName = "Scriptable Objects/AudioProfile")]
public class AudioProfile : ScriptableObject
{
    [SerializeField] private AudioClip normalHit;
    [SerializeField] private AudioClip fireHit;
    [SerializeField] private AudioClip electricHit;
    [SerializeField] private AudioClip fireExplosion;
    [SerializeField] private AudioClip electricExplosion;
    
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    public AudioClip NormalHit => normalHit;

    public AudioClip FireHit => fireHit;

    public AudioClip ElectricHit => electricHit;

    public AudioClip FireExplosion => fireExplosion;

    public AudioClip ElectricExplosion => electricExplosion;

    public float Volume => volume;
}
