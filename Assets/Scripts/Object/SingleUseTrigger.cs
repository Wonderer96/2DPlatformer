using UnityEngine;
using System.Collections.Generic;
public class SingleUseTrigger : BaseTrigger
{
    [Header("Flip Settings")]
    [Tooltip("�Ƿ�������������")]
    public bool allowContinuousFlip = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activatorTags.Contains(other.tag))
        {
           // if (allowContinuousFlip || !IsActivated)
            {
                IsActivated = !IsActivated;
            }
        }
    }
}