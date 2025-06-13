using Systems;
using Types;
using UniOrchestrator;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
  [RequireComponent(typeof(Button))]
  public class ButtonSFX: MonoBehaviour
  {
    private void Awake() => GetComponent<Button>().onClick.AddListener(PlaySound);

    private void PlaySound() => Orchestrator.GetSystem<AudioSystem>().Play(Sound.Click);
  }
}