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
    [SerializeField] private bool _showGridDebug;
    [Header("Generate those lanes by right click -> Generate Lane Splines")]
    [SerializeField] private SplineContainer _containerExteriorLane;
    [SerializeField] private SplineContainer _containerInteriorLane;
    [SerializeField] private List<GridCell> _gridCells = new List<GridCell>();//Use only for debug and populate dictionary
    [Header("_______________________________________________")]
    [Header("Obstacles Configuration")]
    [SerializeField] private Collider _finishLineTrigger;
    [SerializeField] private List<GameObject> _obstaclePrefab = new List<GameObject>();
    [SerializeField] private float _timeBetweenObstacle = 5f;
    [SerializeField] private float _minDistanceWithPlayer = 5f;
    [SerializeField] private GameObject _VFXpop;

    public SplineContainer ContainerCentralLane { get => _containerCentralLane; }
    public SplineContainer ContainerExteriorLane { get => _containerExteriorLane; }
    public SplineContainer ContainerInteriorLane { get => _containerInteriorLane; }

    private Dictionary<Vector2Int, GridCell> _grid = new Dictionary<Vector2Int, GridCell>();

    protected override void OnAwake()
    {
        _grid.Clear();
        foreach (GridCell cell in _gridCells)
        {
            _grid[cell.Coordinates] = cell;
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(SpawnObstacle), _timeBetweenObstacle, _timeBetweenObstacle);
        Invoke(nameof(ActivateFinishLine), 1f);
    }

    private void ActivateFinishLine()
    {
        _finishLineTrigger.enabled = true;
    }

    private void SpawnObstacle()
    {
        if (PlayerManager.Instance.gamePaused)
            return;

        GridCell targetCell = PickRandomGridCell();
        if (targetCell == null)
            return;

        Instantiate(_VFXpop, targetCell.WorldPosition, Quaternion.identity);

        GameObject obstacle = Instantiate(_obstaclePrefab[UnityEngine.Random.Range(0, _obstaclePrefab.Count)], targetCell.WorldPosition, Quaternion.identity);
        //obstacle.transform.right = targetCell.Tangent.normalized; // Align obstacle with the lane direction
        //obstacle.transform.GetChild(0).transform.rotation = Quaternion.Euler(new Vector3(0, -obstacle.transform.rotation.y, 0));
        targetCell.IsOccupied = true; // Mark the cell as occupied
        obstacle.GetComponentInChildren<SpriteRenderer>().sortingOrder = PlayerManager.Instance.GetOrderFromZ(obstacle.transform.position.z);
    }

    private GridCell PickRandomGridCell()
    {
        GridCell targetCell = null;
        int currentAttempts = 0;

        while (targetCell == null && currentAttempts < 30)
        {
            currentAttempts++;

            int row = UnityEngine.Random.Range(0, _numSegments);
            row = LoopRow(row); // Ensure row is within bounds
            int lane = UnityEngine.Random.Range(0, 3);

            _grid.TryGetValue(new Vector2Int(lane, row), out var tempCell);

            bool playerHit = false;
            Collider[] hitColliders = Physics.OverlapSphere(tempCell.WorldPosition, _minDistanceWithPlayer);
            foreach (var hitCollider in hitColliders)// Avoid position too close of the player
            {
                if (hitCollider.CompareTag("Player"))
                {
                    playerHit = true;
                    break;
                }
            }
            if (playerHit)
                continue;

            if (_grid.TryGetValue(new Vector2Int(lane, row), out var cell) && cell.IsOccupied)// Cell is already occupied, skip
                continue;

            if (IsRowFull(row))// Row is full, skip this cell
                continue;

            bool prevFull = IsRowFull(LoopRow(row - 1));
            bool nextFull = IsRowFull(LoopRow(row + 1));

            if (prevFull && nextFull)// Both neighbors are full, skip this cell
                continue;

            if (prevFull || nextFull)// If the selected lane is free in the full adjacent row → skip
            {
                int fullRow = prevFull ? LoopRow(row - 1) : LoopRow(row + 1);
                if (_grid.TryGetValue(new Vector2Int(lane, fullRow), out var fullRowCell) && !fullRowCell.IsOccupied)
                    continue;
            }

            /*
            if (!HasReachableCellFromPreviousRow(row, lane))// No reachable cell in the previous row, skip
                continue;

            if (!HasReachableCellToNextRow(row, lane))// No reachable cell in the next row, skip
                continue;

            tempCell.IsOccupied = true;// Mark the cell as occupied to recompute reachability

            RecomputeReachability();

            if (!HasFullPath())// Blocking this cell would break the path, skip
            {
                tempCell.IsOccupied = false;
                continue;
            }
            */
            targetCell = _grid.GetValueOrDefault(new Vector2Int(lane, row));
        }
        return targetCell;
    }

    int LoopRow(int row) => (row + 50) % 50;

    private bool IsRowFull(int row)
    {
        int obstacleCount = 0;

        for (int lane = 0; lane < 3; lane++)
        {
            if (_grid.TryGetValue(new Vector2Int(lane, row), out GridCell cell) && cell.IsOccupied)
            {
                obstacleCount++;
                if (obstacleCount >= 2)
                    return true;
            }
        }
        return false;
    }

    #region PATH CHECK
    private bool HasReachableCellFromPreviousRow(int row, int blockedLane)
    {
        int prevRow = LoopRow(row - 1);

        for (int lane = 0; lane < 3; lane++)
        {
            if (lane == blockedLane)
                continue;

            var coord = new Vector2Int(lane, row);
            var fromCoord = new Vector2Int(lane, prevRow);

            if (_grid.TryGetValue(coord, out var cell) && !cell.IsOccupied &&
                _grid.TryGetValue(fromCoord, out var fromCell) && fromCell.IsReachable)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasReachableCellToNextRow(int row, int blockedLane)
    {
        int nextRow = LoopRow(row + 1);

        for (int nextLane = 0; nextLane < 3; nextLane++)
        {
            var targetCoord = new Vector2Int(nextLane, nextRow);
            if (!_grid.TryGetValue(targetCoord, out var targetCell) || targetCell.IsOccupied)
                continue;

            for (int dl = -1; dl <= 1; dl++)
            {
                int lane = nextLane + dl;
                if (lane < 0 || lane > 2 || lane == blockedLane)
                    continue;

                var fromCoord = new Vector2Int(lane, row);
                if (_grid.TryGetValue(fromCoord, out var fromCell) && !fromCell.IsOccupied)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RecomputeReachability()
    {
        // First, clear all reachability
        foreach (var cell in _grid.Values)
            cell.IsReachable = false;

        // Step 1: Initialize reachable starting cells at row 0
        for (int lane = 0; lane < 3; lane++)
        {
            var coord = new Vector2Int(lane, 0);
            if (_grid.TryGetValue(coord, out var cell) && !cell.IsOccupied)
                cell.IsReachable = true;
        }

        // Step 2: Propagate reachability forward over all rows
        for (int r = 1; r < _numSegments; r++)
        {
            int row = LoopRow(r);
            int prevRow = LoopRow(row - 1);

            for (int lane = 0; lane < 3; lane++)
            {
                var coord = new Vector2Int(lane, row);
                if (!_grid.TryGetValue(coord, out var currentCell) || currentCell.IsOccupied)
                    continue;

                // Check previous row for any reachable neighbor
                for (int dl = -1; dl <= 1; dl++)
                {
                    int prevLane = lane + dl;
                    if (prevLane < 0 || prevLane > 2)
                        continue;

                    var prevCoord = new Vector2Int(prevLane, prevRow);
                    if (_grid.TryGetValue(prevCoord, out var prevCell) && prevCell.IsReachable)
                    {
                        currentCell.IsReachable = true;
                        break;
                    }
                }
            }
        }

        // Step 3: Wraparound from last row to row 0
        int finalRow = LoopRow(_numSegments - 1);
        for (int lane = 0; lane < 3; lane++)
        {
            var coord = new Vector2Int(lane, 0);
            if (_grid.TryGetValue(coord, out var cell) && !cell.IsOccupied)
            {
                for (int dl = -1; dl <= 1; dl++)
                {
                    int prevLane = lane + dl;
                    if (prevLane < 0 || prevLane > 2)
                        continue;

                    var prevCoord = new Vector2Int(prevLane, finalRow);
                    if (_grid.TryGetValue(prevCoord, out var prevCell) && prevCell.IsReachable)
                    {
                        cell.IsReachable = true;
                        break;
                    }
                }
            }
        }
    }

    private bool HasFullPath()
    {
        for (int row = 0; row < _numSegments; row++)
        {
            bool rowHasReachableCell = false;
            for (int lane = 0; lane < 3; lane++)
            {
                var coord = new Vector2Int(lane, row);
                if (_grid.TryGetValue(coord, out var cell) && cell.IsReachable)
                {
                    rowHasReachableCell = true;
                    break;
                }
            }

            if (!rowHasReachableCell)
                return false; // No path through this row
        }

        return true;
    }
    #endregion


#if UNITY_EDITOR
    private void DrawDebugCell(GridCell cell, float cellSize)
    {
        Vector3 forward = cell.Tangent.normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3 p0 = cell.WorldPosition + right * (cellSize / 2f);
        Vector3 p1 = cell.WorldPosition - right * (cellSize / 2f);
        Vector3 p2 = p1;
        Vector3 p3 = p0;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);
    }


    private void OnDrawGizmos()
    {
        if (_showGridDebug)
        {
            if (_gridCells == null || _gridCells.Count == 0) return;

            foreach (var cell in _gridCells)
            {
                DrawDebugCell(cell, _laneWidth);
            }
        }
    }

    [ContextMenu("Generate Lane Splines")]
    public void GenerateLaneSplines()
    {
        //Tracks are good, comment code for security
        /*if (_containerCentralLane == null)
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
        _containerExteriorLane = CreateLaneObject("ExteriorLaneSpline", rightKnots);*/

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

                float3 posAdjustedToLane;
                SplineUtility.GetNearestPoint(GetSpline(lane - 1), pos, out posAdjustedToLane, out _);

                //tangent = SplineUtility.EvaluateTangent(GetSpline(lane - 1), t); //Use local tangent

                _gridCells.Add(new GridCell
                {
                    Coordinates = new Vector2Int(lane, row),
                    WorldPosition = posAdjustedToLane,
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
    public bool IsReachable = true;
}