using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] public Image hp1;
    [SerializeField] public Image hp2;
    [SerializeField] public Image hp3;

    [SerializeField] public TextMeshProUGUI lapCounter;

    [SerializeField] private GameObject _endScreen;
    [SerializeField] private TextMeshProUGUI _endScreenLapCounter;

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void GameEnded()
    {
        _endScreenLapCounter.text += lapCounter.text;
        _endScreen.SetActive(true);
    }
}
