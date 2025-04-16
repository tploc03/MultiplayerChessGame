using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChessBot
{
    private readonly ChessPlayer botPlayer;
    private readonly ChessPlayer humanPlayer;

    public ChessBot(ChessPlayer botPlayer, ChessPlayer humanPlayer)
    {
        this.botPlayer = botPlayer;
        this.humanPlayer = humanPlayer;
    }

    public void MakeMove()
    {
        var pieces = botPlayer.activePieces.Where(p => p.avaliableMoves.Count > 0).ToList();
        if (pieces.Count == 0) return;

        List<(Piece, Vector2Int, int)> scoredMoves = new List<(Piece, Vector2Int, int)>();
        bool isEndgame = ChessBotEvaluator.IsEndgame(botPlayer.board);

        foreach (var piece in pieces)
        {
            foreach (var move in piece.avaliableMoves)
            {
                int score = ChessBotEvaluator.EvaluateMove(move, piece, botPlayer.board, isEndgame);
                scoredMoves.Add((piece, move, score));
            }
        }

        scoredMoves = scoredMoves.OrderByDescending(m => m.Item3).ToList();

        if (scoredMoves.Count == 0) return;

        int topN = Mathf.Min(3, scoredMoves.Count);
        var topMoves = scoredMoves.Take(topN).ToList();

        var chosenMove = topMoves[UnityEngine.Random.Range(0, topMoves.Count)];

        botPlayer.board.OnSetSelectedPiece(chosenMove.Item1.occupiedSquare);
        botPlayer.board.OnSelectedPieceMoved(chosenMove.Item2);
    }

}