using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UniOrchestrator;
using ScriptableObjects;
using Types;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Visuals;
using Random = UnityEngine.Random;

namespace Systems
{
    public class MapSystem : ISystem
    {
        public Action<Unit> OnUnitKilled;
        
        public const int SizeX = 9;
        public const int SizeY = 15;

        public List<Unit> Units { get; private set; }
        public Tile[,] Map { get; } = new Tile[SizeX, SizeY];

        private const string TileBlockObjectAddress = "Assets/Prefabs/Map/TileBlockingRocks.prefab";
        private const float noiseScale = 10f; // Lower = bigger clusters
        private const float detailThreshold = 0.5f; // [0,1]
        private const float detailDensity = 1f;

        private AudioSystem _audioSystem;
        
        public async UniTask Init()
        {
            Debug.Assert(SizeX % 2 != 0 && SizeY % 2 != 0, "Width and height should be odd numbers");
            
            _audioSystem = await Orchestrator.GetSystemAsync<AudioSystem>();
            
            Units = new ();
            await GenerateMap();
        }

        private async UniTask GenerateMap()
        {
            var parent = new GameObject($"Map [{SizeX}, {SizeY}]").transform;

            float noiseOffsetX = Random.Range(0f, 1000f);
            float noiseOffsetY = Random.Range(0f, 1000f);
    
            for (int x = 0; x < SizeX; x++)
            {
                for (int y = 0; y < SizeY; y++)
                {
                    bool isWalkable = true;
                    var pos = new Vector3(x - SizeX / 2, 0, y - SizeY / 2);

                    if (y is > 4 and < SizeY - 4 && x is > 2 and < SizeX - 2)
                    {
                        float nx = x / noiseScale + noiseOffsetX;
                        float ny = y / noiseScale + noiseOffsetY;
                        float noiseValue = Mathf.PerlinNoise(nx, ny);

                        if (noiseValue > detailThreshold * detailDensity)
                        {
                            isWalkable = false;
                        }

                        if (!isWalkable)
                        {
                            var rot = new Vector3(0, Random.Range(0, 360), 0);
                            var quat = Quaternion.Euler(rot);
                            _ = await Addressables.InstantiateAsync(TileBlockObjectAddress, pos, quat, parent);
                        }
                    }

                    Map[x, y] = new Tile(x, y, isWalkable);
                }
            }
        }

#if UNITY_EDITOR
        public void Update()
        {
            for (int x = 0; x < SizeX; x++)
            {
                Debug.DrawLine(TileToWorldSpace((x, 0)), TileToWorldSpace((x, SizeY - 1)), Color.cyan);
            }
            
            for (int y = 0; y < SizeY; y++)
            {
                Debug.DrawLine(TileToWorldSpace((0, y)), TileToWorldSpace((SizeX - 1, y)), Color.cyan);
            }
        }     
#endif

        public List<(int x, int y)> GetTilesCovered((int x, int y) center, int radius)
        {
            var tiles = new List<(int x, int y)>();
            int r2 = radius * radius;
            for (int dx = -radius + 1; dx < radius; dx++)
            {
                for (int dy = -radius + 1; dy < radius; dy++)
                {
                    int tx = center.x + dx;
                    int ty = center.y + dy;
                    if (tx >= 0 && tx < SizeX && ty >= 0 && ty < SizeY)
                    {
                        if (dx * dx + dy * dy < r2)
                            tiles.Add((tx, ty));
                    }
                }
            }
            return tiles;
        }

        public bool PlaceUnit(Unit unit, (int x, int y) pos)
        {
            var newTiles = GetTilesCovered(pos, unit.Data.Radius);
            var oldTiles = GetTilesCovered(unit.CurrTile, unit.Data.Radius);

            foreach (var tile in oldTiles)
            {
                if (Map[tile.x, tile.y].Unit == unit)
                    Map[tile.x, tile.y].Unit = null;
            }

            foreach (var tile in newTiles)
            {
                if (!IsWalkable(tile) || (Map[tile.x, tile.y].Unit != null && Map[tile.x, tile.y].Unit != unit))
                {
                    Debug.LogError($"Failed to place unit {unit.GetHashCode()} at {tile} (radius {unit.Data.Radius})");
                    return false;
                }
            }

            foreach (var tile in newTiles)
                Map[tile.x, tile.y].Unit = unit;

            unit.CurrTile = pos;
            Debug.Log($"placed at {pos}. IsWalkable = {Map[pos.x, pos.y].IsWalkable}");

            return true;
        }

