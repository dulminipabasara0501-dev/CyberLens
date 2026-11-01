using System.Collections.Generic;
using UnityEngine;

// Saves scores and progress on the phone using PlayerPrefs. No passwords or personal data.
public static class ProgressStore
{
    private const string PreScoreKey = "cl_pre_score";
    private const string PreMissedKey = "cl_pre_missed";
    private const string PostScoreKey = "cl_post_score";
    private const string PostMissedKey = "cl_post_missed";
    private const string CompletedModulesKey = "cl_completed_modules";
    private const string ChallengeKey = "cl_challenge_done";

    // ----- Quiz results -----

    public static bool HasQuizResult(QuizKind kind)
    {
        return PlayerPrefs.HasKey(ScoreKey(kind));
    }

    public static int GetScore(QuizKind kind)
    {
        return PlayerPrefs.GetInt(ScoreKey(kind), 0);
    }

    public static List<int> GetMissedModules(QuizKind kind)
    {
        List<int> result = new List<int>();
        string saved = PlayerPrefs.GetString(MissedKey(kind), "");

        if (string.IsNullOrEmpty(saved))
        {
            return result;
        }

        foreach (string part in saved.Split(','))
        {
            int value;
            if (int.TryParse(part, out value))
            {
                result.Add(value);
            }
        }

        return result;
    }

    public static void SaveQuizResult(QuizKind kind, int score, List<int> missedModules)
    {
        PlayerPrefs.SetInt(ScoreKey(kind), score);
        PlayerPrefs.SetString(MissedKey(kind), string.Join(",", missedModules));
        PlayerPrefs.Save();
        AudioManager.Play(SoundId.Success);
    }

    // ----- Module progress -----

    public static bool IsModuleComplete(int moduleIndex)
    {
        return (PlayerPrefs.GetInt(CompletedModulesKey, 0) & (1 << moduleIndex)) != 0;
    }

    public static void MarkModuleComplete(int moduleIndex)
    {
        if (moduleIndex < 0 || moduleIndex >= ModuleLibrary.Count)
        {
            return;
        }

        int bits = PlayerPrefs.GetInt(CompletedModulesKey, 0);
        PlayerPrefs.SetInt(CompletedModulesKey, bits | (1 << moduleIndex));
        PlayerPrefs.Save();
        AudioManager.Play(SoundId.Success);
    }

    public static int CompletedModuleCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < ModuleLibrary.Count; i++)
            {
                if (IsModuleComplete(i))
                {
                    count++;
                }
            }
            return count;
        }
    }

    public static bool AllModulesComplete
    {
        get { return CompletedModuleCount == ModuleLibrary.Count; }
    }

    // ----- Security Challenge -----

    public static bool IsChallengeComplete
    {
        get { return PlayerPrefs.GetInt(ChallengeKey, 0) == 1; }
    }

    public static void MarkChallengeComplete()
    {
        PlayerPrefs.SetInt(ChallengeKey, 1);
        PlayerPrefs.Save();
        AudioManager.Play(SoundId.Success);
    }

    // ----- Resetting -----

    // Used by Replay: clears modules and the challenge, but keeps quiz scores.
    public static void ResetLessonProgress()
    {
        PlayerPrefs.DeleteKey(CompletedModulesKey);
        PlayerPrefs.DeleteKey(ChallengeKey);
        PlayerPrefs.Save();
    }

    private static string ScoreKey(QuizKind kind)
    {
        return kind == QuizKind.Pre ? PreScoreKey : PostScoreKey;
    }

    private static string MissedKey(QuizKind kind)
    {
        return kind == QuizKind.Pre ? PreMissedKey : PostMissedKey;
    }
}