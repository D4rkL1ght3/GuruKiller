using System.Collections.Generic;
using UnityEngine;

public class MathQuestionGenerator : MonoBehaviour
{
    public enum MathOperation
    {
        Add,
        Subtract,
        Multiply,
        Divide
    }

    public struct MathQuestion
    {
        public string questionText;
        public int answer;

        public MathQuestion(string questionText, int answer)
        {
            this.questionText = questionText;
            this.answer = answer;
        }
    }

    private struct MathTerm
    {
        public string text;
        public int value;

        public MathTerm(string text, int value)
        {
            this.text = text;
            this.value = value;
        }
    }

    public MathQuestion GenerateQuestion(int difficultyLevel)
    {
        difficultyLevel = Mathf.Clamp(difficultyLevel, 1, 8);

        switch (difficultyLevel)
        {
            case 1:
                return GenerateExpression(2, new MathOperation[]
                {
                    MathOperation.Add,
                    MathOperation.Subtract
                }, false);

            case 2:
                return GenerateExpression(3, new MathOperation[]
                {
                    MathOperation.Add,
                    MathOperation.Subtract
                }, false);

            case 3:
                return GenerateExpression(2, new MathOperation[]
                {
                    MathOperation.Multiply,
                    MathOperation.Divide
                }, false);

            case 4:
                return GenerateExpression(3, new MathOperation[]
                {
                    MathOperation.Multiply,
                    MathOperation.Divide
                }, false);

            case 5:
                return GenerateExpression(3, new MathOperation[]
                {
                    MathOperation.Add,
                    MathOperation.Subtract,
                    MathOperation.Multiply,
                    MathOperation.Divide
                }, false);

            case 6:
                return GenerateExpression(3, new MathOperation[]
                {
                    MathOperation.Add,
                    MathOperation.Subtract,
                    MathOperation.Multiply,
                    MathOperation.Divide
                }, true);

            case 7:
                return GenerateExpression(4, new MathOperation[]
                {
                    MathOperation.Add,
                    MathOperation.Subtract,
                    MathOperation.Multiply,
                    MathOperation.Divide
                }, false);

            case 8:
                return GenerateExpression(4, new MathOperation[]
                {
                    MathOperation.Add,
                    MathOperation.Subtract,
                    MathOperation.Multiply,
                    MathOperation.Divide
                }, true);

            default:
                return GenerateExpression(2, new MathOperation[]
                {
                    MathOperation.Add,
                    MathOperation.Subtract
                }, false);
        }
    }

    private MathQuestion GenerateExpression(
        int numberCount,
        MathOperation[] allowedOperations,
        bool useBrackets
    )
    {
        if (useBrackets)
        {
            return GenerateBracketExpression(numberCount, allowedOperations);
        }

        return GenerateNormalExpression(numberCount, allowedOperations);
    }

    private MathQuestion GenerateNormalExpression(
        int numberCount,
        MathOperation[] allowedOperations
    )
    {
        List<MathTerm> terms = new List<MathTerm>();
        List<MathOperation> lowPriorityOperations = new List<MathOperation>();

        MathTerm currentTerm = GenerateSingleNumberTerm();
        int usedNumbers = 1;

        while (usedNumbers < numberCount)
        {
            MathOperation operation = GetRandomOperation(allowedOperations);

            if (operation == MathOperation.Multiply)
            {
                int number = Random.Range(2, 13);

                currentTerm.text += $" × {number}";
                currentTerm.value *= number;

                usedNumbers++;
            }
            else if (operation == MathOperation.Divide)
            {
                int divisor = Random.Range(2, 13);

                int result = currentTerm.value;
                int dividend = result * divisor;

                currentTerm.text = $"{dividend} ÷ {divisor}";
                currentTerm.value = result;

                usedNumbers++;
            }
            else
            {
                terms.Add(currentTerm);
                lowPriorityOperations.Add(operation);

                currentTerm = GenerateSingleNumberTerm();
                usedNumbers++;
            }
        }

        terms.Add(currentTerm);

        int answer = terms[0].value;
        string question = terms[0].text;

        for (int i = 1; i < terms.Count; i++)
        {
            MathOperation operation = lowPriorityOperations[i - 1];

            if (operation == MathOperation.Add)
            {
                answer += terms[i].value;
                question += $" + {terms[i].text}";
            }
            else if (operation == MathOperation.Subtract)
            {
                answer -= terms[i].value;
                question += $" - {terms[i].text}";
            }
        }

        return new MathQuestion(question + " = ?", answer);
    }

    private MathQuestion GenerateBracketExpression(
        int numberCount,
        MathOperation[] allowedOperations
    )
    {
        int left = Random.Range(1, 21);
        int right = Random.Range(1, 21);

        bool bracketUsesAddition = Random.value > 0.5f;

        int bracketValue;
        string bracketText;

        if (bracketUsesAddition)
        {
            bracketValue = left + right;
            bracketText = $"({left} + {right})";
        }
        else
        {
            bracketValue = left - right;
            bracketText = $"({left} - {right})";
        }

        MathTerm bracketTerm = new MathTerm(bracketText, bracketValue);

        int remainingNumbers = numberCount - 2;

        int answer = bracketTerm.value;
        string question = bracketTerm.text;

        for (int i = 0; i < remainingNumbers; i++)
        {
            MathOperation operation = GetRandomOperation(allowedOperations);
            int number = Random.Range(2, 13);

            if (operation == MathOperation.Add)
            {
                answer += number;
                question += $" + {number}";
            }
            else if (operation == MathOperation.Subtract)
            {
                answer -= number;
                question += $" - {number}";
            }
            else if (operation == MathOperation.Multiply)
            {
                answer *= number;
                question += $" × {number}";
            }
            else if (operation == MathOperation.Divide)
            {
                int divisor = Random.Range(2, 13);

                int cleanDividend = answer * divisor;

                question = $"({cleanDividend}) ÷ {divisor}";
                answer = cleanDividend / divisor;
            }
        }

        return new MathQuestion(question + " = ?", answer);
    }

    private MathTerm GenerateSingleNumberTerm()
    {
        int number = Random.Range(1, 21);
        return new MathTerm(number.ToString(), number);
    }

    private MathOperation GetRandomOperation(MathOperation[] allowedOperations)
    {
        int index = Random.Range(0, allowedOperations.Length);
        return allowedOperations[index];
    }
}