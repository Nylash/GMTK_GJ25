using UnityEngine;
using UnityEngine.UI;

public class WaterBottleGame : MiniGame
{
    [SerializeField] private Image _waterSprite;
    [SerializeField][Range(0f,1f)] private float _waterAmountDrinkPerAction;

    protected override void Awake()
    {
        base.Awake();

        _inputs.Player.Action.started += ctx => EmptyBottle();
    }

    private void EmptyBottle()
    {
        _waterSprite.fillAmount -= _waterAmountDrinkPerAction;
        if (_waterSprite.fillAmount <= 0.01f)
            FinishGame();
    }

    protected override void FinishGame()
    {
        base.FinishGame();
        PlayerManager.Instance.GainHealth();
    }

    public override void InitializeGame()
    {
        _waterSprite.fillAmount = 1;
        _gameCanvas.SetActive(true);
    }
}
