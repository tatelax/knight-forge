using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
  public class ButtonWithDragEvents : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
  {
    public Action OnBeginDragAction;
    public Action OnDragAction;
    public Action OnEndDragAction;
    
    public void OnBeginDrag(PointerEventData eventData)
    {
      if(IsInteractable())
        OnBeginDragAction?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
      if(IsInteractable())
        OnDragAction?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
      if(IsInteractable())
        OnEndDragAction?.Invoke();
    }
  }
}