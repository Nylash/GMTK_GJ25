using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class TrackManager : Singleton<TrackManager>
{
    [Header("_______________________________________________")]
    [Header("Track Configuration")]
    [SerializeField] private SplineContainer _containerCentralLane;
    [SerializeField] private float _laneWidth = 1.5f;
    [SerializeField] private int _numSegments = 50;
    [Header("Generate those lanes by right click -> Generate Lane Splines")]
    [SerializeField] private SplineContainer _containerExteriorLane;
    [SerializeField] private SplineContainer _containerInteriorLane;
    [SerializeField] private List<GridCell> _gridCells = new List<GridCell>();
    [Header("_______________________________________________")]
    [Header("Obstacles Configuration")]
    [SerializeField] private List<GameObject> _obstaclePrefab = new List<GameObject>();

    public SplineContainer ContainerCentralLane { get => _containerCentralLane; }
    public SplineContainer ContainerExteriorLane { get => _containerExteriorLane; }
    public SplineContainer ContainerInteriorLane { get => _containerInteriorLane; }

    private void Start()
    {

    }

#if UNITY_EDITOR
    private void DrawDebugCell(GridCell cell, float cellSize)
    {
        Vector3 forward = cell.Tangent.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3 p0 = cell.WorldPosition + right * (cellSize / 2f);
        Vector3 p1 = cell.WorldPosition - right * (cellSize / 2f);
        Vector3 p2 = p1 + forward * cellSize;
        Vector3 p3 = p0 + forward * cellSize;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);
    }


    private void OnDrawGizmos()
    {
        if (_gridCells == null || _gridCells.Count == 0) return;

        foreach (var cell in _gridCells)
        {
            DrawDebugCell(cell, _laneWidth);
        }
    }

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

        CreateGrid();
    }

    private void CreateGrid()
    {
        _gridCells.Clear();

        for (int row = 0; row < _numSegments; row++)
        {
            float t = row / (float)_numSegments;

            // Use center lane only for positioning
            var centerPos = SplineUtility.EvaluatePosition(_containerCentralLane.Spline, t);
            var tangent = SplineUtility.EvaluateTangent(_containerCentralLane.Spline, t);
            var right = Vector3.Cross(Vector3.up, tangent).normalized;

            for (int lane = 0; lane < 3; lane++)
            {
                float laneOffset = (lane - 1) * _laneWidth; // lane 0: -1, lane 1: 0, lane 2: +1
                Vector3 pos = (Vector3)centerPos + right * laneOffset;

                //tangent = SplineUtility.EvaluateTangent(GetSpline(lane - 1), t); //Use local tangent

                _gridCells.Add(new GridCell
                {
                    Coordinates = new Vector2Int(row, lane),
                    WorldPosition = pos,
                    Tangent = tangent,
                    IsOccupied = false
                });
            }
        }

    }

    private SplineContainer CreateLaneObject(string name, BezierKnot[] knots)
    {
        var go = new GameObject(name);
        go.transform.SetParent(this.transform);

        var container = go.AddComponent<SplineContainer>();
        var spline = new Spline();
        for (int i = 0; i < knots.Length; i++)
        {
            spline.Add(knots[i]);
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
            var original = sourceSpline[i];

            float3 offsetPos = original.Position;

            if (offset != 0)
            {
                if (offsetPos.x != 0)
                    offsetPos.x += math.sign(offsetPos.x) * offset;
                if (offsetPos.z != 0)
                    offsetPos.z += math.sign(offsetPos.z) * offset;
            }

            result[i] = new BezierKnot
            {
                Position = offsetPos,
                TangentIn = original.TangentIn,
                TangentOut = original.TangentOut,
                Rotation = original.Rotation
            };
        }

        return result;
    }

    private Spline GetSpline(int index)
    {
        switch (index)
        {
            case -1:
                return _containerInteriorLane.Spline;
            case 0:
                return _containerCentralLane.Spline;
            case 1:
                return _containerExteriorLane.Spline;
            default:
                Debug.LogError("Invalid lane index requested: " + index);
                return null;
        }
    }
#endif
}

[System.Serializable]
public class GridCell
{
    public Vector2Int Coordinates; //lane, row
    public Vector3 WorldPosition;
    public Vector3 Tangent;
    public bool IsOccupied;
}