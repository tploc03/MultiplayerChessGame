using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleplayerChessGameController : ChessGameController
{
    private ChessBot bot;
    private bool isPlayerTurn = true;

    public override void CreatePlayers()
    {
        base.CreatePlayers();
        bot = new ChessBot(blackPlayer, whitePlayer);
    }

    protected override void SetGameState(GameState state)
    {
        this.state = state;
    }

    public override void TryToStartThisGame()
    {
        SetGameState(GameState.Play);
        isPlayerTurn = true;
    }

    public override bool CanPerformMove()
    {
        if (!IsGameInProgress() || !isPlayerTurn)
            return false;
        return true;
    }

    public override void EndTurn()
    {
        base.EndTurn();

        if (IsGameInProgress())
        {
            isPlayerTurn = !isPlayerTurn;

            if (!isPlayerTurn)
            {
                // Bot's turn
                StartCoroutine(BotTurn());
            }
        }
    }

    private IEnumerator BotTurn()
    {
        yield return new WaitForSeconds(0.8f);

        bot.MakeMove();
    }
}