using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    [Header("_______________________________________________")]
    [Header("UI Configuration")]
    [SerializeField] public Animator hp1;
    [SerializeField] public Animator hp2;
    [SerializeField] public Animator hp3;
    [SerializeField] public TextMeshProUGUI lapCounter;
    [SerializeField] private GameObject _endScreen;
    [SerializeField] private TextMeshProUGUI _endScreenLapCounter;
    [Header("_______________________________________________")]
    [Header("Mini Games Configuration")]
    [SerializeField] private List<MiniGame> _games = new List<MiniGame>();
    [Tooltip("Value taken randomly between X and Y")]
    [SerializeField] private Vector2 _timeBetweenMiniGame;

    private bool _inMiniGame;

    public bool InMiniGame { get => _inMiniGame; }

    private void Start()
    {
        InvokeMiniGame();
    }

    public void InvokeMiniGame()
    {
        _inMiniGame = false;
        Invoke(nameof(PlayMiniGame), Random.Range(_timeBetweenMiniGame.x, _timeBetweenMiniGame.y));
    }

    private void PlayMiniGame()
    {
        if (PlayerManager.Instance.gamePaused)
            return;

        _inMiniGame = true;
        _games[Random.Range(0, _games.Count)].InitializeGame();
    }

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