        public Unit CreateUnit(UnitVisual visual, bool isPlayerOwned, UnitDataScriptableObject stats)
        {
            var pos = WorldToTileSpace(visual.transform.position);

            if (!IsTileOpen(pos, stats.Radius))
            {
                Debug.LogError($"Failed to create unit of type {stats.UnitType} at {pos} because tile was occupied");
                return null;
            }

            var newUnit = new Unit(visual, isPlayerOwned, pos, stats);
            Units.Add(newUnit);

            visual.gameObject.name = $"{stats.UnitType} Unit ({newUnit.GetHashCode()})";
            PlaceUnit(newUnit, newUnit.CurrTile);

            return newUnit;
        }

        public void AttackTarget(Unit unit)
        {
            if (unit.Target is null)
            {
                Debug.LogError("Tried to attack target when there was none.");
                return;
            }

            unit.Target.Damage(unit.Data.Strength);

            if (unit.Target.CurrHealth <= 0)
            {
                foreach (var tile in GetTilesCovered(unit.Target.CurrTile, unit.Target.Data.Radius))
                {
                    if (Map[tile.x, tile.y].Unit == unit.Target)
                        Map[tile.x, tile.y].Unit = null;
                }

                _audioSystem.Play(Sound.Die);
                
                unit.Target.Kill();

                Units.Remove(unit.Target);
                
                OnUnitKilled?.Invoke(unit.Target);
            }
        }

        public bool IsTileOpen((int x, int y) pos, int radius = 1, Unit ignoreUnit = null)
        {
            var tiles = GetTilesCovered(pos, radius);
            foreach (var tile in tiles)
            {
                if (tile.x < 0 || tile.x >= SizeX || tile.y < 0 || tile.y >= SizeY)
                    return false;
                if (!IsWalkable(tile))
                    return false;
                if(Map[tile.x, tile.y].Unit is not null && Map[tile.x, tile.y].Unit != ignoreUnit)
                    return false;
            }
            return true;
        }

        public bool IsTileOpen(int x, int y, int radius = 1, Unit ignoreUnit = null)
        {
            return IsTileOpen((x, y), radius, ignoreUnit);
        }

        public void IdleAllUnits()
        {
            for (var i = 0; i < Units.Count; i++)
            {
                Units[i].Reset();
            }
        }

        public static (int, int) WorldToTileSpace(Vector3 world, int maxX = SizeX - 1, int maxY = SizeY - 1)
        {
            int tileX = Mathf.RoundToInt(world.x) + (SizeX - 1) / 2;
            int tileY = Mathf.RoundToInt(world.z) + (SizeY - 1) / 2;
            
            tileX = Mathf.Clamp(tileX, 0, maxX);
            tileY = Mathf.Clamp(tileY, 0, maxY);
            
            return (tileX, tileY);
        }

        public static Vector3 TileToWorldSpace((int x, int y) pos, float worldY = 0f)
        {
            float worldX = pos.x - (SizeX - 1) / 2f;
            float worldZ = pos.y - (SizeY - 1) / 2f;
            return new Vector3(worldX, worldY, worldZ);
        }

        public static Vector3 TileToWorldSpace(int x, int y, float worldY = 0f)
        {
            return TileToWorldSpace((x, y), worldY);
        }

        public static int DistanceBetween_TileSpace_Squared((int x, int y) a, (int x, int y) b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return dx * dx + dy * dy;
        }

        public static int DistanceBetween_TileSpace((int x, int y) a, (int x, int y) b)
        {
            int dx = a.x - b.x;
            int dy = a.y - b.y;
            return Mathf.RoundToInt(Mathf.Sqrt(dx * dx + dy * dy));
        }

        public bool IsWalkable((int x, int y) pos) => Map[pos.x, pos.y].IsWalkable;
    }
}