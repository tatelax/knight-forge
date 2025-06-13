using System;
using Cysharp.Threading.Tasks;
using UniOrchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class UnitAttackSystem : ISystem
  {
    private MapSystem _mapSystem;
    private AudioSystem _audioSystem;
    private UnitAnimationSystem _unitAnimationSystem;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
      _audioSystem = await Orchestrator.GetSystemAsync<AudioSystem>();
      _unitAnimationSystem = await Orchestrator.GetSystemAsync<UnitAnimationSystem>();
    }

    public void Update()
    {
      for (var i = 0; i < _mapSystem.Units.Count; i++)
      {
        var unit = _mapSystem.Units[i];
        
        if (unit?.Target is null || unit.State != UnitState.Attacking)
          continue;

        if (unit.CurrAttackTimer <= 0)
        {
          _mapSystem.AttackTarget(unit);
          unit.CurrAttackTimer = unit.Data.AttackSpeed;
          _audioSystem.Play(Sound.Attack);
          _unitAnimationSystem.PlayAttackAnimation(unit);
        }

        unit.CurrAttackTimer -= Time.deltaTime;
      }
    }
  }
}
