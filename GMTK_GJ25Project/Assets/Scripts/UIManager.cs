using System.Collections;
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
    [SerializeField] private GameObject _menu;
    [SerializeField] private Button _endReplay;
    [SerializeField] private Button _endQuit;
    [Header("_______________________________________________")]
    [Header("Mini Games Configuration")]
    [SerializeField] private List<MiniGame> _games = new List<MiniGame>();
    [Tooltip("Value taken randomly between X and Y")]
    [SerializeField] private Vector2 _timeBetweenMiniGame;

    private bool _inMiniGame;
    private float _timer;
    private float _targetTimer;

    public bool InMiniGame { get => _inMiniGame; set => _inMiniGame = value; }

    private void Start()
    {
        _targetTimer = Random.Range(_timeBetweenMiniGame.x, _timeBetweenMiniGame.y);

        Cursor.visible = false;
    }

    private void Update()
    {
        if (PlayerManager.Instance.gamePaused) return;

        if (_inMiniGame) return;

        _timer += Time.deltaTime;
        if (_timer > _targetTimer)
        {
            _timer = 0;
            _targetTimer = Random.Range(_timeBetweenMiniGame.x, _timeBetweenMiniGame.y);
            PlayMiniGame();
        }
    }

    public void PauseMenu()
    {
        if (_endScreen.activeSelf) return;

        PlayerManager.Instance.gamePaused = !PlayerManager.Instance.gamePaused;
        _menu.SetActive(!_menu.activeSelf);
        Cursor.visible = !Cursor.visible;
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
        PlayerManager.Instance.gamePaused = true;
        Cursor.visible = true;
        foreach (var item in _games)
            item.gameObject.SetActive(false);
        StartCoroutine(ActivateEndButtons());
    }

    private IEnumerator ActivateEndButtons()
    {
        yield return new WaitForSeconds(1f);
        _endReplay.interactable = true;
        _endQuit.interactable = true;
    }
}
