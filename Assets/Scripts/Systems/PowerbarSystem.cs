using System.Globalization;
using Cysharp.Threading.Tasks;
using Types;
using UniOrchestrator;
using UnityEngine;

namespace Systems
{
  public class PowerbarSystem: ISystem
  {
    private const float FillRate = 0.03f;
    
    private UISystem _uiSystem;
    private GameStateSystem _gameState;

    public async UniTask Init()
    {
      _uiSystem = await Orchestrator.GetSystemAsync<UISystem>();
      _gameState = await Orchestrator.GetSystemAsync<GameStateSystem>();
      
      _uiSystem.PowerBar.value = 0f;
    }

    public void Update()
    {
      if (_gameState.State != GameState.Playing)
        return;
      
      FillPowerbarContinuous();
      UpdateCharacterButtonEnabled();
    }

    private void UpdateCharacterButtonEnabled()
    {
      foreach (var button in _uiSystem.CharacterButtons)
      {
        var barValue = _uiSystem.PowerBar.value * 100f;
        button.interactable = !(barValue < button.UnitData.PowerRequired);
      }
    }

    private void FillPowerbarContinuous()
    {
      _uiSystem.PowerBar.value += Mathf.Min(FillRate * Time.deltaTime, 1f);
      _uiSystem.GemCount.text = Mathf.RoundToInt(_uiSystem.PowerBar.value * 100).ToString(CultureInfo.InvariantCulture);
    }

    public bool UsePower(float amount)
    {
      var adjustedAmount = amount / 100;

      if (_uiSystem.PowerBar.value - adjustedAmount < 0 || _uiSystem.PowerBar.value - adjustedAmount > 1)
      {
        return false;
      }

      _uiSystem.PowerBar.value -= adjustedAmount;
      
      return true;
    }
  }
}