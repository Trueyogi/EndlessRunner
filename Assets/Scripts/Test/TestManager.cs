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
    public Text countDownText;
    public Text questionText;
    public Text[] options;
    public Button[] buttonOptions;
    
    private void Awake()
    {
        string jsonPath = Application.dataPath + "/Scripts/Test/questions.json";
        questionList = JsonUtility.FromJson<QuestionList>(File.ReadAllText(jsonPath));
        ResetLearningList();
    }
    
    public void Start(){
        countDownText.gameObject.SetActive(true);
    }
    void Update()
    {
        if(countDownTime>0)
        {
            countDownTime -= Time.unscaledDeltaTime;
            countDownText.text = Mathf.Ceil(countDownTime).ToString();
        }
    }
    public IEnumerator ShowNewQuestion()
    {
        int randomQuestionIndex = -1;
        randomQuestionIndex = Random.Range(0, questionList.Questions.Count);
        /*
        do
        {
            randomQuestionIndex = Random.Range(0, questionList.Questions.Count);
        }
        while (learningQuestionList.Contains(randomQuestionIndex));*/
        currentQuestionIndex = randomQuestionIndex;
        Question question = questionList.Questions[currentQuestionIndex];

        questionText.text = question.QuestionText;
        options[0].text = question.Options[0];
        options[1].text = question.Options[1];
        options[2].text = question.Options[2];
        countDownTime = 10f;   
        yield return new WaitForSeconds(10f);
    }

    public void AnsweredQuestion(int selectIndex)
    {
        Button currentSelectedButton = buttonOptions[selectIndex];
        Text currentSelectedButtonText = options[selectIndex];
        currentSelectedButton.GetComponent<Image>().color = Color.yellow;
        currentSelectedButtonText.GetComponent<Text>().color = Color.white;
        
        StartCoroutine(ShowAnswer(currentSelectedButton, selectIndex));
        // TODO: RESUME
    }
    public IEnumerator ShowAnswer(Button currentSelectedButton, int selectIndex)
    {
        if (selectIndex == questionList.Questions[currentQuestionIndex].correctAnswerIndex)
        {
            learningQuestionList.Remove(currentQuestionIndex);
            currentSelectedButton.GetComponent<Image>().color = Color.green;
        }
        else
            currentSelectedButton.GetComponent<Image>().color = Color.red;
        
        if (learningQuestionList.Count == 0)
            ResetLearningList();
        countDownTime = 5f;
        yield return new WaitForSeconds(5f);
    }
    private void ResetLearningList()
    {
        learningQuestionList = Enumerable.Range(0, 20).ToList();
    }
}