using Godot;
using System;

public class Map : Node2D
{
	BorderHandler borderHandler;
	Camera2D cam;
	Node2D root;
	public override void _Ready()
	{
		root = (Node2D) GetTree().GetRoot().GetNode("GameHandler");
		cam = (Camera2D)root.FindNode("CamMv").FindNode("Cam");
		borderHandler = (BorderHandler) FindNode("Border");
		borderHandler.Init(3);
	}

}
