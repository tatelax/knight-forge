using System;
using DG.Tweening;
using TMPro;
using Types;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI
{
  public class GameCanvas : MonoBehaviour
  {
    [Header("References")]
    [SerializeField] private Slider powerBar;
    [SerializeField] private TextMeshProUGUI gemCount;
    [SerializeField] private CharacterButton[] characterButtons;
    [SerializeField] private TextMeshProUGUI roundTimer;
    [SerializeField] private RectTransform inGamePanel;
    [SerializeField] private RectTransform endGamePanel;
    [SerializeField] private TextMeshProUGUI enemyPlayerName;
    [SerializeField] private TextMeshProUGUI endGamePlayerNameText;
    [SerializeField] private TextMeshProUGUI endGameEnemyNameText;
    [SerializeField] private GameObject enemyWonPanel;
    [SerializeField] private GameObject playerWonPanel;
    [SerializeField] private GameObject stalematePanel;
    
    public Slider PowerBar => powerBar;
    public TextMeshProUGUI GemCount => gemCount;
    public CharacterButton[] CharacterButtons => characterButtons;
    public TextMeshProUGUI RoundTimer => roundTimer;
    public RectTransform InGamePanel => inGamePanel;
    public RectTransform EndGamePanel => endGamePanel;
    
    public Action<string> OnDragBegin;
    public Action<string> OnDragEnd;
    
    private void Awake()
    {
      EndGamePanel.anchoredPosition = new Vector2(0, EndGamePanel.rect.height);
      
      InGamePanel.gameObject.SetActive(true);
      EndGamePanel.gameObject.SetActive(false);
      playerWonPanel.SetActive(false);
      enemyWonPanel.SetActive(false);
      stalematePanel.SetActive(false);

      enemyPlayerName.text = $"Player{Random.Range(1000,9999)}";
      endGameEnemyNameText.text = enemyPlayerName.text;
      endGamePlayerNameText.text = "Demo Player";
      
      for (ushort i = 0; i < characterButtons.Length; i++)
      {
        var button = characterButtons[i];
        
        button.OnBeginDragAction += () => OnBeginDragAction(button.UnitData.Name);
        button.OnEndDragAction += () => OnEndDragAction(button.UnitData.Name);
      }
    }

    private void OnBeginDragAction(string id) => OnDragBegin?.Invoke(id);
    private void OnEndDragAction(string id) => OnDragEnd?.Invoke(id);

    public void DisplayEndGame(WinType winType)
    {
      EndGamePanel.gameObject.SetActive(true);
      EndGamePanel.DOAnchorPosY(0f, 0.8f);

      switch (winType)
      {
        case WinType.PlayerWon:
          playerWonPanel.SetActive(true);
          break;
        case WinType.EnemyWon:
          enemyWonPanel.SetActive(true);
          break;
        case WinType.Stalemate:
          stalematePanel.SetActive(true);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(winType), winType, null);
      }
    }
  }
}
