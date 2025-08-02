using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaterBottleGame : MiniGame
{
    [SerializeField] private Image _waterSprite;
    [SerializeField] private Animator _armAnimator;
    [SerializeField][Range(0f,1f)] private float _waterAmountDrinkPerAction;

    Sprite _initialSprite;

    protected override void Awake()
    {
        base.Awake();

        _inputs.Player.Action.started += ctx => EmptyBottle();

        _initialSprite = _armAnimator.GetComponent<Image>().sprite;
    }

    private void EmptyBottle()
    {
        _waterSprite.fillAmount -= _waterAmountDrinkPerAction;
        if (!_armAnimator.GetBool("Drink"))
            _armAnimator.SetBool("Drink", true);
        if (_waterSprite.fillAmount <= 0.05f)
            FinishGame();
        StartCoroutine(ResetAnim());
    }

    private IEnumerator ResetAnim()
    {
        yield return new WaitForEndOfFrame();
        _armAnimator.SetBool("Drink", false);
    }

    protected override void FinishGame()
    {
        _armAnimator.SetBool("Drink", false);
        base.FinishGame();
        PlayerManager.Instance.GainHealth();
        _armAnimator.GetComponent<Image>().sprite = _initialSprite;
    }

    public override void InitializeGame()
    {
        _waterSprite.fillAmount = 1;
        _gameCanvas.SetActive(true);
    }
}
