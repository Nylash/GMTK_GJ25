using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TomatoFace : MiniGame
{
    [SerializeField] private RectTransform _handCursor;
    [SerializeField] private List<GameObject> _tomatoePieces = new List<GameObject>();
    [SerializeField] private int _totalPiecesToClean = 12;
    [SerializeField] private GameObject _ok;

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
                    StartCoroutine(CallFinishGame());
            }
        }
    }

    private IEnumerator CallFinishGame()
    {
        yield return new WaitForSeconds(0.5f);
        FinishGame();
    }

    public override void InitializeGame()
    {
        _ok.SetActive(false);
        int r = Random.Range(0, 21);
        if (r == 20)
            _ok.SetActive(true);
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
