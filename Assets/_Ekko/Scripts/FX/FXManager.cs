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
                break;
            case FXCategory.Sound:
                PlaySound(fx, pos);
                break;
            case FXCategory.Haptic:
                PlayHaptic(fx);
                break;
        }
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

    public void PlayWaveFX(Vector3 pos, float impactForce, float minForce, float maxForce, float targetRadius, Quaternion rot)
    {
        List<FXType> effects = fxLibrary.GetEffects(FXEventType.Wave);
        if (effects == null) return;

        foreach (var fx in effects)
        {
            if (fx is FXWave fxWave)
            {
                Wave wave =  ObjectPoolManager.SpawnObject(fxWave.wavePrefab, pos, Quaternion.identity, ObjectPoolManager.PoolType.Wave);
                wave.Initialize(impactForce, targetRadius, minForce, maxForce);
            }
            else
            {
                PlayEffect(fx, pos, rot);
            }
        }
        
        waveDistortionController.PlayDistortion(impactForce, minForce, maxForce);
    }
}
