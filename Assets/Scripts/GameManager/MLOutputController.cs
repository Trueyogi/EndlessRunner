using System;
using System.Collections;
using System.Collections.Generic;
using Mediapipe;
using Mediapipe.Unity;
using UnityEngine;
using Screen = UnityEngine.Screen;

public class MLOutputController : MonoBehaviour
{
    public Hand[] hands;
    public Canvas canvas;
    public GameObject correctPoseSign;
    public LoadoutState loadoutState;
    public CharacterInputController characterInputController;
    [HideInInspector] public static MLOutputController instance;
    [HideInInspector] public bool isStale;
    [HideInInspector] public IList<NormalizedLandmark> currentTarget;

    private Camera mainCamera;
    private Vector3 screenBottomLeft;
    private Vector4 screenTopRight;
    private float canvasDistance;
    private float leftRestrictive;
    private float rightRestrictive;
    private float bottomRestrictive;
    private float upperRestrictive;
    private int PoseCheckerCorrectFrame;
    private int MidY = 0;
    private bool isStarting;
    private int x_pos_index = 1;
    private int y_pos_index = 1;

    public GameObject pauseScreen;
    public bool isUserInScreen;

    private void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        canvasDistance = canvas.planeDistance;
        isUserInScreen = true;
    }

    private void Start()
    {
        /*screenTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, canvas.planeDistance));
        screenBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, canvas.planeDistance));*/
        MLOutputFilterer.Initialize();
    }

    protected virtual void LateUpdate()
    {
        if (currentTarget == null) return;
        if (isStale && MLModeController.instance.Get() == MLModelModes.GamePlay)
        {
            if (correctPoseSign.activeSelf)
            {
                PoseCheckerCorrectFrame = 0;
                isStarting = false;
                correctPoseSign.SetActive(false);
            }
            if (CheckMinPointsInScreen())
            {
                if (!isUserInScreen)
                {
                    GameState.instance.Pause();
                    pauseScreen.SetActive(false);
                    isUserInScreen = true;
                }
                processOutputsForGame();
            }
            else if(!GameState.instance.checkForOutFromScreen)
            {
                if (isUserInScreen)
                {
                    GameState.instance.Pause();
                    pauseScreen.SetActive(true);
                    isUserInScreen = false;
                }
            }
        }
        if (isStale && MLModeController.instance.Get() == MLModelModes.PoseChecker) BeforeGamePoseChecker();
        isStale = true;
    }

    private void processOutputsForGame()
    {
        string horizontal_position = checkHorizontal();
        string vertical_position = checkVertical(MidY);
        Debug.Log("horizontal_position" +horizontal_position);
        Debug.Log("vertical_position" +vertical_position);
        if ((horizontal_position=="Left" && x_pos_index!=0) || (horizontal_position=="Center" && x_pos_index==2))
        {
            //RIGHT MOVE
            characterInputController.ChangeLane(-1);
            x_pos_index -= 1;
        }
        else if ((horizontal_position == "Right" && x_pos_index != 2) || (horizontal_position == "Center" && x_pos_index == 0))
        {
            //LEFT MOVE
            characterInputController.ChangeLane(1);
            x_pos_index += 1;
        }
        
        if (vertical_position == "Jumping" && y_pos_index == 1)
        {
            //UP MOVE
            characterInputController.Jump();
            y_pos_index += 1;
        }
        else if (vertical_position == "Crouching" && y_pos_index == 1)
        {
            //DOWN MOVE
            if(!characterInputController.m_Sliding)
                characterInputController.Slide();
            y_pos_index -= 1;
        }
        else if (vertical_position == "Standing" && y_pos_index != 1)
        {
            y_pos_index = 1;
        }
    }
    private String checkHorizontal()
    {
        int width = 0;
        var left_x = mainCamera.ViewportToWorldPoint(new Vector3(currentTarget[11].X, currentTarget[11].Y, canvasDistance))[0];
        var right_x = mainCamera.ViewportToWorldPoint(new Vector3(currentTarget[12].X, currentTarget[12].Y, canvasDistance))[0];
        string horizontal_position = "";
        
        if (right_x <= width/2 && left_x <= width/2)
            horizontal_position = "Left";
        
        else if (right_x >= width/2 && left_x >= width/2)
            horizontal_position = "Right";
        
        else if (right_x >= width/2 && left_x <= width/2)
            horizontal_position = "Center";
        
        return horizontal_position;
    }
    private String checkVertical(int MidY)
    {
        var left_y = mainCamera.ViewportToWorldPoint(new Vector3(currentTarget[11].X, currentTarget[11].Y, canvasDistance))[1];
        var right_y = mainCamera.ViewportToWorldPoint(new Vector3(currentTarget[12].X, currentTarget[12].Y, canvasDistance))[1];
        double actualMidY = Mathf.Abs((float)(right_y + left_y) / 2);
        string vertical_position = "";

        int lowerBound = MidY - 5;
        int upperBound = MidY + 10;

        if (actualMidY < lowerBound)
            vertical_position = "Jumping";
        else if (actualMidY > upperBound)
            vertical_position = "Crouching";
        else
            vertical_position = "Standing";

        return vertical_position;
    }

    private bool CheckMinPointsInScreen()
    {
        var points = 0;
        for (var i = 11; i < 15; i++)
            if (currentTarget[i].Visibility >= 0.80f)
                points += 1;

        return points == 4;
    } 
    
    private void BeforeGamePoseChecker()
    {
        var points = 0;
        for (var i = 9; i < 25; i++)
            if (currentTarget[i].Visibility >= 0.65f)
                points += 1;

        if (points == 16)
        {
            PoseCheckerCorrectFrame++;

            if (PoseCheckerCorrectFrame >= 20)
            {
                Starting();
                isStarting = true;
            }
        }
        else if (PoseCheckerCorrectFrame > 0)
        {
            PoseCheckerCorrectFrame--;
        }
    }
    
    private void Starting()
    {
        if (isStarting) 
            return;
        
        correctPoseSign.SetActive(true);
        loadoutState.Invoke("StartGamePlay", 1.5f);
        
        var left_y = mainCamera.ViewportToWorldPoint(new Vector3(currentTarget[11].X, currentTarget[11].Y, canvasDistance))[1];
        var right_y = mainCamera.ViewportToWorldPoint(new Vector3(currentTarget[12].X, currentTarget[12].Y, canvasDistance))[1];
        MidY = (int)Mathf.Abs((float)(right_y + left_y) / 3);
    }

    private IEnumerator WaitSecs(float time)
    {
        yield return new WaitForSeconds(time);
    }
}
