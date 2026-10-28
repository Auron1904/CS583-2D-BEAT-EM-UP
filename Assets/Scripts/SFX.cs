using UnityEngine;

public static class SFX
{
    private static AudioSource source;

    public static void Play(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;   // safe to call before sounds exist

        if (source == null)
        {
            GameObject go = new GameObject("SFX");
            Object.DontDestroyOnLoad(go);
            source = go.AddComponent<AudioSource>();
        }
        source.PlayOneShot(clip, volume);
    }
}