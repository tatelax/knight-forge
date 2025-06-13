using UnityEngine;

namespace Utils
{
  public class LockWorldRotation : MonoBehaviour
  {
    [SerializeField] private Vector3 rot = new(50f, 0f, 0f);
    
    void LateUpdate()
    {
      transform.rotation = Quaternion.Euler(50f, 0f, 0f);;
    }
  }
}