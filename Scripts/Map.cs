using Godot;
using System;

public class Map : Node2D
{
	BorderHandler borderHandler;
	Camera2D cam;
	Sprite sky;
	Node2D root;
	public static Player player;
	TurnInfo turnInfo;
	public override void _Ready()
	{
		root = (Node2D)GetTree().GetRoot().GetNode("GameHandler");
		cam = (Camera2D)root.FindNode("CamMv").FindNode("Cam");
		sky = (Sprite)root.FindNode("CamMv").FindNode("Sky");
		borderHandler = (BorderHandler)FindNode("Border");
		root.FindNode("UI").FindNode("NextTurn").Connect("pressed", this, nameof(_on_next_turn));
		SetProcessInput(true);

		player = new Player(0, (Label)root.FindNode("UI").FindNode("Player"));
		turnInfo = new TurnInfo(2, (Label)root.FindNode("UI").FindNode("CurrentPlayer"));

		Color[] teamColors = new Color[8];
		teamColors[0] = new Color(51 / 255f, 102 / 255f, 204 / 255f, 1);
		teamColors[1] = new Color(204 / 255f, 51 / 255f, 51 / 255f, 1);
		teamColors[2] = new Color(0f / 255f, 204 / 255f, 51 / 255f, 1);
		teamColors[3] = new Color(230 / 255f, 230 / 255f, 0 / 255f, 1);

		teamColors[4] = new Color(51 / 255f, 204 / 255f, 204 / 255f, 1);
		teamColors[5] = new Color(204 / 255f, 0 / 255f, 204 / 255f, 1);
		teamColors[6] = new Color(204 / 255f, 204 / 255f, 204 / 255f, 1);
		teamColors[7] = new Color(255f / 255f, 153 / 255f, 51 / 255f, 1);

		borderHandler.Init(turnInfo.GetAmountOfPlayers(), teamColors);
		ZoomHandle();
	}

	float zspd = 1.05f;
	float zlvl = 1;
	bool clicked = false;
	int distance = 1;
	public static bool dragging = false;
	Vector2 clickedPos;
	Vector2 camPos;
	public static int mousePointingNation = -1;
	public static int mousePointingDegree = 45;
	private bool conquest;

	public override void _Input(InputEvent @event)
	{
		InputEventMouseButton key = null;
		if (@event is InputEventMouseButton eventKey)
		{
			/*
            Lagre hvor du trykker og om du fortsatt holder nede og plasseringen er x vekke fra originalpunkt 
            så registrer det som dragging og ta ikke imot noen andre klikk. slett når man releaser.
            */

			key = eventKey;
			//Bestemm om du klikker eller ei:
			if (eventKey.IsPressed() && eventKey.GetButtonIndex() == 1)
			{
				if (!clicked)
				{
					clickedPos = eventKey.GetPosition();
					clicked = true;
				}
			}
			else
			{
				clicked = false;
				dragging = false;
			}


			//Zooming:
			if (eventKey.ButtonIndex == 4)
			{
				//innover
				if (eventKey.GetControl())
				{
					mousePointingDegree = (mousePointingDegree + 2) % 360;
				}
				else
				{
					zlvl = zlvl / zspd;
					if (zlvl < 0.26f)
					{
						zlvl = 0.26f;
					}
					ZoomHandle();
				}
			}
			else if (eventKey.ButtonIndex == 5)
			{
				if (eventKey.GetControl())
				{
					mousePointingDegree = (mousePointingDegree - 2) % 360;
				}
				else
				{
					zlvl = zlvl * zspd;
					if (zlvl > 7f)
					{
						zlvl = 7f;
					}
					ZoomHandle();
				}
			}
			// GD.Print("ZLVL: " + zlvl);
		}

		if (GetGlobalMousePosition().x > BorderHandler.size || GetGlobalMousePosition().x < -BorderHandler.size || GetGlobalMousePosition().y > BorderHandler.size || GetGlobalMousePosition().y < -BorderHandler.size)
		{
			conquest = false;
		}
		else
		{
			conquest = true;
		}
		if (conquest && mousePointingNation >= 0)
		{
			GD.Print("Checking");
			borderHandler.SuggestConquest(key);
		}
	}

	private void ZoomHandle()
	{
		cam.SetZoom(new Vector2(zlvl, zlvl));
		Rect2 rect = new Rect2(new Vector2(0, 0), new Vector2(cam.GetViewportRect().Size.x * zlvl, cam.GetViewportRect().Size.y * zlvl));
		sky.SetRegionRect(rect);
	}

	public override void _Process(float delta)
	{
		if (clicked)
		{
			Vector2 mp = GetViewport().GetMousePosition();
			//Dragging:
			if (dragging)
			{
				camPos = new Vector2(camPos.x + (clickedPos.x - mp.x) * zlvl, camPos.y + (clickedPos.y - mp.y) * zlvl);
				cam.SetPosition(camPos);
				// GD.Print("New pos: X: " + cam.GetPosition().x + ", Y: " + cam.GetPosition().y);
				clickedPos = mp;
			}
			else if ((clickedPos.x + distance < mp.x || clickedPos.x - distance > mp.x) || (clickedPos.y + distance < mp.y || clickedPos.y - distance > mp.y))
			{
				dragging = true;
				camPos = cam.GetPosition();
			}
		}
	}

	public void _on_next_turn()
	{
		GD.Print("Pressed");
		if (IsMyTurn())
		{
			turnInfo.IncrementWhosTurn();
			player.playernr = (player.playernr + 1) % turnInfo.GetAmountOfPlayers();
		}
	}

	public bool IsMyTurn()
	{
		return turnInfo.GetWhosTurn() == player.playernr;
	}
}
