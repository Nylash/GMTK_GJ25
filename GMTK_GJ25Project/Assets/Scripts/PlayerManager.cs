using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("_______________________________________________")]
    [Header("Track Configuration")]
    [SerializeField] private SplineContainer _containerCentralLane;
    [SerializeField] private float _laneWidth = 1.5f;
    [Header("Generate those lanes by right click -> Generate Lane Splines")]
    [SerializeField] private SplineContainer _containerExteriorLane;
    [SerializeField] private SplineContainer _containerInteriorLane;
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
        _centralLane = _containerCentralLane.Spline;
        _exteriorLane = _containerExteriorLane.Spline;
        _interiorLane = _containerInteriorLane.Spline;

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

#if UNITY_EDITOR
    [ContextMenu("Generate Lane Splines")]
    public void GenerateLaneSplines()
    {
        if (_containerCentralLane == null)
        {
            Debug.LogError("Center spline not assigned.");
            return;
        }

        if (_containerExteriorLane)
            DestroyImmediate(_containerExteriorLane.gameObject);
        if (_containerInteriorLane)
            DestroyImmediate(_containerInteriorLane.gameObject);

        var leftKnots = OffsetKnotArray(_containerCentralLane.Spline, -_laneWidth);
        var rightKnots = OffsetKnotArray(_containerCentralLane.Spline, +_laneWidth);
        _containerInteriorLane = CreateLaneObject("InteriorLaneSpline", leftKnots);
        _containerExteriorLane = CreateLaneObject("ExteriorLaneSpline", rightKnots);
    }

    private SplineContainer CreateLaneObject(string name, BezierKnot[] knots)
    {
        var go = new GameObject(name);

        var container = go.AddComponent<SplineContainer>();
        var spline = new Spline();
        for (int i = 0; i < knots.Length; i++)
        {
            spline.Add(knots[i]);
            spline.SetTangentMode(i, TangentMode.AutoSmooth);
        }
        spline.Closed = true;

        container.Spline = spline;
        return container;
    }

    private BezierKnot[] OffsetKnotArray(Spline sourceSpline, float offset)
    {
        int count = sourceSpline.Count;
        var result = new BezierKnot[count];

        for (int i = 0; i < count; i++)
        {
            float3 p = sourceSpline[i].Position;
            float3 offsetPos = p;

            if (p.x != 0)
                offsetPos.x += math.sign(p.x) * offset;
            if (p.z != 0)
                offsetPos.z += math.sign(p.z) * offset;

            result[i] = new BezierKnot(offsetPos);
        }

        return result;
    }
#endif
}
