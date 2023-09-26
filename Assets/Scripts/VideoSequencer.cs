using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSequencer : MonoBehaviour
{
    public Image Image;
    public float FPS = 30f;
    public bool PlayOnAwake = false;
    public bool Loop = false;
    public List<Sprite> Frames;

    int FrameCount = 0;
    int index = 0;
    void OnEnable()
    {
        FrameCount = Frames.Count;
        index = 0;
        if (PlayOnAwake)
        {
            Play();
        }
    }
    
    public void Play()
    {
        if (index < FrameCount)
        {
            Image.sprite = Frames[index];
            index++;
            Invoke("Play", 1f / FPS);
        }
        else if(Loop)
        {
            index = 0;
            Invoke("Play", 1f / FPS);
        }
        else
        {
            index = 0;
        }
    }


}
