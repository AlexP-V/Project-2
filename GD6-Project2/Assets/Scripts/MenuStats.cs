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
            msg = "one penguin reached finish in only " + best.Value.ToString() + " steps, hehe...";
        }
        else
        {
            msg = "one penguin reached finish in only - steps, hehe...";
        }

        if (smartestText != null) smartestText.text = msg;
        if (smartestTMP != null) smartestTMP.text = msg;
    }
}
