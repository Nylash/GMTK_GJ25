using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] public Image hp1;
    [SerializeField] public Image hp2;
    [SerializeField] public Image hp3;

    [SerializeField] public TextMeshProUGUI lapCounter;
}
