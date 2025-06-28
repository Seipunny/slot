using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerDiceLogic : DiceLogicBase
{
    public override bool CanTakeTurn()
    {
        return gameManager.CanPlayerTakeTurn();
    }

    protected override void OnTurnEnd(int balance)
    {
        Debug.Log($"PlayerDiceLogic OnTurnEnd called with balance: {balance}");
        gameManager.OnPlayerTurnEnd(balance);
    }
}
