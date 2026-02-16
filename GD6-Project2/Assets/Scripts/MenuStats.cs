using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuStats : MonoBehaviour
{
    public Text smartestText;
    public TextMeshProUGUI smartestTMP;

    void Awake()
    {
        int? best = SaveManager.GetBestFinishedSteps();
        string msg;
        if (best.HasValue)
        {
            msg = "smartest penguin's score: " + best.Value.ToString();
        }
        else
        {
            msg = "smartest penguin's score: -";
        }

        if (smartestText != null) smartestText.text = msg;
        if (smartestTMP != null) smartestTMP.text = msg;
    }
}
