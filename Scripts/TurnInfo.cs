using Godot;
using System;

public class TurnInfo
{
    private int whosTurn = 0;
    private int whatTurn = 0;
    Label label;

    private int amountOfPlayers;
    public TurnInfo(){
    }
    public TurnInfo(int n, Label label)
	{
		this.label = label;
		amountOfPlayers = n;
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		label.SetText("Turn " + whatTurn + ". It is Player " + whosTurn + "'s turn.");
	}

	public void IncrementWhosTurn(){
        whosTurn = whosTurn + 1;
        if(whosTurn == amountOfPlayers){
            whosTurn = 0;
            IncrementWhatTurn();
        }
        UpdateLabel();
    }
    
    public void IncrementWhatTurn(){
        whatTurn++;
        UpdateLabel();
    }
    public int GetWhosTurn(){
        return whosTurn;
    }
    
    public int GetWhatTurn(){
        return whatTurn;
    }
    public int GetAmountOfPlayers(){
        return amountOfPlayers;
    }
}
