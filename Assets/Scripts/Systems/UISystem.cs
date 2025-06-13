using System;
using Cysharp.Threading.Tasks;
using UniOrchestrator;
using TMPro;
using Types;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace Systems
{
  public class UISystem: ISystem
  {
    private const string MainMenuAddress = "Assets/Prefabs/UI/MainMenuCanvas.prefab";
    private const string CanvasAddress = "Assets/Prefabs/UI/GameCanvas.prefab";

    private GameCanvas _gameCanvas;
    
    public Slider PowerBar => _gameCanvas.PowerBar;
    public TextMeshProUGUI GemCount => _gameCanvas.GemCount;
    public CharacterButton[] CharacterButtons => _gameCanvas.CharacterButtons;
    public Action<string> OnDragBegin
    {
      get => _gameCanvas.OnDragBegin;
      set => _gameCanvas.OnDragBegin = value;
    }

    public Action<string> OnDragEnd
    {
      get => _gameCanvas.OnDragEnd;
      set => _gameCanvas.OnDragEnd = value;
    }

    public async UniTask Init()
    {
      _ = await Addressables.InstantiateAsync(MainMenuAddress);
      var gameStateSystem = await Orchestrator.GetSystemAsync<GameStateSystem>();
      
      gameStateSystem.OnFinishGame += OnFinishGame;
      
      var canvas = await Addressables.InstantiateAsync(CanvasAddress);

      if (canvas.TryGetComponent<GameCanvas>(out var references))
      {
        _gameCanvas = references;
      }
      else
      {
        throw new NullReferenceException("CanvasReferences not found.");
      }
    }

    private void OnFinishGame(WinType winType) => _gameCanvas.DisplayEndGame(winType);

    public void SetTimerValue(float newValue)
    {
      var timeLeft = Mathf.Max(0, newValue);
      var text = timeLeft.ToString("F2");

      _gameCanvas.RoundTimer.text = text;
    }
  }
}