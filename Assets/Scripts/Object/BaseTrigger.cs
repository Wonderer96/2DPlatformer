using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class BaseTrigger : MonoBehaviour
{
    [Header("Base Settings")]
    [Tooltip("可以触发器的标签列表")]
    public List<string> activatorTags = new List<string>() { "Player" };
    [SerializeField]
    private bool _isActivated;
    public bool IsActivated { get => _isActivated; protected set => _isActivated = value; }

    protected Collider2D triggerCollider;

    protected virtual void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        triggerCollider.isTrigger = true;
    }

    public virtual void ForceDeactivate()
    {
        IsActivated = false;
    }
}

