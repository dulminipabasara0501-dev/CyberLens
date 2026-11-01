using System;

// One multiple-choice question inside a lesson step.
public class LessonQuestion
{
    public string Prompt;
    public string[] Options;
    public int CorrectIndex;
    public string Explanation;
    public bool OptionsAreCode; // true = options are code, shown in a smaller font
}

// One step of a module.
public class LessonStep
{
    public string Subtitle;
    public string Caption;
    public int HighlightLine = -1;   // which code line glows cyan (-1 = none)
    public int TokenWaypoint = -1;   // 0 Computer, 1 Server, 2 Database, -1 = token hidden
    public bool UnsafeData;          // true = token is shown as unsafe
    public string[] CodeLines;       // null = use the module's own code lines
    public LessonQuestion Question;  // null = no question on this step
}

// One module.
public class LessonModule
{
    public string Title;
    public string Activity;
    public string Outcome;
    public string[] CodeLines;
    public LessonStep[] Steps;
}

// All learning content for the five modules. All SQL here is a safe simulation.
public static class ModuleLibrary
{
    private const string UnsafeQuery = "query = \"SELECT * FROM users WHERE username = '\" + username + \"'\";";
    private const string SafeQuery = "query = \"SELECT * FROM users WHERE username = ?\";";
    private const string SafeExecute = "execute(query, [username]);";

    private static readonly LessonModule[] modules = BuildModules();

    public static int Count
    {
        get { return modules.Length; }
    }

    public static LessonModule GetModule(int index)
    {
        int safeIndex = Math.Max(0, Math.Min(index, modules.Length - 1));
        return modules[safeIndex];
    }

    private static LessonModule[] BuildModules()
    {
        return new[]
        {
            BuildModule1(),
            BuildModule2(),
            BuildModule3(),
            BuildModule4(),
            BuildModule5()
        };
    }

