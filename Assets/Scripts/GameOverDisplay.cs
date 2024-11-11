using UnityEngine;
using UnityEngine.UI;

public class GameDataDisplay : MonoBehaviour
{
    [SerializeField] private Text resultText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text reasonText;

    private void Start()
    {
        if (GameData.Instance != null)
        {
            // Fetch the data from GameData and display it
            string res = GameData.Instance.Method;
            if (res.Equals("DEFEAT")) resultText.color = Color.red;
            else resultText.color = Color.green;
            resultText.text = GameData.Instance.Method;
            scoreText.text = "SCORE: " + GameData.Instance.Score.ToString();
            timeText.text = "TIME: " + GameData.Instance.Time.ToString("F2") + " seconds";
            reasonText.text = "DETAILS: " + GameData.Instance.Reason;
        }
    }
}
