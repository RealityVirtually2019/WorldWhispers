using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXController : MonoBehaviour
{

    /// <summary>
    /// The audio layer for discovery
    /// </summary>
    [SerializeField]
    protected AudioSource _audioSource;
    
    [SerializeField]
    protected AudioClip _introClip;

    [SerializeField]
    protected AudioClip _dingClip;

    public void playIntroAudio()
    {
        _audioSource.PlayOneShot(_introClip);
    }

    public void playDing()
    {

        _audioSource.PlayOneShot(_dingClip);
    }
}
