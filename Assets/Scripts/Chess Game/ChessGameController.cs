using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(PiecesCreator))]
public abstract class ChessGameController : MonoBehaviour
{
    protected const byte SET_GAME_STATE_EVENT_CODE = 1;

    [SerializeField] private BoardLayout startingBoardLayout;

    private ChessUIManager UIManager;
    private CameraSetup cameraSetup;
    private Board board;
    private PiecesCreator pieceCreator;
    protected ChessPlayer whitePlayer;
    protected ChessPlayer blackPlayer;
    protected ChessPlayer activePlayer;

    protected GameState state;

    protected abstract void SetGameState(GameState state);
    public abstract void TryToStartThisGame();
    public abstract bool CanPerformMove();

    private void Awake()
    {
        pieceCreator = GetComponent<PiecesCreator>();
    }

    public void SetDependencies(CameraSetup cameraSetup, ChessUIManager UIManager, Board board)
    {
        this.cameraSetup = cameraSetup;
        this.UIManager = UIManager;
        this.board = board;
    }

    public void InitializeGame()
    {
        CreatePlayers();
        board.SetDependencies(this);
    }

    public virtual void CreatePlayers()
    {
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    public void StartNewGame()
    {
        UIManager.OnGameStarted();
        SetGameState(GameState.Init);
        //board.SetDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        TryToStartThisGame();
    }


    public bool IsGameInProgress()
    {
        return state == GameState.Play;
    }

    private void CreatePiecesFromLayout(BoardLayout layout)
    {
        for (int i = 0; i < layout.GetPiecesCount(); i++)
        {
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);

            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }

    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type)
    {
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);

        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);
        if (team == TeamColor.White)
        {
            newPiece.transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    public void SetupCamera(TeamColor team)
    {
        cameraSetup.SetupCamera(team);
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer player)
    {
        player.GenerateAllPossibleMoves();
    }

    public bool IsTeamTurnActive(TeamColor team)
    {
        return activePlayer.team == team;
    }

    public virtual void EndTurn()
    {
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        if (CheckIfGameIsFinished())
        {
            EndGame();
        }
        else
        {
            ChangeActiveTeam();
        }
    }

    public bool CheckIfGameIsFinished()
    {
        if (!whitePlayer.activePieces.Any(piece => piece is King))
        {
            Debug.Log("Black wins! White King is taken.");
            return true;
        }

        if (!blackPlayer.activePieces.Any(piece => piece is King))
        {
            Debug.Log("White wins! Black King is taken.");
            return true;
        }

        if (IsCheckmate())
        {
            Debug.Log("Checkmate!");
            return true;
        }

        if (IsStalemate())
        {
            Debug.Log("Stalemate!");
            return true;
        }

        if (IsInsufficientMaterial())
        {
            Debug.Log("Insufficient Material!");
            return true;
        }

        // Check for threefold repetition
        //if (IsThreefoldRepetition()) return true;

        // Check for fifty-move rule
        //if (IsFiftyMoveRule()) return true;

        return false;
    }

    private bool IsCheckmate()
    {
        Piece[] kingAttackingPieces = activePlayer.GetPieceAtackingOppositePiceOfType<King>();
        if (kingAttackingPieces.Length > 0)
        {
            ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesOfType<King>().FirstOrDefault();
            oppositePlayer.RemoveMovesEnablingAttakOnPieceOfType<King>(activePlayer, attackedKing);

            int avaliableKingMoves = attackedKing.avaliableMoves.Count;
            if (avaliableKingMoves == 0)
            {
                bool canCoverKing = oppositePlayer.CanHidePieceFromAttack<King>(activePlayer);
                if (!canCoverKing)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool IsStalemate()
    {
        if (!CanMove(activePlayer))
        {
            Piece[] kingAttackingPieces = activePlayer.GetPieceAtackingOppositePiceOfType<King>();
            if (kingAttackingPieces.Length == 0)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsInsufficientMaterial()
    {
        if (whitePlayer.activePieces.Count == 1 && blackPlayer.activePieces.Count == 1)
        {
            return true;
        }

        if (whitePlayer.activePieces.Count == 1 && whitePlayer.GetPiecesOfType<King>().Count() == 1 && blackPlayer.activePieces.Count == 2 && blackPlayer.GetPiecesOfType<King>().Count() == 1 && blackPlayer.GetPiecesOfType<Bishop>().Count() == 1)
        {
            return true;
        }
        if (blackPlayer.activePieces.Count == 1 && blackPlayer.GetPiecesOfType<King>().Count() == 1 && whitePlayer.activePieces.Count == 2 && whitePlayer.GetPiecesOfType<King>().Count() == 1 && whitePlayer.GetPiecesOfType<Bishop>().Count() == 1)
        {
            return true;
        }

        if (whitePlayer.activePieces.Count == 1 && whitePlayer.GetPiecesOfType<King>().Count() == 1 && blackPlayer.activePieces.Count == 2 && blackPlayer.GetPiecesOfType<King>().Count() == 1 && blackPlayer.GetPiecesOfType<Knight>().Count() == 1)
        {
            return true;
        }
        if (blackPlayer.activePieces.Count == 1 && blackPlayer.GetPiecesOfType<King>().Count() == 1 && whitePlayer.activePieces.Count == 2 && whitePlayer.GetPiecesOfType<King>().Count() == 1 && whitePlayer.GetPiecesOfType<Knight>().Count() == 1)
        {
            return true;
        }

        if (whitePlayer.activePieces.Count == 2 && whitePlayer.GetPiecesOfType<King>().Count() == 1 && whitePlayer.GetPiecesOfType<Bishop>().Count() == 1 && blackPlayer.activePieces.Count == 2 && blackPlayer.GetPiecesOfType<King>().Count() == 1 && blackPlayer.GetPiecesOfType<Bishop>().Count() == 1)
        {
            Bishop whiteBishop = (Bishop)whitePlayer.GetPiecesOfType<Bishop>().FirstOrDefault();
            Bishop blackBishop = (Bishop)blackPlayer.GetPiecesOfType<Bishop>().FirstOrDefault();
            if ((whiteBishop.occupiedSquare.x + whiteBishop.occupiedSquare.y) % 2 == (blackBishop.occupiedSquare.x + blackBishop.occupiedSquare.y) % 2)
            {
                return true;
            }
        }

        return false;
    }

    private bool CanMove(ChessPlayer player)
    {
        foreach (Piece piece in player.activePieces)
        {
            if (board.HasPiece(piece))
            {
                piece.SelectAvaliableSquares();
                if (piece.avaliableMoves.Count > 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void EndGame()
    {
        TeamColor winner = (whitePlayer.activePieces.Any(piece => piece is King)) ? TeamColor.White : TeamColor.Black;
        UIManager.OnGameFinished(winner.ToString());
        SetGameState(GameState.Finished);
    }

    public void RestartGame()
    {
        DestroyPieces();
        board.OnGameRestarted();
        whitePlayer.OnGameRestarted();
        blackPlayer.OnGameRestarted();
        StartNewGame();
    }

    private void DestroyPieces()
    {
        whitePlayer.activePieces.ForEach(p => Destroy(p.gameObject));
        blackPlayer.activePieces.ForEach(p => Destroy(p.gameObject));
    }

    private void ChangeActiveTeam()
    {
        activePlayer = activePlayer == whitePlayer ? blackPlayer : whitePlayer;
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player)
    {
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    public void OnPieceRemoved(Piece piece)
    {
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
    }

    public void RemoveMovesEnablingAttakOnPieceOfType<T>(Piece piece) where T : Piece
    {
        activePlayer.RemoveMovesEnablingAttakOnPieceOfType<T>(GetOpponentToPlayer(activePlayer), piece);
    }
}