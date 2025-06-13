using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Systems;
using Types;
using UniOrchestrator;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  public class MainMenu : MonoBehaviour
  {
    [Header("Settings")]
    [SerializeField] private float animSpeed = 1f;
    
    [Header("References")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform gate;
    [SerializeField] private RectTransform canvas;
    
    private UniTask startGameTask;
    
    private void Awake()
    {
      startGameButton.onClick.AddListener(StartGamePressed);
    }

    private void StartGamePressed()
    {
      if (Orchestrator.Status != BootStatus.Successful)
        return;
      
      if (startGameTask.Status != UniTaskStatus.Succeeded)
        return;
      
      startGameTask = StartGame();
    }

    private async UniTask StartGame()
    {
      var gameStateSystem = await Orchestrator.GetSystemAsync<GameStateSystem>();

      await UniTask.WaitForSeconds(0.45f);
      
      gate.DOAnchorPosY(canvas.rect.height * -1, animSpeed).SetEase(Ease.OutBounce);

      await UniTask.WaitForSeconds(animSpeed);

      panel.SetActive(false);
      
      gate.DOAnchorPosY(340, animSpeed).SetEase(Ease.InExpo);
      
      await UniTask.WaitForSeconds(animSpeed);

      gameStateSystem.ChangeState(GameState.Playing);
    }
  }
}
