using System.Collections.Generic;
using UnityEngine;

public class GameController
{
    private List<List<Tile>> _boardTiles;
    private List<int> _tilesTypes;
    private int _tileCount;

    public List<List<Tile>> StartGame(int boardWidth, int boardHeight)
    {
        _tilesTypes = new List<int> { 0, 1, 2, 3 };
        _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);
        return _boardTiles;
    }

    public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        Tile switchedTile = newBoard[fromY][fromX];
        newBoard[fromY][fromX] = newBoard[toY][toX];
        newBoard[toY][toX] = switchedTile;

        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (x > 1
                    && newBoard[y][x].type == newBoard[y][x - 1].type
                    && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
                {
                    return true;
                }
                if (y > 1
                    && newBoard[y][x].type == newBoard[y - 1][x].type
                    && newBoard[y - 1][x].type == newBoard[y - 2][x].type)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        Tile switchedTile = newBoard[fromY][fromX];
        newBoard[fromY][fromX] = newBoard[toY][toX];
        newBoard[toY][toX] = switchedTile;

        List<BoardSequence> boardSequences = new List<BoardSequence>();
        List<List<bool>> matchedTiles;
        int comboIndex = 0;
        int numberOfMatchedTiles = 0;

        while (HasMatch(matchedTiles = FindMatches(newBoard)))
        {
            //Cleaning the matched tiles
            List<Vector2Int> matchedPosition = new List<Vector2Int>();

            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
                {
                    if (matchedTiles[y][x])
                    {
                        matchedPosition.Add(new Vector2Int(x, y));
                        newBoard[y][x] = new Tile { id = -1, type = -1 };
                        Debug.Log("combo " + comboIndex + ", number of matched tiles " + numberOfMatchedTiles);
                        numberOfMatchedTiles++;
                    }
                }
            }
            comboIndex++;

            // Dropping the tiles
            Dictionary<int, MovedTileInfo> movedTiles = new Dictionary<int, MovedTileInfo>();
            List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();
            for (int i = 0; i < matchedPosition.Count; i++)
            {
                int x = matchedPosition[i].x;
                int y = matchedPosition[i].y;
                if (y > 0)
                {
                    for (int j = y; j > 0; j--)
                    {
                        Tile movedTile = newBoard[j - 1][x];
                        newBoard[j][x] = movedTile;
                        if (movedTile.type > -1)
                        {
                            if (movedTiles.ContainsKey(movedTile.id))
                            {
                                movedTiles[movedTile.id].to = new Vector2Int(x, j);
                            }
                            else
                            {
                                MovedTileInfo movedTileInfo = new MovedTileInfo
                                {
                                    from = new Vector2Int(x, j - 1),
                                    to = new Vector2Int(x, j)
                                };
                                movedTiles.Add(movedTile.id, movedTileInfo);
                                movedTilesList.Add(movedTileInfo);
                            }
                        }
                    }

                    newBoard[0][x] = new Tile
                    {
                        id = -1,
                        type = -1
                    };
                }
            }

            // Filling the board
            List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
            for (int y = newBoard.Count - 1; y > -1; y--)
            {
                for (int x = newBoard[y].Count - 1; x > -1; x--)
                {
                    if (newBoard[y][x].type == -1)
                    {
                        int tileType = Random.Range(0, _tilesTypes.Count);
                        Tile tile = newBoard[y][x];
                        tile.id = _tileCount++;
                        tile.type = _tilesTypes[tileType];
                        addedTiles.Add(new AddedTileInfo
                        {
                            position = new Vector2Int(x, y),
                            type = tile.type
                        });
                    }
                }
            }

            BoardSequence sequence = new BoardSequence
            {
                matchedPosition = matchedPosition,
                movedTiles = movedTilesList,
                addedTiles = addedTiles,
                numberOfMatchedTiles = numberOfMatchedTiles,
                comboIndex = comboIndex
            };
            boardSequences.Add(sequence);
            numberOfMatchedTiles = 0;
        }

        _boardTiles = newBoard;

        return boardSequences;
        //return _boardTiles;
    }

    private static bool HasMatch(List<List<bool>> list)
    {
        for (int y = 0; y < list.Count; y++)
            for (int x = 0; x < list[y].Count; x++)
                if (list[y][x])
                    return true;
        return false;
    }

    private static List<List<bool>> FindMatches(List<List<Tile>> newBoard)
    {
        List<List<bool>> matchedTiles = new List<List<bool>>();
        for (int y = 0; y < newBoard.Count; y++)
        {
            matchedTiles.Add(new List<bool>(newBoard[y].Count));
            for (int x = 0; x < newBoard.Count; x++)
            {
                matchedTiles[y].Add(false);
            }
        }

        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (x > 1
                    && newBoard[y][x].type == newBoard[y][x - 1].type
                    && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
                {
                    matchedTiles[y][x] = true;
                    matchedTiles[y][x - 1] = true;
                    matchedTiles[y][x - 2] = true;
                }
                if (y > 1
                    && newBoard[y][x].type == newBoard[y - 1][x].type
                    && newBoard[y - 1][x].type == newBoard[y - 2][x].type)
                {
                    matchedTiles[y][x] = true;
                    matchedTiles[y - 1][x] = true;
                    matchedTiles[y - 2][x] = true;
                }
            }
        }

        int multiplier = 0;
        for(int i = 0; i < matchedTiles.Count; i++)
        {
            for(int j = 0; j < matchedTiles[i].Count; j++)
            {
                if (matchedTiles[i][j] == true)
                    multiplier++;
            }
        }

        return matchedTiles;
    }

    private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
    {
        List<List<Tile>> newBoard = new List<List<Tile>>(boardToCopy.Count);
        for (int y = 0; y < boardToCopy.Count; y++)
        {
            newBoard.Add(new List<Tile>(boardToCopy[y].Count));
            for (int x = 0; x < boardToCopy[y].Count; x++)
            {
                Tile tile = boardToCopy[y][x];
                newBoard[y].Add(new Tile { id = tile.id, type = tile.type });
            }
        }

        return newBoard;
    }

    private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
    {
        List<List<Tile>> board = new List<List<Tile>>(height);
        _tileCount = 0;
        for (int y = 0; y < height; y++)
        {
            board.Add(new List<Tile>(width));
            for (int x = 0; x < width; x++)
            {
                board[y].Add(new Tile { id = -1, type = -1 });
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                List<int> noMatchTypes = new List<int>(tileTypes.Count);
                for (int i = 0; i < tileTypes.Count; i++)
                {
                    noMatchTypes.Add(_tilesTypes[i]);
                }

                if (x > 1
                    && board[y][x - 1].type == board[y][x - 2].type)
                {
                    noMatchTypes.Remove(board[y][x - 1].type);
                }
                if (y > 1
                    && board[y - 1][x].type == board[y - 2][x].type)
                {
                    noMatchTypes.Remove(board[y - 1][x].type);
                }

                board[y][x].id = _tileCount++;
                board[y][x].type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
            }
        }

        return board;
    }
}
