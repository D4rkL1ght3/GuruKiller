using TMPro;
using UnityEngine;

public class QuizTester : MonoBehaviour
{
    [Header("References")]
    public MathQuestionGenerator questionGenerator;

    [Header("UI")]
    public TMP_Text progressText;
    public TMP_Text questionText;
    public TMP_InputField answerInput;

    [Header("Quiz Settings")]
    public int questionsPerQuiz = 3;
    public int startingDifficulty = 1;
    public int maxDifficulty = 8;

    private int currentDifficulty;
    private int currentQuestionNumber;
    private int correctAnswer;

    void Start()
    {
        currentDifficulty = startingDifficulty;
        currentQuestionNumber = 0;

        StartNewQuiz();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            SubmitAnswer();
        }
    }

    void StartNewQuiz()
    {
        currentQuestionNumber = 0;

        Debug.Log($"New quiz started! Difficulty Level {currentDifficulty}.");
        GenerateNextQuestion();
    }

    void GenerateNextQuestion()
    {
        currentQuestionNumber++;

        MathQuestionGenerator.MathQuestion question =
            questionGenerator.GenerateQuestion(currentDifficulty);

        questionText.text = question.questionText;
        correctAnswer = question.answer;
        progressText.text = $"Question: {currentQuestionNumber} / {questionsPerQuiz}";

        answerInput.text = "";
        answerInput.ActivateInputField();
    }

    public void SubmitAnswer()
    {
        if (!int.TryParse(answerInput.text, out int playerAnswer))
        {
            Debug.Log("Please enter a valid number.");
            answerInput.text = "";
            answerInput.ActivateInputField();
            return;
        }

        if (playerAnswer == correctAnswer)
        {
            Debug.Log("Correct!");

            if (currentQuestionNumber >= questionsPerQuiz)
            {
                CompleteQuiz();
            }
            else
            {
                GenerateNextQuestion();
            }
        }
        else
        {
            Debug.Log($"Wrong! Correct answer was {correctAnswer}. Quiz restarted.");

            // For playtesting only.
            // In the real game, this will trigger game over.
            StartNewQuiz();
        }
    }

    void CompleteQuiz()
    {
        currentDifficulty++;

        Debug.Log($"Quiz complete! Difficulty increased to Level {currentDifficulty}.");

        if (currentDifficulty > maxDifficulty)
        {
            currentDifficulty = maxDifficulty;
        }

        StartNewQuiz();
    }
}