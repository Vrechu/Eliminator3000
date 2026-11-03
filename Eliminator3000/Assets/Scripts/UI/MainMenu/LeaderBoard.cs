using TMPro;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField] private int scoresToDisplay = 10;
    [SerializeField] private TextMeshProUGUI leaderBoardText;

    private void OnEnable()
    {
        UpdateScoresText();
    }

    public void UpdateScoresText()
    {
        if (ScoreManager.Instance == null || leaderBoardText == null)
        {
            Debug.LogWarning("ScoreManager instance or leaderBoardText is null.");
            return;
        }
        leaderBoardText.text = "";
        for (int i = 0; i < Mathf.Min(ScoreManager.Instance.ScoreList.Count, scoresToDisplay); i++)
        {
            leaderBoardText.text += $"Player {i+1}: {ScoreManager.Instance.ScoreList[i].Value} : {ScoreManager.Instance.ScoreList[i].Key}. \n";
        }
    }
}
