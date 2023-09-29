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
    private float countDownTime = 10f;
    private int selectedOption;
    public Text countDownText;
    public Text questionText;
    public Text[] options;
    public Button[] buttonOptions;
    public static TestManager instance;
    private void Awake()
    {
        instance = this; 
        string jsonPath = Application.dataPath + "/Scripts/Test/questions.json";
        questionList = JsonUtility.FromJson<QuestionList>(File.ReadAllText(jsonPath));
        ResetLearningList();
    }
    
    public void Start(){
        countDownText.gameObject.SetActive(true);
    }
    public IEnumerator ShowNewQuestion()
    {
        int randomQuestionIndex = -1;
        randomQuestionIndex = Random.Range(0, questionList.Questions.Count);
        currentQuestionIndex = randomQuestionIndex;
        Question question = questionList.Questions[currentQuestionIndex];

        questionText.text = question.QuestionText;
        options[0].text = question.Options[0];
        options[1].text = question.Options[1];
        options[2].text = question.Options[2];
        countDownTime = 10f;
        
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
        SelectButtonUI(selectIndex);
        selectedOption = selectIndex;
    }
    public IEnumerator ShowAnswer(int selectIndex)
    {
        Button currentSelectedButton = buttonOptions[selectIndex];
        int correctAnswerIndex = questionList.Questions[currentQuestionIndex].correctAnswerIndex;
        buttonOptions[correctAnswerIndex].GetComponent<Image>().color = Color.green;
        
        if (selectIndex == correctAnswerIndex)
            learningQuestionList.Remove(currentQuestionIndex);
        else
            currentSelectedButton.GetComponent<Image>().color = Color.red;
        
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
                countDownText.gameObject.SetActive(false);
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
        for (int i = 0; i < buttonOptions.Length; i++)
        {
            Button currentSelectedButton = buttonOptions[i];
            Text currentSelectedButtonText = options[i];
            if (i == selectedIndex)
            {
                currentSelectedButton.GetComponent<Image>().color = Color.yellow;
                currentSelectedButtonText.GetComponent<Text>().color = Color.white;
            }
            else
            {
                currentSelectedButton.GetComponent<Image>().color = Color.white;
                currentSelectedButtonText.GetComponent<Text>().color = Color.gray;
            }
        }
    }
}