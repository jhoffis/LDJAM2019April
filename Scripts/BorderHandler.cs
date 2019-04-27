using Godot;
using System;
using System.Collections;
public class BorderHandler : Node2D
{
    private int amount;
    private ArrayList nations;
    private Line2D edge;
    public static int size = 200;
    public override void _Ready()
    {
    }
    public void Init(int amount)
    {
        this.amount = amount;
        nations = new ArrayList();
        if (amount <= 0)
        {
            return;
        }

        double deg = 360 / amount;
        double rad = (deg * (Math.PI)) / 180;
        Vector2 center = new Vector2(0, 0);

        for (int i = 0; i < amount; i++)
        {
            GD.Print("Player " + i + " has " + (deg * (i+1) - deg / 2));

            Nation nation = new Nation(deg * i, i, NewVector((deg * (i+1) - deg / 2), size));

            Vector2 firBorder = NewVector(deg * i, size);
            Vector2 secBorder = NewVector(deg * (i + 1), size);
            nation.NewBorder(center, firBorder);
            nation.NewBorder(center, secBorder);
            nation.FollowEdgeNewBorder(firBorder, secBorder);
            nations.Add(nation);

            ArrayList lines = nation.GetBorders();
            for (int l = 0; l < lines.Count; l++)
            {
                AddChild((Line2D)lines[l]);
            }

            AddChild(nation.genericPointerLine);
            UpdateGenericPointer(nation);
            
        }

        // edge = new Line2D();
        // edge.SetWidth(1);
        // edge.SetName("Edge");
        // AddChild(edge);

        // for (int i = 0; i < 5; i++)
        // {
        //     edge.AddPoint(NewVector((90 * i) % 360 - 45, size));
        // }
    }

    private void UpdateGenericPointer(int i){
            UpdateGenericPointer((Nation) nations[i]);
    }

    private void UpdateGenericPointer(Nation nation){
            Vector2 pointer = nation.CalculateGenericPointer();
            double degree = Math.Atan(pointer.y / pointer.x) * (180 / Math.PI);
            if(degree == 0 && pointer.x < 0)
                degree = 180;
            pointer = NewVector(degree, size);
            nation.SetGenericPointer(pointer);
    }

    public Vector2 NewVector(double deg, double katet)
    {
        Vector2 res = new Vector2(0, 0);
        deg = deg % 360;

        if (deg == 180)
        {
            res = new Vector2((float)(-katet), 0);
        }
        else if (deg == 0)
        {
            res = new Vector2((float)(katet), 0);
        }
        else
        {
            double radian = (deg * (Math.PI)) / 180;
            double hypotenus1 = Math.Abs(katet / Math.Sin(radian));
            double hypotenus2 = Math.Abs(katet / Math.Cos(radian));

            float x1 = (float)(Math.Round(hypotenus1 * Math.Cos(radian)));
            float y1 = (float)(Math.Round(hypotenus1 * Math.Sin(radian)));

            float x2 = (float)(Math.Round(hypotenus2 * Math.Cos(radian)));
            float y2 = (float)(Math.Round(hypotenus2 * Math.Sin(radian)));

            float x2abs = Math.Abs(x2);
            float y2abs = Math.Abs(y2);
            float x1abs = Math.Abs(x1);
            float y1abs = Math.Abs(y1);

            //Find the closest to (100, 100), but not bigger
            if (x2abs <= katet && y2abs <= katet)
            {
                if ((x1abs <= katet && y1abs <= katet))
                {
                    if (x2abs > x1abs && y2abs > y1abs)
                    {
                        res = new Vector2(x2, y2);
                    }
                    else
                    {
                        res = new Vector2(x1, y1);
                    }
                }
                else
                {
                    res = new Vector2(x2, y2);
                }
            }
            else
            {
                res = new Vector2(x1, y1);
            }


        }
        return res;
    }


}
