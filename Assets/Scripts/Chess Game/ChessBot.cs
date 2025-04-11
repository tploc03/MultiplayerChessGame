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

        Piece selectedPiece = null;
        Vector2Int bestMove = Vector2Int.zero;
        int bestScore = int.MinValue;
        bool isEndgame = ChessBotEvaluator.IsEndgame(botPlayer.board);

        foreach (var piece in pieces)
        {
            foreach (var move in piece.avaliableMoves)
            {
                int score = ChessBotEvaluator.EvaluateMove(move, piece, botPlayer.board, isEndgame);
                if (score > bestScore)
                {
                    bestScore = score;
                    selectedPiece = piece;
                    bestMove = move;
                }
            }
        }

        if (selectedPiece != null)
        {
            var board = botPlayer.board;
            board.OnSetSelectedPiece(selectedPiece.occupiedSquare);
            board.OnSelectedPieceMoved(bestMove);
        }
    }
}