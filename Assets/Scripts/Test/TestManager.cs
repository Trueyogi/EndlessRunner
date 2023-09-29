using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Test;

public class TestManager : MonoBehaviour
{
    [HideInInspector]
    private QuestionList questionList;
    private List<int> learningQuestionList = new List<int>();
    private int currentQuestionIndex = -1;
    private int selectedOption=-1;
    private float countDownTime;
    public GameObject QuestionMenu;
    public TextAsset jsonFile;
    public Text countDownText;
    public Text questionText;
    public Text[] options;
    public Button[] buttonOptions;
    public static TestManager instance;

    private void Awake()
    {
        instance = this; 
    }

    private void OnEnable()
    {
        questionList = JsonUtility.FromJson<QuestionList>(jsonFile.text);
        ResetLearningList();
    }
    public IEnumerator ShowNewQuestion()
    {
        countDownTime = 10f;
        int randomQuestionIndex = -1;
        Debug.Log(questionList.Questions.Count);
        randomQuestionIndex = Random.Range(0, questionList.Questions.Count);
        currentQuestionIndex = randomQuestionIndex;
        Question question = questionList.Questions[currentQuestionIndex];

        questionText.text = question.QuestionText;
        options[0].text = question.Options[0];
        options[1].text = question.Options[1];
        options[2].text = question.Options[2];
        QuestionMenu.gameObject.SetActive(true);
        
        while (true)
        {
            if(countDownTime>0)
            {
                countDownTime -= Time.unscaledDeltaTime;
                countDownText.text = Mathf.Ceil(countDownTime).ToString();
            }
            else
            {
                StartCoroutine(ShowAnswer(selectedOption));
                break;
            }
            yield return null;
        }
    }

    public void AnsweredQuestion(int selectIndex)
    {
        if (selectedOption != selectIndex)
        {
            SelectButtonUI(selectIndex);
            DeselectButtonUI(selectedOption);
            selectedOption = selectIndex;
        }
    }
    public IEnumerator ShowAnswer(int selectIndex)
    {
        int correctAnswerIndex = questionList.Questions[currentQuestionIndex].correctAnswerIndex;
        buttonOptions[correctAnswerIndex].GetComponent<Image>().color = Color.green;

        if (selectIndex != -1)
        {
            Button currentSelectedButton = buttonOptions[selectIndex];
            if (selectIndex == correctAnswerIndex)
                learningQuestionList.Remove(currentQuestionIndex);
            else
                currentSelectedButton.GetComponent<Image>().color = Color.red;
        }
        
        if (learningQuestionList.Count == 0)
            ResetLearningList();
        GameState.instance.canAnswering = false;
        countDownTime = 5f;
        while (true)
        {
            if(countDownTime>0)
            {
                countDownTime -= Time.unscaledDeltaTime;
                countDownText.text = Mathf.Ceil(countDownTime).ToString();
            }
            else
            {
                DeselectAllButtonUI();
                QuestionMenu.gameObject.SetActive(false);
                GameState.instance.questionMenu.gameObject.SetActive(false);
                GameState.instance.Resume();
                break;
            }
            yield return null;
        }
    }
    private void ResetLearningList()
    {
        learningQuestionList = Enumerable.Range(0, 20).ToList();
    }

    private void SelectButtonUI(int selectedIndex)
    {
        Button currentSelectedButton = buttonOptions[selectedIndex];
        Text currentSelectedButtonText = options[selectedIndex];
        currentSelectedButton.GetComponent<Image>().color = Color.yellow;
        currentSelectedButtonText.GetComponent<Text>().color = Color.white;
    }

    private void DeselectButtonUI(int selectedIndex)
    {
        if (selectedIndex != -1)
        {
            Button preSelectedButton = buttonOptions[selectedIndex];
            Text preSelectedButtonText = options[selectedIndex];
            preSelectedButton.GetComponent<Image>().color = Color.white;
            preSelectedButtonText.GetComponent<Text>().color = Color.black;
        }
    }

    private void DeselectAllButtonUI()
    {
        for (int i = 0; i < buttonOptions.Length; i++)
            DeselectButtonUI(i);
    }
}