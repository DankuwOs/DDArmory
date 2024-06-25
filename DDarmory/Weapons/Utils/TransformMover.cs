using System;
using UnityEngine;

namespace DDArmory.Weapons.Utils;

public class TransformMover : MonoBehaviour
{
    public Transform swapTf;
    
    public Transform parentTfSlot0;
    
    public Transform parentTfSlot1;

    public event Action<int> IntEvent;

    public void SwapObject(int slot)
    {
        switch (slot)
        {
            case 0:
                swapTf.SetParent(parentTfSlot0, false);
                break;
            
            case 1: 
                swapTf.SetParent(parentTfSlot1, false);
                break;
            
            default:
                Debug.Log($"Slot out of range!");
                return;
        }
        
        if (IntEvent != null)
            IntEvent.Invoke(slot);
    }
}