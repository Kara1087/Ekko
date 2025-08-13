using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPoolManager : MonoBehaviour
{
    [SerializeField] private bool addToDontDestroyOnLoad = false;

    private GameObject emptyHolder;

    private static GameObject particleSystemEmpty;
    private static GameObject gameObjectsEmpty;
    private static GameObject splashEffectsEmpty;
    private static GameObject soundFXEmpty;

    private static Dictionary<GameObject, ObjectPool<GameObject>> objectPools;
    private static Dictionary<GameObject, GameObject> cloneToPrefabMap;

    public enum PoolType
    {
        ParticleSystem,
        GameObjects,
        SplashEffects,
        SoundFX
    }

    public static PoolType poolTypes;

    private void Awake()
    {
        objectPools = new Dictionary<GameObject, ObjectPool<GameObject>>();
        cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

        SetupEmpties();
    }

    private void SetupEmpties()
    {
        emptyHolder = new GameObject("Object Pools");
        
        particleSystemEmpty = new GameObject("Particle System");
        particleSystemEmpty.transform.SetParent(emptyHolder.transform);
        
        gameObjectsEmpty = new GameObject("Game Objects");
        gameObjectsEmpty.transform.SetParent(emptyHolder.transform);
        
        splashEffectsEmpty = new GameObject("Splash Effects");
        splashEffectsEmpty.transform.SetParent(emptyHolder.transform);
        
        soundFXEmpty = new GameObject("Sound FX");
        soundFXEmpty.transform.SetParent(emptyHolder.transform);

        if (addToDontDestroyOnLoad)
        {
            DontDestroyOnLoad(particleSystemEmpty.transform.root);
        }
    }

    private static void CreatePool(GameObject prefab, Vector3 pos, Quaternion rot,
        PoolType poolType = PoolType.GameObjects)
    {
        ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
            createFunc: ()=> CreateObject(prefab, pos, rot, poolType),
            actionOnGet: OnGetObject,
            actionOnRelease: OnReleaseObject,
            actionOnDestroy: OnDestroyObject
        );
        
        objectPools.Add(prefab, pool);
    }
    
    private static GameObject CreateObject(GameObject prefab, Vector3 pos, Quaternion rot,
        PoolType poolType = PoolType.GameObjects)
    {
        prefab.SetActive(false);
        GameObject obj = Instantiate(prefab, pos, rot);
        prefab.SetActive(true);
        
        GameObject parentObject = GetParentObject(poolType);
        obj.transform.SetParent(parentObject.transform);
        
        return obj;
    }
    
    private static void OnGetObject(GameObject obj)
    {
        //optional logic
    }

    private static void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
    }
    
    private static void OnDestroyObject(GameObject obj)
    {
        cloneToPrefabMap.Remove(obj);
    }
    
    private static GameObject GetParentObject(PoolType poolType)
    {
        switch (poolType)
        {
            case PoolType.ParticleSystem:
                return particleSystemEmpty;
            
            case PoolType.GameObjects:
                return gameObjectsEmpty;
            
            case PoolType.SplashEffects:
                return splashEffectsEmpty;
            
            case PoolType.SoundFX:
                return soundFXEmpty;
            
            default:
                return null;
        }
    }
    
    private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 pos, Quaternion rot,
        PoolType poolType = PoolType.GameObjects) where T : Object
    {
        if (!objectPools.ContainsKey(objectToSpawn))
        {
            CreatePool(objectToSpawn, pos, rot, poolType);
        }
        
        GameObject obj = objectPools[objectToSpawn].Get();

        if (obj)
        {
            cloneToPrefabMap.TryAdd(obj, objectToSpawn);
            obj.transform.position = pos;
            obj.transform.rotation = rot;
            obj.SetActive(true);

            if (typeof(T) == typeof(GameObject))
            {
                return obj as T;
            }
            
            T component =  obj.GetComponent<T>();
            if (!component)
            {
                Debug.LogError($"Object {objectToSpawn.name} does not have a component of type {typeof(T)}");
                return null;
            }
            return component;
        }
        return null;
    }

    public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation,
        PoolType poolType = PoolType.GameObjects) where T : Component
    {
        return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
    }

    public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation,
        PoolType poolType = PoolType.GameObjects)
    {
        return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
    }

    public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
    {
        if (cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
        {
            GameObject parentObject = GetParentObject(poolType);

            if (obj.transform.parent != parentObject.transform)
            {
                obj.transform.SetParent(parentObject.transform);
            }

            if (objectPools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
            {
                pool.Release(obj);
            }
        }
        else
        {
            Debug.LogWarning("Trying to return an object that is not pooled: " + obj.name);
        }
    }
    }