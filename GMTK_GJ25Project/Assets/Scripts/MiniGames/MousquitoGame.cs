using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MousquitoGame : MiniGame
{
    [SerializeField] private RectTransform _cursor;
    [SerializeField] private List<GameObject> _mousquitos = new List<GameObject>();
    [SerializeField] private List<RectTransform> _bitingPos = new List<RectTransform>();
    [SerializeField] private float _mousquitoMovementSpeed;
    public float interval = 0.5f;

    private List<GameObject> _remainingMousquitos = new List<GameObject>();
    private GameObject _currentMousquitos;
    private Dictionary<GameObject, Vector4> _mousquitosTargetAnchors = new Dictionary<GameObject, Vector4>();
    private float timer;

    protected override void Awake()
    {
        base.Awake();

        _inputs.Player.MouseAction.started += ctx => SmashMousquito();

        timer = interval;
    }

    private void Update()
    {
        if (_cursor.gameObject.activeSelf)
        {
            _cursor.position = Input.mousePosition;
            //Get new target pos
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                foreach (GameObject mousquito in _remainingMousquitos)
                {
                    //if (mousquito == _currentMousquitos) continue;

                    timer = interval;
                    _mousquitosTargetAnchors[mousquito] = MoveAnchors(mousquito.GetComponent<RectTransform>());
                }
            }
            //Movement
            foreach (GameObject mousquito in _remainingMousquitos)
            {
                RectTransform rt = mousquito.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.MoveTowards(rt.anchorMin,
                    new Vector2(_mousquitosTargetAnchors[mousquito].x, _mousquitosTargetAnchors[mousquito].y),
                    _mousquitoMovementSpeed * Time.deltaTime);
                rt.anchorMax = Vector2.MoveTowards(rt.anchorMax,
                    new Vector2(_mousquitosTargetAnchors[mousquito].z, _mousquitosTargetAnchors[mousquito].w),
                    _mousquitoMovementSpeed * Time.deltaTime);
            }
        }

    }

    private Vector4 MoveAnchors(RectTransform rt, RectTransform focus = null)
    {
        // Compute current anchor size
        Vector2 anchorSize = rt.anchorMax - rt.anchorMin;

        // Compute safe center range so the object stays fully within (0,1)
        Vector2 minCenter = anchorSize * 0.5f;
        Vector2 maxCenter = Vector2.one - anchorSize * 0.5f;

        Vector2 center;
        if (focus != null)
        {
            // Use the single anchor point of the focus object
            Vector2 focusPoint = focus.anchorMin;

            float range = 0.05f; // how far from focus point (in normalized anchor space)

            center = new Vector2(
                Mathf.Clamp(focusPoint.x + Random.Range(-range, range), minCenter.x, maxCenter.x),
                Mathf.Clamp(focusPoint.y + Random.Range(-range, range), minCenter.y, maxCenter.y)
            );
        }
        else
        {
            center = new Vector2(
                Random.Range(minCenter.x, maxCenter.x),
                Random.Range(minCenter.y, maxCenter.y)
            );
        }

        Vector2 anchorMin = center - anchorSize * 0.5f;
        Vector2 anchorMax = center + anchorSize * 0.5f;

        return new Vector4(anchorMin.x, anchorMin.y, anchorMax.x, anchorMax.y);
    }

    private void SmashMousquito()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject hit = results
            .Select(r => r.gameObject)
            .FirstOrDefault(go => go.CompareTag("Mousquito"));

        if (hit != null)
        {
            hit.gameObject.SetActive(false);
            hit.tag = "Untagged";
            _remainingMousquitos.Remove(hit);
            _mousquitosTargetAnchors.Remove(hit);
            if (_remainingMousquitos.Count <= 0)
                StartCoroutine(CallFinishGame());
        }
    }

    private IEnumerator CallFinishGame()
    {
        yield return new WaitForSeconds(0.5f);
        FinishGame();
    }

    private void MousquitoBite()
    {
        _currentMousquitos = _remainingMousquitos[0];
        _mousquitosTargetAnchors[_currentMousquitos] = MoveAnchors(_currentMousquitos.GetComponent<RectTransform>(), _bitingPos[Random.Range(0, _bitingPos.Count)]);
        _currentMousquitos.tag = "Mousquito";
        _currentMousquitos.GetComponent<Animator>().enabled = false;
    }

    public override void InitializeGame()
    {
        foreach (var item in _mousquitos)
        {
            item.tag = "Mousquito";
            item.SetActive(true);
            item.GetComponent<Animator>().enabled = true;
        }
        _remainingMousquitos.Clear();
        _remainingMousquitos.AddRange(_mousquitos);

        _mousquitosTargetAnchors.Clear();
        foreach (var item in _remainingMousquitos)
            _mousquitosTargetAnchors.Add(item, MoveAnchors(item.GetComponent<RectTransform>()));

        _cursor.gameObject.SetActive(true);
        _gameCanvas.SetActive(true);
    }
}
