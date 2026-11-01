// A SAFE SIMULATION. It only joins and compares text. There is no database,
// no network and nothing real is ever attacked.
public class SimulationResult
{
    public string Query;
    public string Response;
    public bool IsUnsafe;
}

public static class SqlSimulator
{
    public const string NormalInput = "alice";
    public const string CraftedInput = "' OR '1'='1";

    // Unsafe version: the input is joined straight into the query text.
    public static SimulationResult RunUnsafe(string input)
    {
        SimulationResult result = new SimulationResult();
        result.Query = "SELECT * FROM users WHERE username = '" + input + "'";

        if (LooksLikeInjection(input))
        {
            result.Response = "All rows returned: the query meaning changed";
            result.IsUnsafe = true;
        }
        else if (input == NormalInput)
        {
            result.Response = "1 row returned (alice)";
        }
        else
        {
            result.Response = "0 rows returned";
        }

        return result;
    }

    // Safe version: the query has a placeholder (?) and the input is a separate value.
    public static SimulationResult RunParameterized(string input)
    {
        SimulationResult result = new SimulationResult();
        result.Query = "SELECT * FROM users WHERE username = ?   value: " + input;

        if (input == NormalInput)
        {
            result.Response = "1 row returned (alice)";
        }
        else if (LooksLikeInjection(input))
        {
            result.Response = "0 rows: the input stayed plain data";
        }
        else
        {
            result.Response = "0 rows returned";
        }

        return result;
    }

    private static bool LooksLikeInjection(string input)
    {
        string lower = input.ToLowerInvariant();
        return lower.Contains("'") && (lower.Contains(" or ") || lower.Contains("--"));
    }
}