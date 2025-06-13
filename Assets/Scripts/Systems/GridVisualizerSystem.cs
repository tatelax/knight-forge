using Cysharp.Threading.Tasks;
using UniOrchestrator;
using Shapes;
using UnityEngine;

namespace Systems
{
  public class GridVisualizerSystem: ISystem
  {
    public GameObject Grid { get; private set; }

    private readonly Color BlockedTileColor = new(0.502f, 0.502f, 0.502f, 0.749f);
    private readonly Color OpenTileColor    = new(0.0f,   0.502f, 0.502f, 0.749f);
    private readonly Color LineColor = new (1f, 1f, 1f, 0.5f);
    private const float LineThickness = 0.03f;
    private const float CornerRadius = 0.3f;
    private readonly Rectangle.RectangleType RectangleType = Rectangle.RectangleType.RoundedSolid;
    
    private MapSystem _mapSystem;
    private PlayerUnitPlacementSystem _placementSystem;
    private Rectangle[,] _tiles;
    private GameObject _lines;
    
    public async UniTask Init()
    {
      _mapSystem = await Orchestrator.GetSystemAsync<MapSystem>();
      _placementSystem = await Orchestrator.GetSystemAsync<PlayerUnitPlacementSystem>();
      
      CreateTileGrid();
    }

    private void CreateTileGrid()
    {
      Grid = new GameObject("Grid Visual");
      _tiles = new Rectangle[MapSystem.SizeX, MapSystem.SizeY];

      for (int x = 0; x < MapSystem.SizeX; x++)
      {
        for (int y = 0; y < MapSystem.SizeY; y++)
        {
          var go = new GameObject($"Tile {x}, {y}");
          go.transform.parent = Grid.transform;
          var rect = go.AddComponent<Rectangle>();
          rect.Color = Color.clear;
          rect.BlendMode = ShapesBlendMode.Transparent;
          rect.Type = RectangleType;
          rect.CornerRadius = CornerRadius;
          _tiles[x, y] = rect;

          go.transform.position = MapSystem.TileToWorldSpace(x, y, 0.01f);
          go.transform.rotation = Quaternion.AngleAxis(90, Vector3.right);
        }
      }

      for (int x = 0; x < MapSystem.SizeX + 1; x++)
      {
        var go = new GameObject($"Line {x}");
        go.transform.parent = Grid.transform;
        var line = go.AddComponent<Line>();
        line.Color = LineColor;
        line.Thickness = LineThickness;
        line.SortingOrder = 10;
        
        var start = MapSystem.TileToWorldSpace(x, 0, 0.01f);
        start.x -= 0.5f;
        start.z -= 0.5f;
        
        var end = MapSystem.TileToWorldSpace(x, MapSystem.SizeY / 2 + 1, 0.01f);
        end.x -= 0.5f;
        end.z -= 0.5f;
        
        line.Start = start;
        line.End = end;
      }
      
      for (int y = 0; y < MapSystem.SizeY / 2 + 2; y++)
      {
        var go = new GameObject($"Line {y}");
        go.transform.parent = Grid.transform;
        var line = go.AddComponent<Line>();
        line.Color = LineColor;
        line.Thickness = LineThickness;
        line.SortingOrder = 10;
        
        var start = MapSystem.TileToWorldSpace(0, y, 0.01f);
        start.x -= 0.5f;
        start.z -= 0.5f;
        
        var end = MapSystem.TileToWorldSpace(MapSystem.SizeX + 1, y, 0.01f);
        end.x -= 0.5f;
        end.z -= 0.5f;
        
        line.Start = start;
        line.End = end;
      }
      
      Grid.SetActive(false);
    }

    public void Update()
    {
      (int x, int y) currTile = _placementSystem.CurrentVisualTile;
      (int x, int y) prevTile = _placementSystem.PreviousVisualTile;
      
      if (_placementSystem.CurrentUnitVisual is null)
      {
        _tiles[prevTile.x, prevTile.y].Color = Color.clear;
        _tiles[currTile.x, currTile.y].Color = Color.clear;
        
        Grid.SetActive(false);
        return;
      }
      
      Grid.SetActive(true);
      
      _tiles[prevTile.x, prevTile.y].Color = Color.clear;
      _tiles[currTile.x, currTile.y].Color = _mapSystem.IsTileOpen(currTile) ? OpenTileColor : BlockedTileColor;
    }
  }
}