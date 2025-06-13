using Cysharp.Threading.Tasks;
using Misc;
using UniOrchestrator;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
  public class AudioSystem: ISystem
  {
    private const string AudioSourceAddress = "Assets/Prefabs/AudioPlayer.prefab";
    
    private AudioPlayer _audioPlayer;
    
    public async UniTask Init()
    {
      var obj = await Addressables.InstantiateAsync(AudioSourceAddress, Camera.main.gameObject.transform.position, Quaternion.identity);
      _audioPlayer = obj.GetComponent<AudioPlayer>();

      var gameState = await Orchestrator.GetSystemAsync<GameStateSystem>();
      gameState.OnStateChanged += OnStateChanged;
    }

    private void OnStateChanged(GameState state)
    {
      if(state == GameState.Playing)
        _audioPlayer.PlayGameMusic();
      else if (state == GameState.Complete)
        _audioPlayer.PlayEndGameSounds();
    }

    public void Play(Sound sound) => _audioPlayer.PlaySFX(sound);
  }
}