using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TeleportMode { Instant, PathAnimation }

[RequireComponent(typeof(Collider2D))]
public class TeleportGate : MonoBehaviour
{
    [Header("Core Settings")]
    public TeleportMode mode = TeleportMode.Instant;
    public TeleportGate linkedGate;
    public float cooldown = 1f;
    public Vector2 exitDirection = Vector2.right;

    [Header("Path Mode Settings")]
    public Transform[] pathPoints;
    public GameObject effectPrefab;
    public float moveSpeed = 20f;
    public float exitSpeed = 10f;

    private bool isCoolingDown;
    private Coroutine cooldownRoutine;
    private List<GameObject> activeEffects = new List<GameObject>();

    private class ComponentState
    {
        public MonoBehaviour[] scripts;
        public SpriteRenderer renderer;
        public Rigidbody2D rigidbody;
        public bool wasKinematic;
        public bool wasSimulated;
        public Vector2 originalVelocity;
    }
    private Dictionary<GameObject, ComponentState> stateCache = new Dictionary<GameObject, ComponentState>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ShouldIgnoreTrigger(other)) return;
        StartCoroutine(TeleportProcess(other.gameObject));
    }

    private bool ShouldIgnoreTrigger(Collider2D other)
    {
        return isCoolingDown ||
               !other.CompareTag("Player") ||
               linkedGate == null ||
               linkedGate.isCoolingDown;
    }

    private IEnumerator TeleportProcess(GameObject target)
    {
        StartCooldown();
        linkedGate.StartCooldown();
        DisableColliders();

        switch (mode)
        {
            case TeleportMode.Instant:
                InstantTeleport(target);
                break;
            case TeleportMode.PathAnimation:
                yield return StartCoroutine(PathAnimationTeleport(target));
                break;
        }

        yield return new WaitForSeconds(cooldown);
        EndCooldown();
        linkedGate.EndCooldown();
    }

    private void InstantTeleport(GameObject target)
    {
        Vector3 positionOffset = target.transform.position - transform.position;
        target.transform.position = linkedGate.transform.position + positionOffset;
        ApplyExitForce(target.GetComponent<Rigidbody2D>());
    }

    private IEnumerator PathAnimationTeleport(GameObject target)
    {
        ComponentState state = CacheAndDisableComponents(target);
        SpawnEffect(target.transform);

        foreach (Transform point in pathPoints)
        {
            while (Vector3.Distance(target.transform.position, point.position) > 0.1f)
            {
                target.transform.position = Vector3.MoveTowards(
                    target.transform.position,
                    point.position,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }
        }

        target.transform.position = linkedGate.transform.position;
        RestoreComponents(target, state);
        ApplyExitForce(state.rigidbody);
        CleanupEffects();
    }

    private ComponentState CacheAndDisableComponents(GameObject target)
    {
        ComponentState state = new ComponentState();
        state.scripts = target.GetComponents<MonoBehaviour>();
        state.renderer = target.GetComponent<SpriteRenderer>();
        state.rigidbody = target.GetComponent<Rigidbody2D>();

        // ½ûÓÃ×é¼þ
        foreach (var script in state.scripts)
        {
            if (script != null && script != this)
            {
                script.enabled = false;
            }
        }

        if (state.renderer) state.renderer.enabled = false;

        if (state.rigidbody)
        {
            state.wasKinematic = state.rigidbody.isKinematic;
            state.wasSimulated = state.rigidbody.simulated;
            state.originalVelocity = state.rigidbody.velocity;

            state.rigidbody.isKinematic = true;
            state.rigidbody.simulated = false;
            state.rigidbody.velocity = Vector2.zero;
        }

        stateCache[target] = state;
        return state;
    }
    private void RestoreComponents(GameObject target, ComponentState state)
    {
        foreach (var script in state.scripts)
        {
            if (script != null && script != this)
            {
                script.enabled = true;
            }
        }

        if (state.renderer) state.renderer.enabled = true;

        if (state.rigidbody)
        {
            state.rigidbody.isKinematic = state.wasKinematic;
            state.rigidbody.simulated = state.wasSimulated;
            state.rigidbody.velocity = state.originalVelocity;
        }

        stateCache.Remove(target);
    }

    private void SpawnEffect(Transform parent)
    {
        if (!effectPrefab) return;
        GameObject effect = Instantiate(effectPrefab, parent);
        activeEffects.Add(effect);
    }

    private void CleanupEffects()
    {
        foreach (var effect in activeEffects)
        {
            if (effect) Destroy(effect);
        }
        activeEffects.Clear();
    }

    private void ApplyExitForce(Rigidbody2D rb)
    {
        if (!rb) return;
        Vector2 dir = linkedGate.exitDirection.normalized;
        rb.velocity = dir * exitSpeed;
    }

    public void StartCooldown()
    {
        isCoolingDown = true;
        if (cooldownRoutine != null) StopCoroutine(cooldownRoutine);
        cooldownRoutine = StartCoroutine(CooldownTimer());
    }

    private IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(cooldown);
        EndCooldown();
    }

    public void EndCooldown()
    {
        isCoolingDown = false;
        EnableColliders();
    }

    private void DisableColliders()
    {
        GetComponent<Collider2D>().enabled = false;
        if (linkedGate) linkedGate.GetComponent<Collider2D>().enabled = false;
    }

    private void EnableColliders()
    {
        GetComponent<Collider2D>().enabled = true;
        if (linkedGate) linkedGate.GetComponent<Collider2D>().enabled = true;
    }

    private void OnValidate()
    {
        GetComponent<Collider2D>().isTrigger = true;
        exitDirection = exitDirection.normalized;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isCoolingDown ? Color.red : Color.magenta;
        if (linkedGate) Gizmos.DrawLine(transform.position, linkedGate.transform.position);

        if (mode == TeleportMode.PathAnimation)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                if (pathPoints[i] && pathPoints[i + 1])
                {
                    Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var state in stateCache.Values)
        {
            if (state.rigidbody)
            {
                state.rigidbody.isKinematic = state.wasKinematic;
                state.rigidbody.simulated = state.wasSimulated;
            }
        }
    }
}
