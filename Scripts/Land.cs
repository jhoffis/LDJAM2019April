using Godot;
using System;

public class Land : Node2D
{
	private Nation nation;
    public override void _Ready()
    {
        
    }

	public void ReferenceToParent(Nation nation){
		this.nation = nation;
	}

	public void _on_Area_input_event(Node viewport, InputEvent @event, int shape_idx)
	{
		// GD.Print("Nation nr " + nation.playernr);
		Map.mousePointingNation = nation.playernr;
		if (@event is InputEventMouseButton eventKey)
		{
			// GD.Print(eventKey.ButtonIndex);
		}
	}
}
