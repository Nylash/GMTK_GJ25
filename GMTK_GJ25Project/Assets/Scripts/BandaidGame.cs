using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class BandaidGame : MiniGame
{
    [SerializeField] private RectTransform _bandaidCursor;
    [SerializeField] private List<GameObject> _injuries = new List<GameObject>();
    [SerializeField] private int _injuriesToHeal = 3;

    private List<GameObject> _selectedInjuries = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        _inputs.Player.MouseAction.started += ctx => PlaceBandaid();
    }

    private void Update()
    {
        if (_bandaidCursor.gameObject.activeSelf)
            _bandaidCursor.position = Input.mousePosition;
    }

    private void PlaceBandaid()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject hit = results
            .Select(r => r.gameObject)
            .FirstOrDefault(go => go.CompareTag("Injure"));

        if (hit != null)
        {
            hit.transform.GetChild(0).gameObject.SetActive(true);
            hit.tag = "Untagged";
            _selectedInjuries.Remove(hit);
            if (_selectedInjuries.Count <= 0)
                FinishGame();
        }
    }

    public override void InitializeGame()
    {
        foreach (var item in _injuries)
        {
            item.tag = "Injure";
            item.SetActive(false);
            item.transform.GetChild(0).gameObject.SetActive(false);
        }
        _selectedInjuries.Clear();
        _selectedInjuries = _injuries.OrderBy(_ => UnityEngine.Random.value).Take(_injuriesToHeal).ToList();
        foreach (var item in _selectedInjuries)
            item.SetActive(true);
        _bandaidCursor.gameObject.SetActive(true);
        _gameCanvas.SetActive(true);
    }
}
