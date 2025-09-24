using System.Collections.Generic;
using UnityEngine;

public class ReactiveLandObjectManager : MonoBehaviour
{
	public static ReactiveLandObjectManager Instance { get; private set; }
	private readonly Dictionary<Transform, IReactiveLandObject> cachedReactivePlatforms = new Dictionary<Transform, IReactiveLandObject>();

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;;
	}

	
	public void RegisterReactivePlatform(Transform platform, IReactiveLandObject reactivePlatform)
	{
		cachedReactivePlatforms.Add(platform, reactivePlatform);
	}
	
	public void UnregisterReactivePlatform(Transform platform)
	{
		cachedReactivePlatforms.Remove(platform);
	}
	
	public IReactiveLandObject GetReactivePlatform(Transform platform)
	{
		cachedReactivePlatforms.TryGetValue(platform,  out IReactiveLandObject reactivePlatform);
		return reactivePlatform;
	}

}