using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shop tabs: selected tab = bright border + bold label + underline bar (not colour only).
public class TabGroup : MonoBehaviour
{
    public Button[] tabs;
    public Image[] borders;
    public TMP_Text[] labels;
    public GameObject[] indicators;
    public GameObject[] contents;
    public Color onColor = Color.cyan, offColor = Color.gray, offTextColor = Color.gray;

    void Start()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int idx = i;
            tabs[i].onClick.AddListener(() => Select(idx));
        }
        Select(0);
    }

    public void Select(int i)
    {
        for (int j = 0; j < tabs.Length; j++)
        {
            bool on = j == i;
            contents[j].SetActive(on);
            borders[j].color = on ? onColor : offColor;
            labels[j].color = on ? onColor : offTextColor;
            labels[j].fontStyle = on ? FontStyles.Bold : FontStyles.Normal;
            indicators[j].SetActive(on);
        }
    }
}
