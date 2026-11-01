using System;

public enum QuizKind
{
    Pre,
    Post
}

// One quiz question. TopicModuleIndex says which module to review if answered wrongly
// (0 = Module 1 ... 4 = Module 5).
public class QuizQuestion
{
    public string Prompt;
    public string[] Options;
    public int CorrectIndex;
    public int TopicModuleIndex;
}

// The five questions used in both the pre-quiz and the post-quiz.
public static class QuizLibrary
{
    private static readonly QuizQuestion[] questions =
    {
        new QuizQuestion
        {
            Prompt = "Main risk of joining user input into an SQL command?",
            Options = new[] { "Input may alter the query", "Slower screen", "Larger app" },
            CorrectIndex = 0,
            TopicModuleIndex = 1
        },
        new QuizQuestion
        {
            Prompt = "In a parameterized query, ? represents...",
            Options = new[] { "A parameter placeholder", "A password", "A comment" },
            CorrectIndex = 0,
            TopicModuleIndex = 2
        },
        new QuizQuestion
        {
            Prompt = "Which is the primary SQL injection defence?",
            Options = new[] { "Hiding errors", "Parameterized queries", "Longer usernames" },
            CorrectIndex = 1,
            TopicModuleIndex = 2
        },
        new QuizQuestion
        {
            Prompt = "Does input validation replace parameterized queries?",
            Options = new[] { "Yes", "No, it supports them" },
            CorrectIndex = 1,
            TopicModuleIndex = 3
        },
        new QuizQuestion
        {
            Prompt = "Least privilege means the app account has...",
            Options = new[] { "Only needed permissions", "Admin rights", "No password" },
            CorrectIndex = 0,
            TopicModuleIndex = 3
        }
    };

    public static int Count
    {
        get { return questions.Length; }
    }

    public static QuizQuestion GetQuestion(int index)
    {
        int safeIndex = Math.Max(0, Math.Min(index, questions.Length - 1));
        return questions[safeIndex];
    }
}