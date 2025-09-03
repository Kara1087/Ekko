using UnityEngine;
using System.Collections.Generic;
using _Ekko.Scripts.FX;

public class FXManager : MonoBehaviour
{
    public static FXManager Instance;

    [SerializeField] private FXLibrary fxLibrary;
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private WaveDistortionController waveDistortionController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        fxLibrary.Init();
    }

    public void PlayEvent(FXEventType eventType, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        List<FXType> effects = fxLibrary.GetEffects(eventType);
        if (effects == null) return;

        foreach (var fx in effects)
        {
            PlayEffect(fx, pos, rot, parent);
        }
    }

    private void PlayEffect(FXType fx, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        switch (fx.category)
        {
            case FXCategory.VFX:
            case FXCategory.Splash:
            case FXCategory.Wave:
                PlayWaveVFX(fx, pos, rot);
                break;
            case FXCategory.Sound:
                PlaySound(fx, pos);
                break;
            case FXCategory.Haptic:
                PlayHaptic(fx);
                break;
        }
    }

    private void PlayWaveVFX(FXType fx, Vector3 pos, Quaternion rot)
    {
        waveDistortionController.PlayDistortion();
    }

    private void PlayVFX(FXType fx, Vector3 pos, Quaternion rot, Transform parent = null)
    {
        GameObject obj = ObjectPoolManager.SpawnObject(
            fx.prefab, pos, rot, ObjectPoolManager.PoolType.ParticleSystem
        );

        if (parent) obj.transform.SetParent(parent);

        if (fx.autoReturnTime > 0)
            StartCoroutine(ReturnAfterTime(obj, fx.autoReturnTime));
    }

    private void PlaySound(FXType fx, Vector3 pos)
    {
        var audioObj = ObjectPoolManager.SpawnObject(
            audioSourcePrefab.gameObject, pos, Quaternion.identity,
            ObjectPoolManager.PoolType.SoundFX
        );

        AudioSource src = audioObj.GetComponent<AudioSource>();
        src.clip = fx.audioClip;
        src.volume = fx.audioVolume;
        src.Play();

        StartCoroutine(ReturnAfterTime(audioObj, fx.audioClip.length));
    }

    private void PlayHaptic(FXType fx)
    {
        // Example: Unity XR input rumble
        // XRController.SendHapticImpulse(intensity, duration);
    }

    private System.Collections.IEnumerator ReturnAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        ObjectPoolManager.ReturnObjectToPool(obj);
    }
}
