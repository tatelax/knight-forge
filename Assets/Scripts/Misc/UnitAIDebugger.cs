using Systems;
using UniOrchestrator;
using UnityEngine;

namespace Misc
{
    public class UnitAIDebugger : MonoBehaviour
    {
        private MapSystem _mapSystem;
        private UnitAISystem _unitAISystem;

        private void Update()
        {
            if (_mapSystem is not null && _unitAISystem is not null)
                return;

            if (Orchestrator.Status != BootStatus.Successful)
                return;

            _mapSystem = Orchestrator.GetSystem<MapSystem>();
            _unitAISystem = Orchestrator.GetSystem<UnitAISystem>();
        }

        private void OnDrawGizmos()
        {
            if (_mapSystem == null || _unitAISystem == null || _mapSystem.Units == null)
                return;

            foreach (var unit in _mapSystem.Units)
            {
                // Draw unit position
                Gizmos.color = unit.IsPlayerOwned ? Color.green : Color.red;
                Gizmos.DrawSphere(unit.Visual.transform.position + Vector3.up * 0.5f, 0.15f);

                // Draw target line
                if (unit.Target != null)
                {
                    // Draw a line to the target
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(unit.Visual.transform.position + Vector3.up * 0.5f, unit.Target.Visual.transform.position + Vector3.up * 0.5f);

                    // Draw a sphere on the target
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(unit.Target.Visual.transform.position + Vector3.up * 0.5f, 0.18f);

                    // If mutual targeting, draw a thick magenta line
                    if (unit.Target.Target == unit)
                    {
                        Gizmos.color = Color.magenta;
                        Gizmos.DrawLine(unit.Visual.transform.position + Vector3.up * 0.6f, unit.Target.Visual.transform.position + Vector3.up * 0.6f);
                    }
                }

                // Draw path
                if (unit.CurrentSmoothedPath != null && unit.CurrentSmoothedPath.Count > 1)
                {
                    Gizmos.color = Color.blue;
                    for (int i = 0; i < unit.CurrentSmoothedPath.Count - 1; i++)
                    {
                        Gizmos.DrawLine(unit.CurrentSmoothedPath[i] + Vector3.up * 0.3f, unit.CurrentSmoothedPath[i + 1] + Vector3.up * 0.3f);
                    }
                }
            }
        }
    }
}