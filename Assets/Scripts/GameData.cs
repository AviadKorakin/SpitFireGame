using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public string Method { get; private set; }
    public int Score { get; private set; }
    public float Time { get; private set; }
    public string Reason { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This keeps GameData alive across scenes
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }
    }

    public void SetGameOutcome(string method, int score, float time, string reason)
    {
        Method = method;
        Score = score;
        Time = time;
        Reason = reason;
    }
}
