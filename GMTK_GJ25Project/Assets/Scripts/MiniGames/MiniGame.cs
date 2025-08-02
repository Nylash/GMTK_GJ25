using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] protected GameObject _gameCanvas;
    [SerializeField] private List<Sprite> _sprites;

    protected InputSystem_Actions _inputs;

    private void OnEnable() => _inputs.Player.Enable();
    private void OnDisable() => _inputs.Player.Disable();

    protected virtual void Awake()
    {
        _inputs = new InputSystem_Actions();
    }

    public virtual void InitializeGame()
    {
        _gameCanvas.GetComponent<Image>().sprite = _sprites[Random.Range(0, _sprites.Count)];
    }

    protected virtual void FinishGame()
    {
        _gameCanvas.SetActive(false);
        UIManager.Instance.InMiniGame = false;
    }
}
