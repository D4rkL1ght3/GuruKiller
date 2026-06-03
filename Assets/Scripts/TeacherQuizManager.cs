using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeacherQuizManager : MonoBehaviour
{
    [Header("References")]
    public MathQuestionGenerator questionGenerator;

    [Header("UI")]
    public GameObject quizPanel;
    public TMP_Text progressText;
    public TMP_Text questionText;
    public TMP_Text feedbackText;
    public TMP_InputField answerInput;

    [Header("Quiz Settings")]
    public int questionsPerQuiz = 3;
    public int maxDifficulty = 8;

    private int currentDifficulty;
    private int currentQuestionNumber;
    private int correctAnswer;

    private Action onQuizPassed;
    private Action onQuizFailed;

    private bool quizActive;

    void Start()
    {
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (!quizActive)
            return;

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

        Time.timeScale = 0f;

        if (quizPanel != null)
        {
            quizPanel.SetActive(true);
        }

        if (feedbackText != null)
        {
            feedbackText.text = "";
        }

        GenerateNextQuestion();
    }

    private void GenerateNextQuestion()
    {
        currentQuestionNumber++;

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
            if (feedbackText != null)
            {
                feedbackText.text = "Enter a valid number!";
            }

            answerInput.text = "";
            answerInput.ActivateInputField();
            return;
        }

        if (playerAnswer == correctAnswer)
        {
            if (feedbackText != null)
            {
                feedbackText.text = "Correct!";
            }

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
            if (feedbackText != null)
            {
                feedbackText.text = $"Wrong! Correct answer was {correctAnswer}.";
            }

            FinishQuiz(false);
        }
    }

    private void FinishQuiz(bool passed)
    {
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
}