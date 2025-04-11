using System.Collections.Generic;
using UnityEngine;

public static class ChessBotEvaluator
{
    private static readonly Dictionary<System.Type, int> PieceValues = new Dictionary<System.Type, int>()
    {
        { typeof(Pawn), 100 },
        { typeof(Knight), 320 },
        { typeof(Bishop), 330 },
        { typeof(Rook), 500 },
        { typeof(Queen), 900 },
        { typeof(King), 20000 }
    };

    private static readonly int[,] PawnPositionScores = new int[8, 8]
    {
        {0,  0,  0,  0,  0,  0,  0,  0},
        {50, 50, 50, 50, 50, 50, 50, 50},
        {10, 10, 20, 30, 30, 20, 10, 10},
        {5,  5, 10, 25, 25, 10,  5,  5},
        {0,  0,  0, 20, 20,  0,  0,  0},
        {5, -5,-10,  0,  0,-10, -5,  5},
        {5, 10, 10,-20,-20, 10, 10,  5},
        {0,  0,  0,  0,  0,  0,  0,  0}
    };

    private static readonly int[,] KnightPositionScores = new int[8, 8]
    {
        {-50,-40,-30,-30,-30,-30,-40,-50},
        {-40,-20,  0,  0,  0,  0,-20,-40},
        {-30,  0, 10, 15, 15, 10,  0,-30},
        {-30,  5, 15, 20, 20, 15,  5,-30},
        {-30,  0, 15, 20, 20, 15,  0,-30},
        {-30,  5, 10, 15, 15, 10,  5,-30},
        {-40,-20,  0,  5,  5,  0,-20,-40},
        {-50,-40,-30,-30,-30,-30,-40,-50}
    };

    private static readonly int[,] BishopPositionScores = new int[8, 8]
   {
        {-20,-10,-10,-10,-10,-10,-10,-20},
        {-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5, 10, 10,  5,  0,-10},
        {-10,  5,  5, 10, 10,  5,  5,-10},
        {-10,  0, 10, 10, 10, 10,  0,-10},
        {-10, 10, 10, 10, 10, 10, 10,-10},
        {-10,  5,  0,  0,  0,  0,  5,-10},
        {-20,-10,-10,-10,-10,-10,-10,-20}
   };

    private static readonly int[,] RookPositionScores = new int[8, 8]
    {
        {  0,  0,  0,  0,  0,  0,  0,  0},
        {  5, 10, 10, 10, 10, 10, 10,  5},
        { -5,  0,  0,  0,  0,  0,  0, -5},
        { -5,  0,  0,  0,  0,  0,  0, -5},
        { -5,  0,  0,  0,  0,  0,  0, -5},
        { -5,  0,  0,  0,  0,  0,  0, -5},
        { -5,  0,  0,  0,  0,  0,  0, -5},
        {  0,  0,  0,  5,  5,  0,  0,  0}
    };

    private static readonly int[,] QueenPositionScores = new int[8, 8]
    {
        {-20,-10,-10, -5, -5,-10,-10,-20},
        {-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5,  5,  5,  5,  0,-10},
        { -5,  0,  5,  5,  5,  5,  0, -5},
        {  0,  0,  5,  5,  5,  5,  0, -5},
        {-10,  5,  5,  5,  5,  5,  0,-10},
        {-10,  0,  5,  0,  0,  0,  0,-10},
        {-20,-10,-10, -5, -5,-10,-10,-20}
    };

    private static readonly int[,] KingPositionScoresEarly = new int[8, 8]
    {
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},
        {-20,-30,-30,-40,-40,-30,-30,-20},
        {-10,-20,-20,-20,-20,-20,-20,-10},
        { 20, 20,  0,  0,  0,  0, 20, 20},
        { 20, 30, 10,  0,  0, 10, 30, 20}
    };

    private static readonly int[,] KingPositionScoresEndgame = new int[8, 8]
    {
        {-50,-40,-30,-20,-20,-30,-40,-50},
        {-30,-20,-10,  0,  0,-10,-20,-30},
        {-30,-10, 20, 30, 30, 20,-10,-30},
        {-30,-10, 30, 40, 40, 30,-10,-30},
        {-30,-10, 30, 40, 40, 30,-10,-30},
        {-30,-10, 20, 30, 30, 20,-10,-30},
        {-30,-30,  0,  0,  0,  0,-30,-30},
        {-50,-30,-30,-30,-30,-30,-30,-50}
    };

    public static int EvaluateMove(Vector2Int moveCoords, Piece piece, Board board, bool isEndgame)
    {
        int score = 0;

        score += PieceValues[piece.GetType()];

        var targetPiece = board.GetPieceOnSquare(moveCoords);
        if (targetPiece != null && !targetPiece.IsFromSameTeam(piece))
        {
            score += PieceValues[targetPiece.GetType()];

            // Encourage taking higher value pieces
            if (PieceValues[targetPiece.GetType()] > PieceValues[piece.GetType()])
            {
                score += 50;
            }
        }

        // Positional Scores
        if (piece is Pawn)
        {
            score += PawnPositionScores[moveCoords.x, moveCoords.y];
        }
        else if (piece is Knight)
        {
            score += KnightPositionScores[moveCoords.x, moveCoords.y];
        }
        else if (piece is Bishop)
        {
            score += BishopPositionScores[moveCoords.x, moveCoords.y];
        }
        else if (piece is Rook)
        {
            score += RookPositionScores[moveCoords.x, moveCoords.y];
        }
        else if (piece is Queen)
        {
            score += QueenPositionScores[moveCoords.x, moveCoords.y];
        }
        else if (piece is King)
        {
            score += isEndgame ? KingPositionScoresEndgame[moveCoords.x, moveCoords.y] : KingPositionScoresEarly[moveCoords.x, moveCoords.y];
        }

        // Center control
        if (moveCoords.x >= 2 && moveCoords.x <= 5 && moveCoords.y >= 2 && moveCoords.y <= 5)
        {
            score += 20;
            if (moveCoords.x >= 3 && moveCoords.x <= 4 && moveCoords.y >= 3 && moveCoords.y <= 4)
            {
                score += 10;
            }
        }

        // Penalize being under attack
        if (IsSquareUnderAttack(moveCoords, piece.team, board))
        {
            score -= PieceValues[piece.GetType()] / 3;
        }

        // Reward protecting pieces
        if (IsProtectingPiece(moveCoords, piece, board))
        {
            score += 30;
        }

        return score;
    }

    private static bool IsSquareUnderAttack(Vector2Int square, TeamColor team, Board board)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                Piece piece = board.GetPieceOnSquare(new Vector2Int(x, y));
                if (piece != null && piece.team != team)
                {
                    piece.SelectAvaliableSquares(); // Ensure moves are up-to-date
                    if (piece.avaliableMoves.Contains(square))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private static bool IsProtectingPiece(Vector2Int moveCoords, Piece piece, Board board)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int adjSquare = moveCoords + new Vector2Int(x, y);
                if (!board.CheckIfCoordinatesAreOnBoard(adjSquare)) continue;

                Piece adjPiece = board.GetPieceOnSquare(adjSquare);
                if (adjPiece != null && adjPiece.IsFromSameTeam(piece))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool IsEndgame(Board board)
    {
        int pieceCount = 0;
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (board.GetPieceOnSquare(new Vector2Int(x, y)) != null)
                {
                    pieceCount++;
                }
            }
        }
        // Adjust the threshold as needed
        return pieceCount <= 10;
    }
}