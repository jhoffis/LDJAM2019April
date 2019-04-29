using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
public class BorderHandler : Node2D
{
	private int amount;
	private ArrayList nations;
	private Line2D edge;
	public static int size = 200;
	private Color[] teamColors;
	private Line2D line;
	private ArrayList suggestedPoints;

	public override void _Ready()
	{
	}
	public void Init(int amount, Color[] teamColors)
	{
		this.teamColors = teamColors;
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
			GD.Print("Player " + i + " has " + (deg * (i + 1) - deg / 2));
			PackedScene landResource = (PackedScene)ResourceLoader.Load("res://Scenes/Land.tscn");
			Land land = (Land)landResource.Instance();
			Nation nation = new Nation(deg * i, i, NewVector((deg * (i + 1) - deg / 2), size), teamColors[i], land);

			Vector2 firBorder = NewVector(deg * i, size);
			Vector2 secBorder = NewVector(deg * (i + 1), size);
			nation.NewBorder(center, firBorder);
			nation.FollowEdgeNewBorder(firBorder, secBorder);
			nation.NewBorder(secBorder, center);
			nations.Add(nation);

			ArrayList borders = nation.GetBorders();

			nation.CleanNewBorders();
			UpdateGenericPointer(nation);
			AddChild(nation);
		}
		line = new Line2D();
		AddChild(line);

	}

	private void UpdateGenericPointer(int i)
	{
		UpdateGenericPointer((Nation)nations[i]);
	}

	private void UpdateGenericPointer(Nation nation)
	{
		Vector2 pointer = nation.CalculateGenericPointer();
		double degree = Math.Atan(pointer.y / pointer.x) * (180 / Math.PI);
		if (pointer.x < 0)
			degree += 180;
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

	private void TryCut(int prev1, int next1, Nation targetNation, bool away)
	{
		Nation myNation = (Nation)nations[Map.player.playernr];
		//Sjekk om det er samme nasjon
		if (targetNation.playernr == myNation.playernr)
		{
			return;
		}

		Vector2[] targetPoints = targetNation.GetPoints().ToArray(typeof(Vector2)) as Vector2[];
		Vector2[] myPoints = myNation.GetPoints().ToArray(typeof(Vector2)) as Vector2[];
		//Sjekk om minst to punkter er like


		//First change enemyborder
		targetNation.ClearBorderPoints();
		myNation.ClearBorderPoints();
		Queue<Vector2> newTargetBorder = new Queue<Vector2>();
		Stack<Vector2> newFriendlyBorder = new Stack<Vector2>();


		int nextAct;
		if (next1 <= prev1)
		{
			nextAct = targetPoints.Length - 1;
		}
		else
		{
			nextAct = next1;
		}


		if (away)
		{
			DrawNewTargetBorderAwayFromCenter(prev1, targetPoints, newTargetBorder, nextAct);

		}
		else
		{
			DrawNewTargetBorderTowardsCenter(prev1, targetPoints, newTargetBorder, nextAct);
			Vector2[] newBorderPoints = GatherTargetBorderAwayFromCenter(prev1, targetPoints, nextAct);
			bool found = false;
			int lastIndex = myPoints.Length - 2;
			int betweenIndex = myPoints.Length - 1;

			int i = 0;
			while (!found)
			{
				Vector2 lastPoint = myPoints[lastIndex];
				Vector2 betweenPoint = myPoints[betweenIndex];
				Vector2 thisPoint = myPoints[i];

				//Sjekk om forrige point peker samme vei som i point. Hvis sant ikke enqueue.
				// Se etter om punktet er rett.
				for (int n = 0; n < newBorderPoints.Length; n++)
				{
					if (myPoints[i].x == newBorderPoints[n].x && myPoints[i].y == newBorderPoints[n].y)
					{
						found = InsertPointsIntoFriendlyBorder(newFriendlyBorder, newBorderPoints, n, betweenPoint, thisPoint, myPoints, i + 1);
						break;
					}
				}
				if (found)
					break;

				SameDirectionCorrection(lastPoint, betweenPoint, thisPoint, newFriendlyBorder);

				lastIndex = (lastIndex + 1) % myPoints.Length;
				betweenIndex = (betweenIndex + 1) % myPoints.Length;


				i++;
			}

		}


		//Change target borders
		Vector2 firstPoint = newTargetBorder.Peek();
		while (newTargetBorder.Count != 0)
		{
			if (newTargetBorder.Count > 1)
				targetNation.NewBorder(newTargetBorder.Dequeue(), newTargetBorder.Peek());
			else
				targetNation.NewBorder(newTargetBorder.Dequeue(), firstPoint);
		}
		targetNation.CleanNewBorders();
		UpdateGenericPointer(targetNation);

		//Change my borders
		Vector2[] nfb = newFriendlyBorder.ToArray() as Vector2[];
		for (int b = nfb.Length - 1; b >= 1; b--)
		{
			myNation.NewBorder(nfb[b], nfb[b - 1]);
		}
		myNation.NewBorder(nfb[0], nfb[nfb.Length - 1]);
		myNation.CleanNewBorders();
		UpdateGenericPointer(myNation);
	}

	private void SameDirectionCorrection(Vector2 lastPoint, Vector2 betweenPoint, Vector2 thisPoint, Stack<Vector2> newFriendlyBorder)
	{
		double d1 = (betweenPoint.y - lastPoint.y) / (betweenPoint.x - lastPoint.x);
		double d2 = (thisPoint.y - betweenPoint.y) / (thisPoint.x - betweenPoint.x);
		if (d1 >= d2 - differ
						&&
						d1 <= d2 + differ)
		{
			//Samme retning som forrige
			newFriendlyBorder.Pop();
			newFriendlyBorder.Push(thisPoint);
		}
		else
		{
			newFriendlyBorder.Push(thisPoint);
		}
	}

	private bool InsertPointsIntoFriendlyBorder(Stack<Vector2> newFriendlyBorder, Vector2[] newBorderPoints, int n, Vector2 lastPoint, Vector2 betweenPoint, Vector2[] myPoints, int leftOffIndex)
	{
		bool found;
		int nbp;
		found = true;

		newFriendlyBorder.Push(newBorderPoints[(n + 1) % newBorderPoints.Length]);
		betweenPoint = newBorderPoints[(n + 1) % newBorderPoints.Length];

		for (int a = 0; a < newBorderPoints.Length - 2; a++)
		{
			nbp = (a + n + 2) % newBorderPoints.Length;
			if (!Contains(newFriendlyBorder, newBorderPoints[nbp]))
			{
				SameDirectionCorrection(lastPoint, betweenPoint, newBorderPoints[nbp], newFriendlyBorder);
				lastPoint = betweenPoint;
				betweenPoint = newBorderPoints[nbp];
			}
		}

		for (int r = leftOffIndex; r < myPoints.Length; r++)
		{
			if (!Contains(newFriendlyBorder, myPoints[r]))
			{
				SameDirectionCorrection(lastPoint, betweenPoint, myPoints[r], newFriendlyBorder);
				lastPoint = betweenPoint;
				betweenPoint = myPoints[r];
			}
		}

		//Control last point but don't actually add it!
		SameDirectionCorrection(lastPoint, betweenPoint, myPoints[0], newFriendlyBorder);
		newFriendlyBorder.Pop();

		return found;
	}

	private bool Contains(Stack<Vector2> border, Vector2 point)
	{
		Vector2[] arr = border.ToArray() as Vector2[];
		foreach (Vector2 v in arr)
		{
			if (v.x == point.x && v.y == point.y)
				return true;
		}
		return false;
	}

	private void DrawNewTargetBorderTowardsCenter(int previous, Vector2[] targetPoints, Queue<Vector2> newTargetBorder, int nextAct)
	{
		bool suggested = false;

		for (int i = 0; i < targetPoints.Length; i++)
		{
			if (i <= previous || i >= nextAct)
			{
				newTargetBorder.Enqueue(targetPoints[i]);
			}
			else if (!suggested)
			{
				newTargetBorder.Enqueue((Vector2)suggestedPoints[0]);
				newTargetBorder.Enqueue((Vector2)suggestedPoints[1]);
				suggested = true;
			}
		}
	}
	private Vector2[] GatherTargetBorderAwayFromCenter(int previous, Vector2[] targetPoints, int nextAct)
	{
		ArrayList list = new ArrayList();
		bool suggested = false;
		for (int i = 0; i < targetPoints.Length + 1; i++)
		{
			if (i <= previous && !suggested)
			{
				list.Add((Vector2)suggestedPoints[0]);
				suggested = true;
			}
			else if (i > previous && i < nextAct)
			{
				list.Add(targetPoints[i]);
				suggested = false;
			}
			else if (!suggested)
			{
				list.Add((Vector2)suggestedPoints[1]);
				suggested = true;
			}
		}
		return list.ToArray(typeof(Vector2)) as Vector2[];
	}

	private void DrawNewTargetBorderAwayFromCenter(int previous, Vector2[] targetPoints, Queue<Vector2> newTargetBorder, int nextAct)
	{
		bool suggested = false;
		for (int i = 0; i < targetPoints.Length + 1; i++)
		{
			if (i <= previous && !suggested)
			{
				newTargetBorder.Enqueue((Vector2)suggestedPoints[0]);
				suggested = true;
			}
			else if (i > previous && i < nextAct)
			{
				newTargetBorder.Enqueue(targetPoints[i]);
				suggested = false;
			}
			else if (!suggested)
			{
				newTargetBorder.Enqueue((Vector2)suggestedPoints[1]);
				suggested = true;
			}
		}
	}

	internal void SuggestConquest(InputEventMouseButton key)
	{
		double d1;
		differ = 0.025;

		d1 = Math.Tan(Map.mousePointingDegree * Math.PI / 180);

		Nation targetNation = (Nation)(nations[Map.mousePointingNation]);
		ArrayList points = targetNation.GetPoints();
		Vector2 mp = GetGlobalMousePosition();

		suggestedPoints = new ArrayList();
		int previous = -1;
		int next = -1;

		for (int i = 0; i < points.Count; i++)
		{
			GD.Print("------------" + points.Count);
			Vector2 p1 = (Vector2)points[i];
			Vector2 p2 = (Vector2)points[(i + 1) % points.Count];
			double a = (p2.y + size) - (p1.y + size);
			double b = (p2.x + size) - (p1.x + size);
			double d2 = a / b;

			double x;
			double y;

			if (!(d1 > d2 - differ && d1 < d2 + differ))
			{
				if (p1.x == p2.x)
				{
					// Få til spesial tilfelle der x = 0 eller y = inf
					x = p1.x;
					y = (mp.y + (d1 * (x - mp.x)));
				}
				else
				{
					double y1 = (mp.y + (d1 * -mp.x));
					double y2 = (p1.y + (d2 * -p1.x));
					x = ((y2 - y1) / (d1 - d2));
					y = (d1 * x + y1);
				}

				// GD.Print("X: " + x + ", Y: " + y + " I mellom punkt: (" + p1.x + ", " + p1.y + ") og (" + p2.x + ", " + p2.y + ")");
				if (CheckX(p1, p2, x))
				{
					if (CheckY(p1, p2, y))
					{
						if (suggestedPoints.Count == 0)
						{
							suggestedPoints.Add(new Vector2((float)(x), (float)(y)));
							previous = i;
						}
						else if (suggestedPoints.Count == 1)
						{
							suggestedPoints.Add(new Vector2((float)(x), (float)(y)));
							next = i + 1;
						}
					}
				}
			}
		}

		while (line.GetPointCount() != 0)
		{
			line.RemovePoint(line.GetPointCount() - 1);
		}
		for (int i = 0; i < suggestedPoints.Count; i++)
		{
			line.AddPoint(mp);
			line.AddPoint((Vector2)suggestedPoints[i]);
		}

		if (key != null && key.IsPressed() && key.ButtonIndex == 2)
		{
			TryCut(previous, next, targetNation, key.Shift);
		}

	}
	int manMaoHaLittAAGaoPao = 2;
	private double differ;

	private bool CheckX(Vector2 p1, Vector2 p2, double x)
	{
		if (p1.x <= p2.x)
		{
			return (x >= p1.x - manMaoHaLittAAGaoPao && x <= p2.x + manMaoHaLittAAGaoPao);
		}
		else
		{
			return (x <= p1.x + manMaoHaLittAAGaoPao && x >= p2.x - manMaoHaLittAAGaoPao);
		}
	}

	private bool CheckY(Vector2 p1, Vector2 p2, double y)
	{
		if (p1.y <= p2.y)
		{
			return (y >= p1.y - manMaoHaLittAAGaoPao && y <= p2.y + manMaoHaLittAAGaoPao);
		}
		else
		{
			return (y <= p1.y + manMaoHaLittAAGaoPao && y >= p2.y - manMaoHaLittAAGaoPao);
		}
	}
}
