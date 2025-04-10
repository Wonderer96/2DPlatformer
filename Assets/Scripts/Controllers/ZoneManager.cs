using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpawnMode { Once, Repeated }

[System.Serializable]
public class SpawnInfo
{
    public GameObject prefab;
    public Transform spawnPoint;
    public SpawnMode mode;
    public float interval = 1f;
}

public class ZoneManager : MonoBehaviour
{
    [SerializeField] private List<SpawnInfo> spawnInfos = new List<SpawnInfo>();
    private GameManager gameManager;
    private MainCharacterController mainCharacter;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<Coroutine> activeCoroutines = new List<Coroutine>();
    public bool needHandleExit = true;

    private void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            mainCharacter = gameManager.mainCharacter;
        }
        else
        {
            Debug.LogError("GameManager not found in scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (mainCharacter != null && other.gameObject == mainCharacter.gameObject)
        {
            StartSpawning();
            GameManager.Instance.currentZone = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (mainCharacter != null && other.gameObject == mainCharacter.gameObject)
        {
            StopAllSpawning();
            DestroyAllObjects();
            GameManager.Instance.currentZone = null;
        }
        else
        {
            if(needHandleExit)
            { HandleObjectExit(other.gameObject); }
        }

    }

    public void StartSpawning()
    {
        foreach (var info in spawnInfos)
        {
            SpawnObject(info);

            if (info.mode == SpawnMode.Repeated)
            {
                var coroutine = StartCoroutine(RepeatedSpawnRoutine(info));
                activeCoroutines.Add(coroutine);
            }
        }
    }

    public void StopAllSpawning()
    {
        foreach (var coroutine in activeCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
    }

    private void SpawnObject(SpawnInfo info)
    {
        if (info.spawnPoint == null) return;

        GameObject obj = Instantiate(info.prefab, info.spawnPoint.position, Quaternion.identity);
        spawnedObjects.Add(obj); // 使用List记录所有生成的物体
    }

    private IEnumerator RepeatedSpawnRoutine(SpawnInfo info)
    {
        while (true)
        {
            yield return new WaitForSeconds(info.interval);
            SpawnObject(info);
        }
    }

    public void DestroyAllObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
            {
                SafeDestroy(spawnedObjects[i]);
            }
        }
        spawnedObjects.Clear();
    }

    private void HandleObjectExit(GameObject exitedObject)
    {
        if (spawnedObjects.Contains(exitedObject))
        {
            spawnedObjects.Remove(exitedObject);
            SafeDestroy(exitedObject);
        }
    }
    private void SafeDestroy(GameObject obj)
    {
        DetachTaggedChildren(obj.transform, "Player");
        Destroy(obj);
    }
    private void DetachTaggedChildren(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                child.SetParent(null); // 解除父子关系
            }
            // 递归检查下一层子物体
            DetachTaggedChildren(child, tag);
        }
    }
}