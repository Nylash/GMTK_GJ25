using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using static UnityEngine.Rendering.DebugUI;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("_______________________________________________")]
    [Header("Player Movement")]
    [SerializeField] private float _initialSpeed;
    [SerializeField] private float _speedIncreasePerSecond;
    [SerializeField] private float _animationSpeedIncreasePerSecond;
    [SerializeField] private bool _reverse;
    [SerializeField] private AudioClip _audioHit;
    [SerializeField][Range(0f, 2f)] private float _hitVolume = 1f;
    [SerializeField] private AudioClip _audioHeal;
    [SerializeField][Range(0f, 2f)] private float _healVolume = 1f;
    [Header("_______________________________________________")]
    [Header("Player Health")]
    [SerializeField] private int _initialHealth = 3;
    [SerializeField] private float _iframeDuration = 1f;

    public bool gamePaused;

    private InputSystem_Actions _inputs;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Animator _movementAnimator;

    private Spline _centralLane;
    private Spline _exteriorLane;
    private Spline _interiorLane;
    private Spline _targetLane;
    private int _laneIndex = 0;
    private int _prevLaneIndex = 0;

    private float _speed;
    private float _normalizedT;

    private int _currentHealth;
    private bool _inIFrame;

    private int _lapCount;

    public Animator MovementAnimator { get => _movementAnimator; }

    private void OnEnable() => _inputs.Player.Enable();
    private void OnDisable() => _inputs.Player.Disable();

    protected override void OnAwake()
    {
        _inputs = new InputSystem_Actions();
        _inputs.Player.Left.performed += ctx => LeftInput();
        _inputs.Player.Right.performed += ctx => RightInput();
        _inputs.Player.Pause.performed += ctx => UIManager.Instance.PauseMenu();
    }

    private void Start()
    {
        _centralLane = TrackManager.Instance.ContainerCentralLane.Spline;
        _exteriorLane = TrackManager.Instance.ContainerExteriorLane.Spline;
        _interiorLane = TrackManager.Instance.ContainerInteriorLane.Spline;

        _speed = _initialSpeed;
        _currentHealth = _initialHealth;

        _targetLane = _centralLane;
        InitializeSplinePosition(transform.position);

        _animator = GetComponent<Animator>();
        _movementAnimator = transform.GetChild(0).GetComponent<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void Update()
    {
        if (gamePaused) return;

        PlayerMovement();

        _speed += _speedIncreasePerSecond * Time.deltaTime;
        _movementAnimator.SetFloat("Speed", _movementAnimator.GetFloat("Speed") + _animationSpeedIncreasePerSecond * Time.deltaTime);

        _spriteRenderer.sortingOrder = GetOrderFromZ(transform.position.z);
    }

    public void ReverseDirection()
    {
        _reverse = !_reverse;
        FlipSprite();
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Obstacle":
                if (!_inIFrame)
                    HitObstacle(other.gameObject);
                break;
            case "FinishLine":
                _lapCount++;
                UIManager.Instance.lapCounter.text = _lapCount.ToString();
                break;
            case "Flip":
                FlipSprite();
                break;
            default:
                Debug.Log("This object tag is not handle by player trigger detection:" +  other.gameObject.name);
                break;
        }
    }

    private void HitObstacle(GameObject go)
    {
        _currentHealth--;
        _inIFrame = true;
        _animator.SetBool("IFrame", true);
        StartCoroutine(ResetIFrame());
        switch (_currentHealth)
        {
            case 0:
                gamePaused = true;
                UIManager.Instance.GameEnded();
                UIManager.Instance.hp1.SetTrigger("Loss");
                break;
            case 1:
                UIManager.Instance.hp2.SetTrigger("Loss");
                StartCoroutine(FreezeForSeconds(0.1f));

                break;
            case 2:
                UIManager.Instance.hp3.SetTrigger("Loss");
                StartCoroutine(FreezeForSeconds(0.1f));
                break;
            default:
                Debug.Log("Incorrect health value " + _currentHealth);
                break;
        }
        AudioSource source = UIManager.Instance.SpawnAudioSource();
        source.PlayOneShot(_audioHit, _hitVolume);
    }

    public void GainHealth()
    {
        if (PlayerManager.Instance.gamePaused)
            return;

        if (_currentHealth == 3)
            return;

        _currentHealth++;
        switch (_currentHealth)
        {
            case 1:
                UIManager.Instance.hp1.SetTrigger("Gain");
                break;
            case 2:
                UIManager.Instance.hp2.SetTrigger("Gain");
                break;
            case 3:
                UIManager.Instance.hp3.SetTrigger("Gain");
                break;
            default:
                Debug.Log("Incorrect health value " + _currentHealth);
                break;
        }
        AudioSource source = UIManager.Instance.SpawnAudioSource();
        source.PlayOneShot(_audioHeal, _healVolume);
    }

    private IEnumerator ResetIFrame()
    {
        yield return new WaitForSeconds(_iframeDuration);
        _inIFrame = false;
        _animator.SetBool("IFrame", false);
    }

    private void PlayerMovement()
    {
        if (_reverse)
        {
            _normalizedT -= (_speed * Time.deltaTime) / _targetLane.GetLength();
            if (_normalizedT < 0f)
                _normalizedT += 1f;
        }
        else
        {
            _normalizedT += (_speed * Time.deltaTime) / _targetLane.GetLength();
            _normalizedT %= 1f;
        }

        float3 pos = SplineUtility.EvaluatePosition(_targetLane, _normalizedT);

        float3 tangent = SplineUtility.EvaluateTangent(_targetLane, _normalizedT);

        transform.position = Vector3.MoveTowards(transform.position, (Vector3)pos, _speed * Time.deltaTime);

        /*if (_reverse)
            transform.forward = -((Vector3)tangent).normalized;
        else
            transform.forward = ((Vector3)tangent).normalized;*/
    }

    private void RightInput()
    {
        if (gamePaused) return;

        if (_reverse)
            _laneIndex = Mathf.Min(_laneIndex + 1, 1);
        else
            _laneIndex = Mathf.Max(_laneIndex - 1, -1);
        if (_prevLaneIndex != _laneIndex)
        {
            _prevLaneIndex = _laneIndex;
            UpdateTargetLane();
        }
    }

    private void LeftInput()
    {
        if (gamePaused) return;

        if (_reverse)
            _laneIndex = Mathf.Max(_laneIndex - 1, -1);
        else
            _laneIndex = Mathf.Min(_laneIndex + 1, 1);
        if (_prevLaneIndex != _laneIndex)
        {
            _prevLaneIndex = _laneIndex;
            UpdateTargetLane();
        }
    }

    private void InitializeSplinePosition(Vector3 worldPosition)
    {
        SplineUtility.GetNearestPoint(_targetLane, worldPosition, out _, out float t);
        _normalizedT = t;
    }

    private void UpdateTargetLane()
    {
        switch (_laneIndex)
        {
            case -1:
                _targetLane = _interiorLane;
                break;
            case 0:
                _targetLane = _centralLane;
                break;
            case 1:
                _targetLane = _exteriorLane;
                break;
        }
        InitializeSplinePosition(transform.position);
    }

    private void FlipSprite()
    {
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
    }

    public int GetOrderFromZ(float z)
    {
        float t = Mathf.InverseLerp(11f, -11f, z); // Note the reversed range to flip order
        return Mathf.RoundToInt(Mathf.Lerp(1f, 1000f, t));
    }

    IEnumerator FreezeForSeconds(float duration)
    {
        Time.timeScale = 0f; // Freeze time
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f; // Resume time
    }
}
