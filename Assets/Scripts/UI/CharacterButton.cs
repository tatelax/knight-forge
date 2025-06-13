using System.Globalization;
using ScriptableObjects;
using TMPro;
using UnityEngine;

namespace UI
{
  public class CharacterButton : ButtonWithDragEvents
  {
    [SerializeField] private UnitDataScriptableObject unitData;
    [SerializeField] private TextMeshProUGUI gemCount;
    
    public UnitDataScriptableObject UnitData => unitData;

    protected override void Start()
    {
      base.Start();
      
      gemCount.text = unitData.PowerRequired.ToString(CultureInfo.InvariantCulture);
    }
  }
}
