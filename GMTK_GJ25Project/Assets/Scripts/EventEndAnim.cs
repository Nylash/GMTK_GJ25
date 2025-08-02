using System;
using UnityEngine;

public class EventEndAnim : MonoBehaviour
{
    public event Action OnAnimationEnd;

    public void CallOnAnimationEnd()
    {
        OnAnimationEnd?.Invoke();
    }
}
