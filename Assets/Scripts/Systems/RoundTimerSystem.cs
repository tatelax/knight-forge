using Cysharp.Threading.Tasks;
using UniOrchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class RoundTimerSystem: ISystem
  {
    private const float RoundLengthInSeconds = 90f;
    
    private GameStateSystem _gameState;
    private UISystem _uiSystem;

    private float _currentTime;
    
    public async UniTask Init()
    {
      _gameState = await Orchestrator.GetSystemAsync<GameStateSystem>();
      _uiSystem = await Orchestrator.GetSystemAsync<UISystem>();

      _currentTime = RoundLengthInSeconds;
    }

    public void Update()
    {
      if (_gameState.State != GameState.Playing)
        return;

      _currentTime -= Time.deltaTime;

      if (_currentTime <= 0)
      {
        Debug.Log("Round timer has ended.");
        _gameState.FinishGame(WinType.Stalemate);
      }

      _uiSystem.SetTimerValue(_currentTime);
    }
  }
}