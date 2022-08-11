using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer instance;

    public static AudioSource Instance
    {
        get
        {
            return instance.GetComponent<AudioSource>();
        }
    }

    private void Awake()
    {
        Debug.Log("Instance");
        instance = this;
    }
}
