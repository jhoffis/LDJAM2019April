using Godot;
using System;

public class Player
{

	public int playernr { get; 
	//private
	set; }
	Label label;
	public Player()
	{
	}

	public Player(int n, Label label)
	{
		this.label = label;
		playernr = n;
		label.SetText("You are Player " + playernr);
	}
}
