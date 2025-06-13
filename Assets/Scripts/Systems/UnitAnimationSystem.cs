using System;
using Cysharp.Threading.Tasks;
using UniOrchestrator;
using Types;
using UnityEngine;

namespace Systems
{
  public class UnitAnimationSystem: ISystem
  {
    private static readonly int UnitState = Animator.StringToHash("UnitState");
    private static readonly int Attack = Animator.StringToHash("Attack");

    private MapSystem _mapSystem;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
    }

    public void Update()
    {
      foreach (var unit in _mapSystem.Units)
      {
        if(unit.Data.UnitType != UnitType.Character)
          continue;
        
        unit.Visual.Animator.SetInteger(UnitState, (int)unit.State);

        if (!unit.Visual.DustCloud)
          continue;
        
        switch (unit.State)
        {
          case Types.UnitState.Navigating:
            unit.Visual.DustCloud.Play();
            break;
          case Types.UnitState.Idle:
          case Types.UnitState.Attacking:
          case Types.UnitState.Dead:
          default:
            unit.Visual.DustCloud.Stop();
            break;
        }
      }
    }

    public bool PlayAttackAnimation(Unit unit)
    {
      if(unit.Data.UnitType != UnitType.Character)
        return false;
      
      unit.Visual.Animator.SetTrigger(Attack);
      return true;
    }
  }
}