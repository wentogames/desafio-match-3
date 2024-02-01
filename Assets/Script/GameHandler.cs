using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    [SerializeField] private GameController gameController;

    [SerializeField] public int boardWidth = 10;

    [SerializeField] public int boardHeight = 10;

    [SerializeField] public BoardView boardView;

    [Header("Boosts - Line Cleaner")]
    [SerializeField] public Button lineCleanerButton;
    [SerializeField] public Sprite lineCleanerInactiveSprite;
    [SerializeField] public Sprite lineCleanerActiveSprite;
    [SerializeField] public TMP_Text lineCleanerText;
    [SerializeField] int lineCleanerQuantity;

    [Header("Boosts - Block Explosion")]
    [SerializeField] public Button blockExplosionButton;
    [SerializeField] public Sprite blockExplosionInactiveSprite;
    [SerializeField] public Sprite blockExplosionActiveSprite;
    [SerializeField] public TMP_Text blockExplosionText;
    [SerializeField] int blockExplosionQuantity;

    [Header("Boosts - ColorCleaner")]
    [SerializeField] public Button colorCleanerButton;
    [SerializeField] public Sprite colorCleanerInactiveSprite;
    [SerializeField] public Sprite colorCleanerActiveSprite;
    [SerializeField] public TMP_Text colorCleanerText;
    [SerializeField] int colorCleanerQuantity;

    [Header("Boosts")]
    [SerializeField] int comboQuantity;

    public bool InLineCleanerMode { get; set; }
    public bool InBlockExplosionMode { get; set; }
    public bool InColorCleanerMode { get; set; }


    private void Awake()
    {
        gameController = new GameController();
        boardView.onTileClick += OnTileClick;
    }

    private void Start()
    {
        List<List<Tile>> board = gameController.StartGame(boardWidth, boardHeight);
        boardView.CreateBoard(board);
        ToggleBoost((int)BoostMode.None);
        UpdateLineCleanerText(BoostMode.None);
    }

    private int selectedX, selectedY = -1;

    private bool isAnimating;

    private void OnTileClick(int x, int y)
    {
        if (isAnimating) return;

        if (InLineCleanerMode)
        {
            isAnimating = true;
            List<BoardSequence> swapResult = gameController.SwapTile(x, y, x, y, BoostMode.LineCleaner);

            AnimateBoard(swapResult, 0, () => isAnimating = false);

            selectedX = -1;
            selectedY = -1;
            ToggleBoost((int)BoostMode.LineCleaner);
            UpdateLineCleanerText(BoostMode.LineCleaner);
            ToggleBoost((int)BoostMode.None);
        }
        else if (InBlockExplosionMode)
        {
            isAnimating = true;
            List<BoardSequence> swapResult = gameController.SwapTile(x, y, x, y, BoostMode.BlockExplosion);

            AnimateBoard(swapResult, 0, () => isAnimating = false);

            selectedX = -1;
            selectedY = -1;
            ToggleBoost((int)BoostMode.BlockExplosion);
            UpdateLineCleanerText(BoostMode.BlockExplosion);
            ToggleBoost((int)BoostMode.None);
        }
        else if (InColorCleanerMode)
        {
            isAnimating = true;
            List<BoardSequence> swapResult = gameController.SwapTile(x, y, x, y, BoostMode.ColorCleaner);

            AnimateBoard(swapResult, 0, () => isAnimating = false);

            selectedX = -1;
            selectedY = -1;
            ToggleBoost((int)BoostMode.ColorCleaner);
            UpdateLineCleanerText(BoostMode.ColorCleaner);
            ToggleBoost((int)BoostMode.None);
        }
        else
        {
            if (selectedX > -1 && selectedY > -1)
            {
                if (Mathf.Abs(selectedX - x) + Mathf.Abs(selectedY - y) > 1)
                {
                    selectedX = -1;
                    selectedY = -1;
                }
                else
                {
                    isAnimating = true;
                    boardView.SwapTiles(selectedX, selectedY, x, y).onComplete += () =>
                    {
                        bool isValid = gameController.IsValidMovement(selectedX, selectedY, x, y);
                        if (!isValid)
                        {
                            boardView.SwapTiles(x, y, selectedX, selectedY)
                            .onComplete += () => isAnimating = false;
                        }
                        else
                        {
                            List<BoardSequence> swapResult = gameController.SwapTile(selectedX, selectedY, x, y);

                            AnimateBoard(swapResult, 0, () => isAnimating = false);
                        }

                        selectedX = -1;
                        selectedY = -1;
                    };
                }
            }
            else
            {
                selectedX = x;
                selectedY = y;
            }
        }
    }

    private void AnimateBoard(List<BoardSequence> boardSequences, int i, Action onComplete)
    {
        Sequence sequence = DOTween.Sequence();

        BoardSequence boardSequence = boardSequences[i];
        if(i == 0)
            PunctuationManager.Instance.ResetClipPitch();

        sequence.Append(boardView.DestroyTiles(boardSequence.matchedPosition)).OnStart(() =>
            PunctuationManager.Instance.ShowPointsAdded((boardSequence.numberOfMatchedTiles * 10) *
                boardSequence.comboIndex));
        sequence.Append(boardView.MoveTiles(boardSequence.movedTiles));
        sequence.Append(boardView.CreateTile(boardSequence.addedTiles));

        i++;
        if (i < boardSequences.Count)
        {
            //Every multiple of 5 gives the user a random boost
            if (boardSequence.comboIndex % comboQuantity == 0)
                EarnExtraBoost();
            sequence.onComplete += () => AnimateBoard(boardSequences, i, onComplete);
        }
        else
        {
            ToggleBoost((int)BoostMode.None);
            sequence.onComplete += () => onComplete();
        }
    }

    private void EarnExtraBoost()
    {
        int boostIndex = UnityEngine.Random.Range(0, 3);

        switch (boostIndex)
        {
            case 0:
                lineCleanerQuantity++;
                lineCleanerText.DOColor(Color.cyan, 1f).OnComplete(() => lineCleanerText.DOColor(Color.white, 1f));
                break;
            case 1:
                blockExplosionQuantity++;
                blockExplosionText.DOColor(Color.cyan, 1f).OnComplete(() => blockExplosionText.DOColor(Color.white, 1f));
                break;
            default:
                colorCleanerQuantity++;
                colorCleanerText.DOColor(Color.cyan, 1f).OnComplete(() => colorCleanerText.DOColor(Color.white, 1f));
                break;
        }
        UpdateLineCleanerText(BoostMode.None);
    }

    public void ToggleBoost(int mode)
    {
        switch ((BoostMode)mode)
        {
            case BoostMode.LineCleaner:
                if (lineCleanerQuantity > 0 && !isAnimating)
                {
                    InLineCleanerMode = true;
                    lineCleanerButton.image.sprite = lineCleanerActiveSprite;
                }
                break;
            case BoostMode.BlockExplosion:
                if (blockExplosionQuantity > 0 && !isAnimating)
                {
                    InBlockExplosionMode = true;
                    blockExplosionButton.image.sprite = blockExplosionActiveSprite;
                }
                break;
            case BoostMode.ColorCleaner:
                if (colorCleanerQuantity > 0 && !isAnimating)
                {
                    InColorCleanerMode = true;
                    colorCleanerButton.image.sprite = colorCleanerActiveSprite;
                }
                break;
            default:
                InLineCleanerMode = false;
                InBlockExplosionMode = false;
                InColorCleanerMode = false;

                lineCleanerButton.image.sprite = lineCleanerInactiveSprite;
                blockExplosionButton.image.sprite = blockExplosionInactiveSprite;
                colorCleanerButton.image.sprite = colorCleanerInactiveSprite;
                break;
        }
    }

    private void UpdateLineCleanerText(BoostMode mode)
    {
        switch (mode)
        {
            case BoostMode.LineCleaner:
                lineCleanerQuantity--;
                if (lineCleanerQuantity == 0)
                    lineCleanerButton.interactable = false;
                else
                    lineCleanerButton.interactable = true;
                lineCleanerText.text = lineCleanerQuantity.ToString();
                break;
            case BoostMode.BlockExplosion:
                blockExplosionQuantity--;
                if (blockExplosionQuantity == 0)
                    blockExplosionButton.interactable = false;
                else
                    blockExplosionButton.interactable = true;
                blockExplosionText.text = blockExplosionQuantity.ToString();
                break;
            case BoostMode.ColorCleaner:
                colorCleanerQuantity--;
                if (colorCleanerQuantity == 0)
                    colorCleanerButton.interactable = false;
                else
                    colorCleanerButton.interactable = true;
                colorCleanerText.text = colorCleanerQuantity.ToString();
                break;
            default:
                lineCleanerText.text = lineCleanerQuantity.ToString();
                blockExplosionText.text = blockExplosionQuantity.ToString();
                colorCleanerText.text = colorCleanerQuantity.ToString();
                break;
        }
    }
}
