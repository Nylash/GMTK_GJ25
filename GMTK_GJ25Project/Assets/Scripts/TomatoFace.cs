using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TomatoFace : MiniGame
{
    [SerializeField] private RectTransform _handCursor;
    [SerializeField] private List<GameObject> _tomatoePieces = new List<GameObject>();
    [SerializeField] private int _totalPiecesToClean = 12;

    private List<GameObject> _selectedPieces = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (_handCursor.gameObject.activeSelf)
        {
            _handCursor.position = Input.mousePosition;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            GameObject hit = results
                .Select(r => r.gameObject)
                .FirstOrDefault(go => go.CompareTag("Tomato"));

            if (hit != null)
            {
                hit.SetActive(false);
                hit.tag = "Untagged";
                _selectedPieces.Remove(hit);
                if (_selectedPieces.Count <= 0)
                    FinishGame();
            }
        }
    }

    public override void InitializeGame()
    {
        foreach (var item in _tomatoePieces)
        {
            item.tag = "Tomato";
            item.SetActive(false);
        }
        _selectedPieces.Clear();
        _selectedPieces = _tomatoePieces.OrderBy(_ => UnityEngine.Random.value).Take(_totalPiecesToClean).ToList();
        foreach (var item in _selectedPieces)
            item.SetActive(true);
        _handCursor.gameObject.SetActive(true);
        _gameCanvas.SetActive(true);
    }
}
