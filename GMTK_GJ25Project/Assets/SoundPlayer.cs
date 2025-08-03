using System.Collections.Generic;
using UnityEngine;

public class SoundPlayer : MonoBehaviour
{
    [SerializeField] private List<AudioClip> _sounds;
    [SerializeField][Range(0f, 2f)] private float _soundVolume;

    public void PlaySound()
    {
        AudioSource source = UIManager.Instance.SpawnAudioSource();
        source.PlayOneShot(_sounds[Random.Range(0, _sounds.Count)], _soundVolume);
    }
}
