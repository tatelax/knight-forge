using Cysharp.Threading.Tasks;
using Helpers;
using UniOrchestrator;
using Types;
using UnityEngine;

namespace Systems
{
    public class UnitAISystem : ISystem
    {
        private const float RotateSpeed = 10.0f;
        
        private MapSystem _mapSystem;
        private GameStateSystem _gameStateSystem;

        public async UniTask Init()
        {
            _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
            _gameStateSystem = await Orchestrator.GetSystemAsync<GameStateSystem>();
            
            _gameStateSystem.OnStateChanged += OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state != GameState.Complete)
                return;

            _mapSystem.IdleAllUnits();
        }

        public void Update()
        {
            if (_mapSystem.Units == null || _mapSystem.Units.Count == 0 || _gameStateSystem.State != GameState.Playing)
                return;

            foreach (Unit unit in _mapSystem.Units)
            {
                if (unit.Data.UnitType != UnitType.Character || unit.State == UnitState.Dead)
                    continue;

                var target = FindClosestTarget(unit);
                if (target is null)
                {
                    unit.SetState(UnitState.Idle);
                    continue;
                }

                if (target != unit.Target)
                {
                    unit.Target = target;
                    unit.CurrentSmoothedPath = null;
                    unit.CurrentPathIndex = 0;
                }

                var visualTile = MapSystem.WorldToTileSpace(unit.Visual.transform.position);
                if (visualTile != unit.CurrTile || unit.CurrentSmoothedPath is null)
                {
                    if (visualTile != unit.CurrTile && !_mapSystem.PlaceUnit(unit, visualTile))
                    {
                        unit.Visual.transform.position = MapSystem.TileToWorldSpace(unit.CurrTile);
                    }

                    int distanceToTarget = MapSystem.DistanceBetween_TileSpace(unit.CurrTile, unit.Target.CurrTile);
                    if (distanceToTarget <= 1)
                    {
                        FaceTargetAndAttack(unit);
                        continue;
                    }

                    int pathLen = FastPathfinder.FindPath(_mapSystem, unit, unit.CurrTile, unit.Target.CurrTile, unit.CurrentPathBuffer);
                    if (pathLen == 0)
                    {
                        unit.SetState(UnitState.Idle);
                        continue;
                    }

                    var pathWorldSpace = new Vector3[pathLen];
                    for (int i = 0; i < pathLen; i++)
                        pathWorldSpace[i] = MapSystem.TileToWorldSpace(unit.CurrentPathBuffer[i]);

                    unit.CurrentSmoothedPath = PathSmoothingUtil.CatmullRomSpline(pathWorldSpace, 5);
                    unit.CurrentPathIndex = 0;
                }

                // After pathfinding, check again for attack range
                int currDist = MapSystem.DistanceBetween_TileSpace(unit.CurrTile, unit.Target.CurrTile);
                if (currDist <= 1)
                {
                    FaceTargetAndAttack(unit);
                    continue;
                }

                // Block movement if next tile is occupied by another unit
                if (unit.CurrentSmoothedPath != null && unit.CurrentSmoothedPath.Count > 1 && unit.CurrentPathIndex + 1 < unit.CurrentSmoothedPath.Count)
                {
                    Vector3 nextPoint = unit.CurrentSmoothedPath[unit.CurrentPathIndex + 1];
                    var nextTile = MapSystem.WorldToTileSpace(nextPoint);

                    bool blocked = false;
                    foreach (var other in _mapSystem.Units)
                    {
                        if (other == unit || other.State == UnitState.Dead)
                            continue;
                        if (other.CurrTile == nextTile)
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (blocked)
                    {
                        unit.SetState(UnitState.Idle);
                        continue;
                    }
                }

                if (unit.CurrentSmoothedPath.Count == 0 || unit.CurrentPathIndex + 1 >= unit.CurrentSmoothedPath.Count - 1)
                {
                    FaceTargetAndAttack(unit, MapSystem.WorldToTileSpace(unit.CurrentSmoothedPath[unit.CurrentPathIndex]));
                    continue;
                }

                AdvanceAlongPath(unit);
            }
        }

        private Unit FindClosestTarget(Unit unit)
        {
            Unit closest = null;
            int shortestDist = int.MaxValue;

            foreach (var other in _mapSystem.Units)
            {
                if (unit == other || unit.IsPlayerOwned == other.IsPlayerOwned)
                    continue;

                int dist = MapSystem.DistanceBetween_TileSpace(unit.CurrTile, other.CurrTile);
                if (dist < shortestDist)
                {
                    shortestDist = dist;
                    closest = other;
                }
            }

            return closest;
        }

        private void FaceTargetAndAttack(Unit unit, (int x, int y)? overrideTile = null)
        {
            unit.SetState(UnitState.Attacking);

            Vector3 worldPos = overrideTile.HasValue 
                ? MapSystem.TileToWorldSpace(overrideTile.Value) 
                : MapSystem.TileToWorldSpace(unit.CurrTile);

            Vector3 direction = unit.Target.Visual.transform.position - unit.Visual.transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                Quaternion smoothedRot = Quaternion.Slerp(unit.Visual.transform.rotation, targetRot, Time.deltaTime * RotateSpeed);

                unit.Visual.transform.SetPositionAndRotation(worldPos, smoothedRot);
            }
            else
            {
                unit.Visual.transform.position = worldPos;
            }
        }

        private void AdvanceAlongPath(Unit unit)
        {
            var path = unit.CurrentSmoothedPath;
            int nextIndex = unit.CurrentPathIndex + 1;
            Vector3 nextPoint = path[nextIndex];

            if (Vector3.Distance(unit.Visual.transform.position, nextPoint) < 0.001f)
                unit.CurrentPathIndex++;

            Vector3 direction = nextPoint - unit.Visual.transform.position;
            
            Vector3 newPos = Vector3.MoveTowards(unit.Visual.transform.position, nextPoint, Time.deltaTime * unit.Data.MoveSpeed);

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion newRot = Quaternion.LookRotation(direction);
                Quaternion smoothedRot = Quaternion.Slerp(unit.Visual.transform.rotation, newRot, Time.deltaTime * RotateSpeed);

                unit.Visual.transform.SetPositionAndRotation(newPos, smoothedRot);
            }
            else
            {
                unit.Visual.transform.position = newPos;
            }

            unit.SetState(UnitState.Navigating);

            for (int i = 0; i < path.Count - 1; i++)
                Debug.DrawLine(path[i], path[i + 1], Color.magenta);
        }
    }
}