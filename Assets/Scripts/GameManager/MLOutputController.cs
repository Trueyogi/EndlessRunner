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
    [HideInInspector] public bool isStale;
    [HideInInspector] public bool canGameStart;
    [HideInInspector] public static MLOutputController instance;
    [HideInInspector] public IList<NormalizedLandmark> currentTarget;

    private Camera mainCamera;
    private Vector3 screenBottomLeft;
    private Vector4 screenTopRight;
    private float width;
    private float height;
    private float lowerBound;
    private float upperBound;
    private float leftRestrictive;
    private float rightRestrictive;
    private float bottomRestrictive;
    private float upperRestrictive;
    private int PoseCheckerCorrectFrame;
    private bool isStarting;
    private int x_pos_index = 1;
    private int y_pos_index = 1;

    public GameObject poseWarning;
    public bool isUserInScreen;

    private void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        isUserInScreen = true;
    }

    private void Start()
    {
        MLOutputFilterer.Initialize();
        canGameStart = true;
        height = mainCamera.orthographicSize * 2.0f;
        width = height * (Screen.width / (float)Screen.height);
    }

    protected virtual void LateUpdate()
    {
        if (currentTarget == null) return;
        if (isStale && MLModeController.instance.Get() == MLModelModes.GamePlay && canGameStart)
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
                    poseWarning.SetActive(false);
                    isUserInScreen = true;
                    GameState.instance.Resume();
                }
                processOutputsForGame();
            }
            else if(!GameState.instance.checkForOutFromScreen)
            {
                if (isUserInScreen)
                {
                    GameState.instance.Pause(false);
                    poseWarning.SetActive(true);
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
        //string vertical_position = checkVertical();
        
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
        /*
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
        }*/
    }

    public int checkAnswer()
    {
        string position = checkHorizontal();
        if (position == "Left") return 0;
        if (position == "Center") return 1;
        if (position == "Right") return 2;
        return -1;
    }
    private String checkHorizontal()
    {
        if (currentTarget.Count < 4) return "";
        var centreX_of_gravity = (currentTarget[11].X  + currentTarget[12].X  + currentTarget[23].X  + currentTarget[24].X )*width /4;
        string horizontal_position = "";
        
        if (centreX_of_gravity <= width/3)
            horizontal_position = "Left";
        else if (centreX_of_gravity <= 2*width/3)
            horizontal_position = "Center";
        else if (centreX_of_gravity <= width)
            horizontal_position = "Right";
        
        return horizontal_position;
    }
    private String checkVertical()
    {
        if (currentTarget.Count < 4) return "";
        var centreY_of_gravity = (currentTarget[11].Y  + currentTarget[12].Y + currentTarget[23].Y  + currentTarget[24].Y )*height /4;
        string vertical_position = "";
 
        if (centreY_of_gravity >= upperBound)
            vertical_position = "Jumping";
        else if (centreY_of_gravity <= lowerBound)
            vertical_position = "Crouching";
        else
            vertical_position = "Standing";

        return vertical_position;
    }

    private bool CheckMinPointsInScreen()
    {
        try
        {
            var visibility_score = currentTarget[11].Visibility + currentTarget[12].Visibility + currentTarget[23].Visibility + currentTarget[24].Visibility;
            return visibility_score >= 2.4f;
        }
        catch(Exception ex)
        {
            return false;
        }
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
        
        lowerBound = (currentTarget[23].Y+currentTarget[24].Y)*height*1.4f/2;
        upperBound = (currentTarget[11].Y+currentTarget[12].Y)*height*0.6f/2;
        correctPoseSign.SetActive(true);
        loadoutState.Invoke("StartGamePlay", 1.5f);
    }

    private IEnumerator WaitSecs(float time)
    {
        yield return new WaitForSeconds(time);
    }
}
