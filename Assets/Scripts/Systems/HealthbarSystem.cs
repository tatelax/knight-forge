using Cysharp.Threading.Tasks;
using UniOrchestrator;

namespace Systems
{
  public class HealthbarSystem: ISystem
  {
    private MapSystem _mapSystem;

    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
    }

    public void Update()
    {
      foreach (var unit in _mapSystem.Units)
      {
        if (unit.Visual?.HealthBar is null)
          continue;

        if (unit.CurrHealth >= 100f)
        {
          unit.Visual.HealthBar.gameObject.SetActive(false);
          continue;
        }

        unit.Visual.HealthBar.gameObject.SetActive(true);

        unit.Visual.HealthBar.value = unit.CurrHealth > 0 ? unit.CurrHealth / 100 : 0;
      }
    }
  }
}