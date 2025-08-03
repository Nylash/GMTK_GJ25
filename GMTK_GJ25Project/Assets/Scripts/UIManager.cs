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
    [SerializeField] private GameObject _reverseUI;
    [SerializeField] private Vector2 _timeBetweenReverse;
    [SerializeField] private GameObject _soundPrefab;
    [SerializeField] private AudioClip _audioReverse;
    [SerializeField][Range(0f, 2f)] private float _reverseVolume = 1f;
    [SerializeField] private AudioClip _audioEnd;
    [SerializeField][Range(0f, 2f)] private float _endVolume = 1f;
    [Header("_______________________________________________")]
    [Header("Mini Games Configuration")]
    [SerializeField] private List<MiniGame> _games = new List<MiniGame>();
    [Tooltip("Value taken randomly between X and Y")]
    [SerializeField] private Vector2 _timeBetweenMiniGame;

    private bool _inMiniGame;
    private float _timer;
    private float _targetTimer;
    private float _timerReverse;
    private float _targetTimerReverse;

    private MiniGame _previousMiniGame = null;

    public bool InMiniGame { get => _inMiniGame; set => _inMiniGame = value; }

    private void Start()
    {
        _targetTimer = Random.Range(_timeBetweenMiniGame.x, _timeBetweenMiniGame.y);
        _targetTimerReverse = Random.Range(_timeBetweenReverse.x, _timeBetweenReverse.y);

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

        _timerReverse += Time.deltaTime;
        if (_timerReverse > _targetTimerReverse)
        {
            _timerReverse = 0;
            _targetTimerReverse = Random.Range(_timeBetweenReverse.x, _timeBetweenReverse.y);
            _reverseUI.SetActive(true);
            StartCoroutine(DepopReverseUI());
        }
    }

    public AudioSource SpawnAudioSource(bool saveObject = false)
    {
        GameObject obj = Instantiate(_soundPrefab, _soundPrefab.transform.position, _soundPrefab.transform.rotation);
        if (!saveObject)
            StartCoroutine(DestroyAudioSource(obj));
        return obj.GetComponent<AudioSource>();
    }

    private IEnumerator DestroyAudioSource(GameObject obj)
    {
        yield return new WaitForSeconds(2.1f);
        Destroy(obj);
    }

    private IEnumerator DepopReverseUI()
    {
        AudioSource source = SpawnAudioSource();
        source.PlayOneShot(_audioReverse, _reverseVolume);
        yield return new WaitForSeconds(1f);
        PlayerManager.Instance.ReverseDirection();
        yield return new WaitForSeconds(2f);
        _reverseUI.GetComponent<Animator>().SetTrigger("Depop");
    }

    public void PauseMenu()
    {
        if (_endScreen.activeSelf) return;

        PlayerManager.Instance.gamePaused = !PlayerManager.Instance.gamePaused;
        _menu.SetActive(!_menu.activeSelf);
        Cursor.visible = !Cursor.visible;
        PlayerManager.Instance.MovementAnimator.speed = PlayerManager.Instance.gamePaused ? 0 : 1;
    }

    private void PlayMiniGame()
    {
        if (PlayerManager.Instance.gamePaused)
            return;

        _inMiniGame = true;

        MiniGame tmp = _games[Random.Range(0, _games.Count)];

        while (tmp == _previousMiniGame) 
            tmp = _games[Random.Range(0, _games.Count)];
            
        tmp.InitializeGame();
        _previousMiniGame = tmp;
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
        PlayerManager.Instance.MovementAnimator.speed = 0;
        PlayerManager.Instance.MovementAnimator.gameObject.SetActive(false);
        foreach (var item in _games)
            item.gameObject.SetActive(false);
        StartCoroutine(ActivateEndButtons());
        AudioSource source = SpawnAudioSource(true);
        source.PlayOneShot(_audioEnd, _endVolume);
    }

    private IEnumerator ActivateEndButtons()
    {
        yield return new WaitForSeconds(1f);
        _endReplay.interactable = true;
        _endQuit.interactable = true;
    }
}
