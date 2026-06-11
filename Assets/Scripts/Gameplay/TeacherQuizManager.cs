using System;
using TMPro;
using UnityEngine;

public class TeacherQuizManager : MonoBehaviour
{
    [Header("References")]
    public MathQuestionGenerator questionGenerator;

    [Header("UI")]
    public GameObject quizPanel;
    public TMP_Text progressText;
    public TMP_Text questionText;
    public TMP_Text timerText;
    public TMP_InputField answerInput;

    [Header("Quiz Settings")]
    public int questionsPerQuiz = 3;
    public int maxDifficulty = 8;

    [Header("Timer")]
    public float secondsPerQuestion = 120f;
    public bool failQuizWhenTimerRunsOut = true;

    private int currentDifficulty;
    private int currentQuestionNumber;
    private int correctAnswer;

    private float currentQuestionTime;

    private Action onQuizPassed;
    private Action onQuizFailed;

    private bool quizActive;
    private bool quizEnding;

    void Start()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        UpdateTimerText(secondsPerQuestion);
    }

    void Update()
    {
        if (!quizActive)
            return;

        UpdateQuestionTimer();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitAnswer();
        }
    }

    public void StartQuiz(int difficulty, Action passedCallback, Action failedCallback)
    {
        currentDifficulty = Mathf.Clamp(difficulty, 1, maxDifficulty);

        onQuizPassed = passedCallback;
        onQuizFailed = failedCallback;

        currentQuestionNumber = 0;
        quizActive = true;
        quizEnding = false;

        Time.timeScale = 0f;

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        GenerateNextQuestion();
    }

    private void UpdateQuestionTimer()
    {
        currentQuestionTime -= Time.unscaledDeltaTime;
        currentQuestionTime = Mathf.Max(currentQuestionTime, 0f);

        UpdateTimerText(currentQuestionTime);

        if (currentQuestionTime <= 0f && failQuizWhenTimerRunsOut)
        {
            FinishQuiz(false);
        }
    }

    private void GenerateNextQuestion()
    {
        currentQuestionNumber++;
        currentQuestionTime = secondsPerQuestion;
        UpdateTimerText(currentQuestionTime);

        MathQuestionGenerator.MathQuestion question =
            questionGenerator.GenerateQuestion(currentDifficulty);

        questionText.text = question.questionText;
        correctAnswer = question.answer;

        progressText.text =
            $"Question: {currentQuestionNumber} / {questionsPerQuiz}";

        answerInput.text = "";
        answerInput.ActivateInputField();
        answerInput.Select();
    }

    public void SubmitAnswer()
    {
        if (!quizActive)
            return;

        if (!int.TryParse(answerInput.text, out int playerAnswer))
        {
            answerInput.text = "";
            answerInput.ActivateInputField();
            return;
        }

        if (playerAnswer == correctAnswer)
        {
            if (currentQuestionNumber >= questionsPerQuiz)
            {
                FinishQuiz(true);
            }
            else
            {
                GenerateNextQuestion();
            }
        }
        else
        {
            FinishQuiz(false);
        }
    }

    private void FinishQuiz(bool passed)
    {
        if (quizEnding)
            return;

        quizEnding = true;
        quizActive = false;

        Time.timeScale = 1f;

        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }

        if (passed)
        {
            onQuizPassed?.Invoke();
        }
        else
        {
            onQuizFailed?.Invoke();
        }
    }

    private void UpdateTimerText(float timeRemaining)
    {
        if (timerText == null)
            return;

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}