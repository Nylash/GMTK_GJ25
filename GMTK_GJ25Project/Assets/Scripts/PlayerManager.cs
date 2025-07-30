using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class PlayerManager : Singleton<PlayerManager>
{
    [SerializeField] private SplineContainer _splineContainer;
    [SerializeField] private float _speed;
    [SerializeField] private bool _reverse;

    private Spline _spline;
    private float _normalizedT;

    private void Start()
    {
        _spline = _splineContainer.Spline;
    }

    private void Update()
    {
        PlayerMover();
    }

    private void PlayerMover()
    {
        if (_reverse)
        {
            _normalizedT -= (_speed * Time.deltaTime) / _spline.GetLength();
            if (_normalizedT < 0f)
                _normalizedT += 1f;
        }
        else
        {
            _normalizedT += (_speed * Time.deltaTime) / _spline.GetLength();
            _normalizedT %= 1f;
        }

        float3 pos = SplineUtility.EvaluatePosition(_spline, _normalizedT);
        float3 tangent = SplineUtility.EvaluateTangent(_spline, _normalizedT);

        transform.position = Vector3.MoveTowards(transform.position, (Vector3)pos, _speed * Time.deltaTime);

        if (_reverse)
            transform.forward = -((Vector3)tangent).normalized;
        else
            transform.forward = ((Vector3)tangent).normalized;
    }
}
