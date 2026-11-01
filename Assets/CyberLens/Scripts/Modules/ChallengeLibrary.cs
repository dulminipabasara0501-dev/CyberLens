// The Security Challenge: four scenario questions that use ideas from all five modules.
// All SQL here is a safe simulation.
public static class ChallengeLibrary
{
    private const string UnsafeQuery = "query = \"SELECT * FROM users WHERE username = '\" + username + \"'\";";
    private const string SafeQuery = "query = \"SELECT * FROM users WHERE username = ?\";";
    private const string SafeExecute = "execute(query, [username]);";

    private static readonly LessonModule challenge = BuildChallenge();

    public static LessonModule GetChallenge()
    {
        return challenge;
    }

    private static LessonModule BuildChallenge()
    {
        string[] layeredCode = new[]
        {
            "validate(username);",
            SafeQuery,
            SafeExecute,
            "GRANT SELECT ON users TO app_role;",
            "return generic_error();"
        };

        return new LessonModule
        {
            Title = "Security Challenge",
            Activity = "Activity: answer scenario questions and get immediate feedback, then take the final quiz.",
            Outcome = "Select an appropriate SQL injection prevention technique for a simple situation.",
            CodeLines = new[] { UnsafeQuery },
            Steps = new[]
            {
                new LessonStep
                {
                    Subtitle = "A fictional app builds SQL by joining a username into the command.",
                    Caption = "Scenario 1",
                    HighlightLine = 0,
                    TokenWaypoint = 1,
                    UnsafeData = true,
                    Question = new LessonQuestion
                    {
                        Prompt = "What is the main risk?",
                        Options = new[] { "Input may change the query", "The screen may be slow", "The logo may be missing" },
                        CorrectIndex = 0,
                        Explanation = "Joined input can alter the SQL meaning."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Choose the change that separates SQL from the username.",
                    Caption = "Scenario 2",
                    HighlightLine = 1,
                    TokenWaypoint = 2,
                    CodeLines = new[] { SafeQuery, SafeExecute },
                    Question = new LessonQuestion
                    {
                        Prompt = "Which prevention technique is primary?",
                        Options = new[] { "Parameterized query", "Hide all errors only", "Rename the table" },
                        CorrectIndex = 0,
                        Explanation = "Parameterized queries keep input as data."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Validation supports, but does not replace, parameterized queries.",
                    Caption = "Scenario 3",
                    HighlightLine = -1,
                    TokenWaypoint = -1,
                    CodeLines = layeredCode,
                    Question = new LessonQuestion
                    {
                        Prompt = "A developer adds only validation. Fully protected?",
                        Options = new[] { "Yes", "No, parameterized queries are still needed" },
                        CorrectIndex = 1,
                        Explanation = "Validation is an extra layer, not the primary fix."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Add validation and limit database permissions.",
                    Caption = "Scenario 4",
                    HighlightLine = 3,
                    TokenWaypoint = -1,
                    CodeLines = layeredCode,
                    Question = new LessonQuestion
                    {
                        Prompt = "The app only reads users. Which account fits?",
                        Options = new[] { "Read-only access to needed tables", "Administrator access", "No password" },
                        CorrectIndex = 0,
                        Explanation = "Least privilege limits the impact of mistakes."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Lesson complete. Take the post-quiz to view your result.",
                    Caption = "Lesson complete",
                    HighlightLine = -1,
                    TokenWaypoint = -1,
                    CodeLines = new[] { SafeQuery, SafeExecute }
                }
            }
        };
    }
}