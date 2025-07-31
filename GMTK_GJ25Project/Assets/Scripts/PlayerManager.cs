using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("_______________________________________________")]
    [Header("Player Movement")]
    [SerializeField] private float _speed;
    [SerializeField] private bool _reverse;
    [SerializeField] private Transform debugCube;

    private Spline _centralLane;
    private Spline _exteriorLane;
    private Spline _interiorLane;
    private Spline _targetLane;
    private float _normalizedT;
    private int _laneIndex = 0;
    private int _prevLaneIndex = 0;

    private void Start()
    {
        _centralLane = TrackManager.Instance.ContainerCentralLane.Spline;
        _exteriorLane = TrackManager.Instance.ContainerExteriorLane.Spline;
        _interiorLane = TrackManager.Instance.ContainerInteriorLane.Spline;

        _targetLane = _centralLane;
        InitializeSplinePosition(transform.position);
    }

    private void Update()
    {
        PlayerMovement();

        SwitchLane();
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

        debugCube.position = (Vector3)pos;

        float3 tangent = SplineUtility.EvaluateTangent(_targetLane, _normalizedT);

        transform.position = Vector3.MoveTowards(transform.position, (Vector3)pos, _speed * Time.deltaTime);

        if (_reverse)
            transform.forward = -((Vector3)tangent).normalized;
        else
            transform.forward = ((Vector3)tangent).normalized;
    }

    private void SwitchLane()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
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
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
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
}
