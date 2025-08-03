using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WaterBottleGame : MiniGame
{
    [SerializeField] private Image _waterSprite;
    [SerializeField] private Animator _armAnimator;
    [SerializeField][Range(0f,1f)] private float _waterAmountDrinkPerAction;

    Sprite _initialSprite;
    bool _gameEnded;

    protected override void Awake()
    {
        base.Awake();

        _inputs.Player.MouseAction.started += ctx => EmptyBottle();

        _initialSprite = _armAnimator.GetComponent<Image>().sprite;
    }

    private void EmptyBottle()
    {
        if (PlayerManager.Instance.gamePaused) return;

        if (!_gameCanvas.activeSelf) return;

        _waterSprite.fillAmount -= _waterAmountDrinkPerAction;
        if (!_armAnimator.GetBool("Drink"))
            _armAnimator.SetBool("Drink", true);
        if (_waterSprite.fillAmount <= 0.05f)
        {
            if (!_gameEnded)
            {
                _gameEnded = true;
                StartCoroutine(CallFinishGame());
            }
        }
        StartCoroutine(ResetAnim());
    }

    private IEnumerator CallFinishGame()
    {
        yield return new WaitForSeconds(0.5f);
        FinishGame();
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
        base.InitializeGame();
        _gameEnded = false;
        _waterSprite.fillAmount = 1;
        _gameCanvas.SetActive(true);
    }
}
