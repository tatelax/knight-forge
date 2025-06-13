using System;
using UniOrchestrator;
using Types;
using UnityEngine;

namespace Systems
{
    public class GameStateSystem : ISystem
    {
        public GameState State { get; private set; } = GameState.Idle;

        public Action<GameState> OnStateChanged;
        public Action<WinType> OnFinishGame;
        
        public bool ChangeState(GameState newState)
        {
            State = newState;
            Debug.Log($"GameState is now {State}");
            OnStateChanged?.Invoke(State);
            return true;
        }

        public void FinishGame(WinType winType)
        {
            ChangeState(GameState.Complete);
            OnFinishGame?.Invoke(winType);
        }
    }
}
