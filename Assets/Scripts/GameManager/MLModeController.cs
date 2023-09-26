using System;
using Mediapipe.Unity;
using UnityEngine;

public class MLModeController : MonoBehaviour
{
    public Solution solution;
    public MLModelModes Mode = MLModelModes.None;
    [HideInInspector] public static MLModeController instance;

    private void Start()
    {
        instance = this;
    }

    public MLModelModes Get() { return Mode; }

    public void Set(MLModelModes mLModelMode)
    {
        Mode = mLModelMode;
        if (Mode == MLModelModes.None)
        {
            solution.Stop();
        } else if (Mode == MLModelModes.PoseChecker)
        {
            ImageSourceProvider.ImageSource.SelectSourceWithRes(1280);
            solution.Play();
        } else if (Mode == MLModelModes.GamePlay)
        {
            ImageSourceProvider.ImageSource.ReturnToDefaultRes();
            solution.Play();
        }
    }
}

public enum MLModelModes
{
    None,
    PoseChecker,
    GamePlay
}