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

    [Header("Boosts")]
    [SerializeField] public Button lineCleanerButton;
    [SerializeField] public TMP_Text lineCleanerText;
    [SerializeField] int lineCleanerQuantity;
    public bool InLineCleanerMode { get; set; }


    private void Awake()
    {
        gameController = new GameController();
        boardView.onTileClick += OnTileClick;
    }

    private void Start()
    {
        List<List<Tile>> board = gameController.StartGame(boardWidth, boardHeight);
        boardView.CreateBoard(board);
        InLineCleanerMode = false;
        lineCleanerText.text = lineCleanerQuantity.ToString();
    }

    private int selectedX, selectedY = -1;

    private bool isAnimating;

    private void OnTileClick(int x, int y)
    {
        if (isAnimating) return;

        if (InLineCleanerMode)
        {
            isAnimating = true;
            List<BoardSequence> swapResult = gameController.SwapTile(x, y, x, y, true);

            AnimateBoard(swapResult, 0, () => isAnimating = false);

            selectedX = -1;
            selectedY = -1;
            ToggleLineCleanerMode(false);
            UseLineCleaner();
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

        sequence.Append(boardView.DestroyTiles(boardSequence.matchedPosition)).OnStart(() => PunctuationManager.Instance.ShowPointsAdded((boardSequence.numberOfMatchedTiles * 10) *
            boardSequence.comboIndex));
        sequence.Append(boardView.MoveTiles(boardSequence.movedTiles));
        sequence.Append(boardView.CreateTile(boardSequence.addedTiles));

        i++;
        if (i < boardSequences.Count)
        {
            sequence.onComplete += () => AnimateBoard(boardSequences, i, onComplete);
        }
        else
        {
            sequence.onComplete += () => onComplete();
        }
    }

    public void ToggleLineCleanerMode(bool activate)
    {
        InLineCleanerMode = !InLineCleanerMode;
        Debug.Log("Line Cleaner mode is on: " + InLineCleanerMode);

        if (InLineCleanerMode)
            lineCleanerButton.image.sprite = lineCleanerButton.spriteState.selectedSprite;
        else
            lineCleanerButton.image.sprite = lineCleanerButton.spriteState.highlightedSprite;
    }

    public void UseLineCleaner()
    {
        lineCleanerQuantity--;
        UpdateLineCleanerText();
    }

    private void UpdateLineCleanerText()
    {
        if (lineCleanerQuantity == 0)
            lineCleanerButton.interactable = false;
        else
            lineCleanerButton.interactable = true;

        lineCleanerText.text = lineCleanerQuantity.ToString();
    }
}