    private static LessonModule BuildModule1()
    {
        return new LessonModule
        {
            Title = "Meet the Database",
            Activity = "Activity: tap each object to reveal its role.",
            Outcome = "Identify the three main components and explain the path of a login request.",
            CodeLines = new[] { "// login request", "handle(request);", "db.query(sql);" },
            Steps = new[]
            {
                new LessonStep
                {
                    Subtitle = "A secure application receives a request, checks it, and then talks to a database.",
                    Caption = "Login request starts on the computer",
                    HighlightLine = -1,
                    TokenWaypoint = 0
                },
                new LessonStep
                {
                    Subtitle = "The interface, server and database have different jobs.",
                    Caption = "Request reaches the server",
                    HighlightLine = 1,
                    TokenWaypoint = 1,
                    Question = new LessonQuestion
                    {
                        Prompt = "Which component checks and handles the request?",
                        Options = new[] { "Computer screen", "Application server", "Database" },
                        CorrectIndex = 1,
                        Explanation = "The application server checks the request and builds database calls."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Risk appears when input becomes part of a command.",
                    Caption = "Server queries the database",
                    HighlightLine = 2,
                    TokenWaypoint = 2,
                    Question = new LessonQuestion
                    {
                        Prompt = "Where might unsafe input become risky?",
                        Options = new[] { "In the database call", "In the logo", "In the subtitles" },
                        CorrectIndex = 0,
                        Explanation = "Input joined into a database command can change its meaning."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Tap each object to learn its role.",
                    Caption = "Tap each 3D object",
                    HighlightLine = -1,
                    TokenWaypoint = -1
                }
            }
        };
    }

    private static LessonModule BuildModule2()
    {
        return new LessonModule
        {
            Title = "How SQL Injection Happens",
            Activity = "Activity: step through the request and watch the simulated database response change.",
            Outcome = "Explain how unsafe query construction can change a query's meaning.",
            CodeLines = new[] { UnsafeQuery, "run(query);" },
            Steps = new[]
            {
                new LessonStep
                {
                    Subtitle = "SQL asks a database for information.",
                    Caption = "Normal input: alice",
                    HighlightLine = 0,
                    TokenWaypoint = 0
                },
                new LessonStep
                {
                    Subtitle = "Injection risk: untrusted input is joined directly into a query.",
                    Caption = "SQL text + input = one string",
                    HighlightLine = 0,
                    TokenWaypoint = 1,
                    UnsafeData = true
                },
                new LessonStep
                {
                    Subtitle = "Input may change the query instead of staying data.",
                    Caption = "Simulated input changes the query",
                    HighlightLine = 0,
                    TokenWaypoint = 2,
                    UnsafeData = true,
                    Question = new LessonQuestion
                    {
                        Prompt = "Which operation creates the risk?",
                        Options = new[] { "Concatenating input into SQL", "Drawing the login panel", "Running a query" },
                        CorrectIndex = 0,
                        Explanation = "Concatenation mixes untrusted input with SQL instructions."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Safe simulation only. No real database is connected.",
                    Caption = "Safe simulation",
                    HighlightLine = 0,
                    TokenWaypoint = -1,
                    UnsafeData = true,
                    Question = new LessonQuestion
                    {
                        Prompt = "Tap the risky line.",
                        Options = new[] { UnsafeQuery, "run(query);" },
                        CorrectIndex = 0,
                        Explanation = "That line builds SQL by joining user text into the command.",
                        OptionsAreCode = true
                    }
                }
            }
        };
    }

    private static LessonModule BuildModule3()
    {
        return new LessonModule
        {
            Title = "Parameterized Queries",
            Activity = "Activity: switch between the unsafe and parameterized simulations.",
            Outcome = "Explain why parameterized queries are the primary defence against SQL injection.",
            CodeLines = new[] { SafeQuery, SafeExecute },
            Steps = new[]
            {
                new LessonStep
                {
                    Subtitle = "Parameterized queries separate SQL structure from user values.",
                    Caption = "SELECT ... = ?",
                    HighlightLine = 0,
                    TokenWaypoint = -1
                },
                new LessonStep
                {
                    Subtitle = "The question mark is a placeholder; the username is separate data.",
                    Caption = "? + [username]",
                    HighlightLine = 1,
                    TokenWaypoint = 0,
                    Question = new LessonQuestion
                    {
                        Prompt = "What does ? represent?",
                        Options = new[] { "A parameter placeholder", "A comment", "A table name" },
                        CorrectIndex = 0,
                        Explanation = "It marks where a value is supplied separately."
                    }
                },
                new LessonStep
                {
                    Subtitle = "The database treats the value as data.",
                    Caption = "Structure stays locked",
                    HighlightLine = 1,
                    TokenWaypoint = 2,
                    Question = new LessonQuestion
                    {
                        Prompt = "Can the username become extra SQL in the safe example?",
                        Options = new[] { "Yes", "No, it stays data" },
                        CorrectIndex = 1,
                        Explanation = "Parameters keep values separate from SQL structure."
                    }
                },
                new LessonStep
                {
                    Subtitle = "The parameterized version is safer.",
                    Caption = "Compare both versions",
                    HighlightLine = 1,
                    TokenWaypoint = 2
                }
            }
        };
    }

    private static LessonModule BuildModule4()
    {
        return new LessonModule
        {
            Title = "Defence in Layers",
            Activity = "Activity: build the layers one at a time, then switch them on and off.",
            Outcome = "Explain how validation, least privilege and safe errors support parameterized queries.",
            CodeLines = new[]
            {
                SafeExecute,
                "validate(username);",
                "GRANT SELECT ON users TO app_role;",
                "return \"Something went wrong.\";"
            },
            Steps = new[]
            {
                new LessonStep
                {
                    Subtitle = "Parameterized queries are the main defence.",
                    Caption = "Primary defence",
                    HighlightLine = 0,
                    TokenWaypoint = -1,
                    Question = new LessonQuestion
                    {
                        Prompt = "Which control is the primary defence?",
                        Options = new[] { "Parameterized queries", "Safe error messages" },
                        CorrectIndex = 0,
                        Explanation = "Other controls support it."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Validation checks whether input follows expected rules.",
                    Caption = "Validation shield",
                    HighlightLine = 1,
                    TokenWaypoint = -1
                },
                new LessonStep
                {
                    Subtitle = "Least privilege gives the app only the permissions it needs.",
                    Caption = "Limited permissions",
                    HighlightLine = 2,
                    TokenWaypoint = -1
                },
                new LessonStep
                {
                    Subtitle = "Safe error messages tell the user something went wrong without revealing database details.",
                    Caption = "Safe error messages",
                    HighlightLine = 3,
                    TokenWaypoint = -1
                },
                new LessonStep
                {
                    Subtitle = "Validation supports, but does not replace, parameterized queries.",
                    Caption = "Toggle the layers",
                    HighlightLine = -1,
                    TokenWaypoint = -1,
                    Question = new LessonQuestion
                    {
                        Prompt = "Does validation replace parameterized queries?",
                        Options = new[] { "Yes", "No, it is an additional control" },
                        CorrectIndex = 1,
                        Explanation = "Defence in depth: layers support each other."
                    }
                }
            }
        };
    }

    private static LessonModule BuildModule5()
    {
        return new LessonModule
        {
            Title = "Fix the Scenario",
            Activity = "Activity: find the unsafe code, choose the fix, then apply it.",
            Outcome = "Apply secure query practices to a simulated login scenario.",
            CodeLines = new[] { UnsafeQuery },
            Steps = new[]
            {
                new LessonStep
                {
                    Subtitle = "This practice login uses unsafe string concatenation.",
                    Caption = "Unsafe practice login",
                    HighlightLine = 0,
                    TokenWaypoint = 1,
                    UnsafeData = true
                },
                new LessonStep
                {
                    Subtitle = "Choose the change that separates SQL from the username.",
                    Caption = "Pick the main fix",
                    HighlightLine = 0,
                    TokenWaypoint = 1,
                    UnsafeData = true,
                    Question = new LessonQuestion
                    {
                        Prompt = "Choose the primary fix.",
                        Options = new[] { "Concatenate input", "Use a parameterized query", "Hide all errors only" },
                        CorrectIndex = 1,
                        Explanation = "Parameterized queries keep input as data."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Arrange the blocks into a secure query workflow.",
                    Caption = "Build the secure workflow",
                    HighlightLine = -1,
                    TokenWaypoint = 1,
                    CodeLines = new[]
                    {
                        "validate(username);",
                        SafeQuery,
                        SafeExecute,
                        "return safeMessage(result);"
                    }
                },
                new LessonStep
                {
                    Subtitle = "The parameterized version is safer.",
                    Caption = "Choose the safer version",
                    HighlightLine = 1,
                    TokenWaypoint = 2,
                    CodeLines = new[] { SafeQuery, SafeExecute },
                    Question = new LessonQuestion
                    {
                        Prompt = "Select the safer implementation.",
                        Options = new[]
                        {
                            "\"... = '\" + username + \"'\"",
                            "\"... = ?\"  +  execute(query, [username])"
                        },
                        CorrectIndex = 1,
                        Explanation = "The placeholder keeps the username as data.",
                        OptionsAreCode = true
                    }
                },
                new LessonStep
                {
                    Subtitle = "Correct: parameterized query first, supporting controls next.",
                    Caption = "Safe query applied",
                    HighlightLine = 1,
                    TokenWaypoint = 2,
                    CodeLines = new[] { SafeQuery, SafeExecute },
                    Question = new LessonQuestion
                    {
                        Prompt = "Pick the best supporting controls.",
                        Options = new[] { "Validation + least privilege", "Remove the database", "Hide all errors only" },
                        CorrectIndex = 0,
                        Explanation = "Add validation and limit database permissions."
                    }
                },
                new LessonStep
                {
                    Subtitle = "Module complete. Next, try the Security Challenge.",
                    Caption = "Module complete",
                    HighlightLine = -1,
                    TokenWaypoint = -1,
                    CodeLines = new[] { SafeQuery, SafeExecute }
                }
            }
        };
    }
}