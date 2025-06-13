using System.Collections.Generic;
using DG.Tweening;
using Types;
using UnityEngine;

namespace Misc
{
  public class AudioPlayer: MonoBehaviour
  {
    [Header("References")]
    [SerializeField] private AudioClip _mainMenuMusic;
    [SerializeField] private AudioClip _gameMusic;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioClip[] _clips;

    public void PlaySFX(Sound sound) => _sfxSource.PlayOneShot(_sounds[sound]);

    private Dictionary<Sound, AudioClip> _sounds;

    private void Awake()
    {
      _sounds = new Dictionary<Sound, AudioClip>();
      
      // Temp hack due to serialized dictionary issue on iOS
      for (var i = 0; i < _clips.Length; i++)
      {
        _sounds.Add((Sound)i, _clips[i]);
      }
    }

    private void Start()
    {
      _musicSource.clip = _mainMenuMusic;
      _musicSource.loop = true;
      _musicSource.Play();
    }

    public void PlayGameMusic()
    {
      _musicSource.DOFade(0, 0.5f).onComplete += () =>
      {
        _musicSource.clip = _gameMusic;
        _musicSource.Play();
        _musicSource.DOFade(1, 0.5f);
      };
    }

    public void PlayEndGameSounds()
    {
      _musicSource.DOFade(0, 0.5f).onComplete += () =>
      {
        _musicSource.Stop();
      };
      
      PlaySFX(Sound.GameOver);
    }
  }
}