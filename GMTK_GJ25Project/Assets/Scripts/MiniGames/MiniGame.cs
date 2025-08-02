using UnityEngine;

public abstract class MiniGame : MonoBehaviour
{
    [SerializeField] protected GameObject _gameCanvas;

    protected InputSystem_Actions _inputs;

    private void OnEnable() => _inputs.Player.Enable();
    private void OnDisable() => _inputs.Player.Disable();

    protected virtual void Awake()
    {
        _inputs = new InputSystem_Actions();
    }

    public abstract void InitializeGame();

    protected virtual void FinishGame()
    {
        _gameCanvas.SetActive(false);
        UIManager.Instance.InMiniGame = false;
    }
}
