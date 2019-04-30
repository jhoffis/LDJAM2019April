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


	//Det er en bug som gjelder øyer.
	//Hvis target har ingen punkt som er på kanten og ikke flere naboer enn 1, init (om ikke finnes) en ny mengde med punkt som er designert til targets playernr. 
	/* 
		Basically ha en referanse til rød sine punkter som blir en nogozone og så fjern her alle punkt som er lik noen av disse.
	 */
	//Sjekk dette hver gang og om dette er usant så sjekk om man har en øy for denne nasjonen. Slett så om sant. 

	//Også: Sett nasjonen som øy eller ei lokalt i nasjonen; om nasjonen er en øy så øk z verdien - ellers mink z verdien.

	/*
		Grunnen til at noen ganger så kommer verdensrommet inn i landet med vanlig kutt (ingen øy) er fordi jeg ikke sjekker Contains() i innlegging av originale punkter i InsertPointsIntoFriendlyBorder. 
		Da kommer det vektorer som dette etterhverandre: (x1, y1) -> (x2, y2) -> (x2, y2) -> (x1, y1)
		Dette er relativt OK OM du ikke kutter noe mer... Men man må jo kutte >:)
	 */


	private void TryConquest(int prev, int next, Nation targetNation, bool away)
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

		Queue<Vector2> newTargetBorder = new Queue<Vector2>();
		Stack<Vector2> newFriendlyBorder = new Stack<Vector2>();
		Vector2[] newBorderPoints;



		//Basically sjekk om LShift er nede
		if (away)
		{
			//Først kutt target
			DrawNewTargetBorderAwayFromCenter(prev, targetPoints, newTargetBorder, next);
			newBorderPoints = GatherTargetBorderTowardsCenter(prev, targetPoints, next);
		}
		else
		{
			//Først kutt target
			DrawNewTargetBorderTowardsCenter(prev, targetPoints, newTargetBorder, next);
			newBorderPoints = GatherTargetBorderAwayFromCenter(prev, targetPoints, next);
		}



		Vector2 found = IsBorderTouching(myPoints, newBorderPoints);
		if (found.x == -1)
			return;

		Vector2 targetIsIsland = IsIsland(newTargetBorder.ToArray() as Vector2[], targetNation.playernr);
		if (targetIsIsland.x == -1)
		{
			DrawNewFriendlyBorder(myPoints, newFriendlyBorder, newBorderPoints, (int)found.x, (int)found.y, false, null);
			RemoveOwnerOfIsland(targetNation);
		}
		else
		{
			//FIXME
			GD.Print("ISLAND!!!");
			DrawNewFriendlyBorder(myPoints, newFriendlyBorder, newBorderPoints, (int)found.x, (int)found.y, true, newTargetBorder.ToArray() as Vector2[]);
			SetOwnerOfIsland(targetNation, (int)targetIsIsland.y);
		}

		//Sjekk også om at NOEN har blitt island... under myNation faktisk.

		// Sjekk om myNation også er island. Hvis så: Sjekk om den vil være island etter kutt. Hvis ikke flytt z verdi til vanlig posisjon, sett island (elns) til false og fjern alle island referanser til myNation.
		DecipherWhichNationsAreIslands(targetNation);


		//Clean up before adding new points
		targetNation.ClearBorderPoints();
		myNation.ClearBorderPoints();

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
		myNation.NewBorder((Vector2)nfb[0], (Vector2)nfb[nfb.Length - 1]);
		myNation.CleanNewBorders();
		UpdateGenericPointer(myNation);
	}

	private void DecipherWhichNationsAreIslands(Nation targetNation)
	{
		foreach (Nation nation in nations)
		{
			if (nation.playernr != targetNation.playernr)
			{
				Vector2 isAnIsland = IsIsland(nation.GetPoints().ToArray(typeof(Vector2)) as Vector2[], nation.playernr);
				if (isAnIsland.x == -1)
				{
					RemoveOwnerOfIsland(nation);
				}
				else
				{
					SetOwnerOfIsland(nation, (int)isAnIsland.y);
				}
			}
		}
	}

	private void RemoveOwnerOfIsland(Nation nation)
	{
		if (nation.islandWithin != -1)
		{
			Nation owner = (Nation)nations[nation.islandWithin];
			owner.ReleaseIsland(nation.playernr);
		}

		nation.island = false;
		nation.islandWithin = -1;
	}

	private void SetOwnerOfIsland(Nation nation, int newOwnerIndex)
	{
		Nation newOwner = (Nation)nations[newOwnerIndex];
		if (nation.islandWithin != -1 && nation.islandWithin != newOwnerIndex)
		{
			Nation owner = (Nation)nations[nation.islandWithin];
			owner.ReleaseIsland(nation.playernr);
			newOwner.AddIsland(nation.playernr);
		}

		newOwner.UpdateIsland(nation.playernr, nation.GetPoints().ToArray(typeof(Vector2)) as Vector2[]);

		nation.island = true;
		nation.islandWithin = newOwnerIndex;
	}

	private Vector2 IsIsland(Vector2[] targetPoints, int target)
	{
		int res = 1;
		int owner = -1;
		int ownerCount = 0;
		//Hvis playercount er lavere enn 2 så ser man bare på kantenoder
		if (amount > 2)
		{
			//se på kantenoder OG om man er borti en annen nasjon.
			foreach (Vector2 v in targetPoints)
			{
				if (Math.Abs(v.x) == size || Math.Abs(v.y) == size)
				{
					res = -1;
					break;
				}
				for (int i = 0; i < nations.Count; i++)
				{
					if (i == target || i == owner)
						break;
					Nation otherNation = (Nation)nations[i];
					ArrayList otherPoints = otherNation.GetPoints();

					for (int n = 0; n < otherPoints.Count; n++)
					{
						if (IsSamePoint(v, (Vector2)otherPoints[n]))
						{
							owner = i;
							ownerCount++;
							break;
						}
					}
				}

				if (ownerCount > 1)
				{
					res = -1;
					break;
				}
			}
		}
		else
		{
			foreach (Vector2 v in targetPoints)
			{
				if (Math.Abs(v.x) == size || Math.Abs(v.y) == size)
				{
					res = -1;
					break;
				}
			}
		}
		return new Vector2(res, owner);
	}

	private void DrawNewFriendlyBorder(Vector2[] myPoints, Stack<Vector2> newFriendlyBorder, Vector2[] newBorderPoints, int myPointsTill, int targetPointsFrom, bool islandmode, Vector2[] newTargetBorder)
	{
		int lastIndex = myPoints.Length - 2;
		int betweenIndex = myPoints.Length - 1;
		int offIndex = myPointsTill + 1;
		if (myPointsTill == 0)
			offIndex = 0;

		Vector2 lastPoint = myPoints[lastIndex];
		Vector2 betweenPoint = myPoints[betweenIndex];
		Vector2 thisPoint = myPoints[0];

		for (int i = 0; i < myPointsTill; i++)
		{
			lastPoint = myPoints[lastIndex];
			betweenPoint = myPoints[betweenIndex];
			thisPoint = myPoints[i];

			IsSameDirectionCorrection(lastPoint, betweenPoint, thisPoint, newFriendlyBorder);

			lastIndex = (lastIndex + 1) % myPoints.Length;
			betweenIndex = (betweenIndex + 1) % myPoints.Length;
		}

		InsertPointsIntoFriendlyBorder(newFriendlyBorder, newBorderPoints, targetPointsFrom, betweenPoint, thisPoint, myPoints, offIndex, islandmode, newTargetBorder);
	}

	private Vector2 IsBorderTouching(Vector2[] myPoints, Vector2[] newBorderPoints)
	{
		for (int i = 0; i < myPoints.Length; i++)
		{
			for (int n = 0; n < newBorderPoints.Length; n++)
			{
				if (IsSamePoint(myPoints[i], newBorderPoints[n]))
				{
					return new Vector2(i, n);
				}
			}
		}
		return new Vector2(-1, -1);
	}

	private bool IsSamePoint(Vector2 v1, Vector2 v2)
	{
		return v1.x == v2.x && v1.y == v2.y;

	}

	private bool IsSameDirection(Vector2 lastPoint, Vector2 betweenPoint, Vector2 thisPoint)
	{
		double d1 = (betweenPoint.y - lastPoint.y) / (betweenPoint.x - lastPoint.x);
		double d2 = (thisPoint.y - betweenPoint.y) / (thisPoint.x - betweenPoint.x);

		//Samme retning som forrige
		return (d1 >= d2 - differ && d1 <= d2 + differ);
	}

	private void IsSameDirectionCorrection(Vector2 lastPoint, Vector2 betweenPoint, Vector2 thisPoint, Stack<Vector2> newFriendlyBorder)
	{
		if (IsSameDirection(lastPoint, betweenPoint, thisPoint))
		{
			newFriendlyBorder.Pop();
			newFriendlyBorder.Push(thisPoint);
		}
		else
		{
			newFriendlyBorder.Push(thisPoint);
		}
	}

	private void InsertPointsIntoFriendlyBorder(Stack<Vector2> newFriendlyBorder, Vector2[] newBorderPoints, int n, Vector2 lastPoint, Vector2 betweenPoint, Vector2[] myPoints, int leftOffIndex, bool islandmode, Vector2[] newTargetBorder)
	{
		int nbp;

		newFriendlyBorder.Push(newBorderPoints[(n + 1) % newBorderPoints.Length]);
		betweenPoint = newBorderPoints[(n + 1) % newBorderPoints.Length];

		for (int a = 0; a < newBorderPoints.Length - 2; a++)
		{
			nbp = (a + n + 2) % newBorderPoints.Length;
			if (!ContainsInStack(newFriendlyBorder, newBorderPoints[nbp]))
			{
				if (!(islandmode && ContainsInArray(newTargetBorder, newBorderPoints[nbp])))
				{
					IsSameDirectionCorrection(lastPoint, betweenPoint, newBorderPoints[nbp], newFriendlyBorder);
					lastPoint = betweenPoint;
					betweenPoint = newBorderPoints[nbp];
				}
			}
		}
		for (int r = leftOffIndex; r < myPoints.Length; r++)
		{
			if (!(islandmode && ContainsInArray(newTargetBorder, myPoints[r])))
			{
				IsSameDirectionCorrection(lastPoint, betweenPoint, myPoints[r], newFriendlyBorder);
				lastPoint = betweenPoint;
				betweenPoint = myPoints[r];
			}
			//FIXME : sjekk om myPoints[r] == lastPoint.

			//Sjekk om l
			//Legg til søk av seg selv nedover i listen. Om den finner seg selv fjern alle i mellom og sin kopi.

			// newFriendlyBorder = PopTillCopyFound(newFriendlyBorder, myPoints[r]);

		}

		//Control last point but don't actually add it!
		IsSameDirectionCorrection(lastPoint, betweenPoint, myPoints[0], newFriendlyBorder);
		newFriendlyBorder.Pop();


	}

	private Stack<Vector2> PopTillCopyFound(Stack<Vector2> path, Vector2 point)
	{
		Stack<Vector2> cleansedPath = new Stack<Vector2>(new Stack<Vector2>(path));
		bool found = false;

		cleansedPath.Pop();
		int i = 0;
		while (cleansedPath.Count != 0 && i < 5)
		{
			Vector2 popped = cleansedPath.Pop();
			if (popped.x == point.x && popped.y == point.y)
			{
				cleansedPath.Push(point);
				found = true;
				break;
			}
			i++;
		}

		if (found)
		{
			return cleansedPath;
		}
		else
		{
			return path;
		}
	}

	private bool ContainsInArray(Vector2[] arr, Vector2 point)
	{
		foreach (Vector2 v in arr)
		{
			if (v.x == point.x && v.y == point.y)
				return true;
		}
		return false;
	}

	private bool ContainsInStack(Stack<Vector2> border, Vector2 point)
	{
		return ContainsInArray(border.ToArray() as Vector2[], point);
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

	private Vector2[] GatherTargetBorderTowardsCenter(int previous, Vector2[] targetPoints, int nextAct)
	{
		ArrayList list = new ArrayList();
		bool suggested = false;
		for (int i = 0; i < targetPoints.Length; i++)
		{
			if (i <= previous || i >= nextAct)
			{
				list.Add(targetPoints[i]);
			}
			else if (!suggested)
			{
				list.Add((Vector2)suggestedPoints[0]);
				list.Add((Vector2)suggestedPoints[1]);
				suggested = true;
			}
		}
		return list.ToArray(typeof(Vector2)) as Vector2[];
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

	// Bug: Den tegner strek UANSETT om den treffer en av sine egne linjer. Tenker ikke noe på om det finnes en strek som er nærmere eller om den går igjennom en annen sitt land.
	internal void SuggestConquest(InputEventMouseButton key)
	{
		double d1;
		differ = 0.025;

		d1 = Math.Tan(Map.mousePointingDegree * Math.PI / 180);

		Nation targetNation = (Nation)(nations[Map.mousePointingNation]);
		ArrayList points = targetNation.GetPoints();
		Vector2 mp = GetGlobalMousePosition();

		suggestedPoints = new ArrayList();
		ArrayList tempLineSuggestions = new ArrayList();
		int previous = -1;
		int next = -1;



		for (int i = 0; i < points.Count; i++)
		{
			// GD.Print("------------" + points.Count);
			Vector2 p1 = (Vector2)points[i];
			Vector2 p2 = (Vector2)points[(i + 1) % points.Count];
			double a = (p2.y + size) - (p1.y + size);
			double b = (p2.x + size) - (p1.x + size);
			double d2 = a / b;

			double x;
			double y;

			if (!(d1 > d2 - differ && d1 < d2 + differ))
			{
				//TODO flytt dette til en generell sjekk hvor kuttes metode - Da kan jeg lett(ere) få til NATIONTEXT yalll!

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
						tempLineSuggestions.Add(new LineSuggestion(new Vector2((float)(x), (float)(y)), i, tempLineSuggestions.Count));
					}
				}
			}
		}

		if (tempLineSuggestions.Count < 2)
			return;

		LineSuggestion suggestionPoint1 = FindSmallestDistance(tempLineSuggestions.ToArray(typeof(LineSuggestion)) as LineSuggestion[], mp, null);
		tempLineSuggestions.RemoveAt(suggestionPoint1.tempindex);
		LineSuggestion suggestionPoint2 = FindSmallestDistance(tempLineSuggestions.ToArray(typeof(LineSuggestion)) as LineSuggestion[], mp, suggestionPoint1);

		if (suggestionPoint1.index < suggestionPoint2.index)
		{
			previous = suggestionPoint1.index;
			suggestedPoints.Add(suggestionPoint1.line);

			next = suggestionPoint2.index + 1;
			suggestedPoints.Add(suggestionPoint2.line);
		}
		else
		{
			previous = suggestionPoint2.index;
			suggestedPoints.Add(suggestionPoint2.line);

			next = suggestionPoint1.index + 1;
			suggestedPoints.Add(suggestionPoint1.line);
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
			TryConquest(previous, next, targetNation, key.Shift);
		}

	}

	private LineSuggestion FindSmallestDistance(LineSuggestion[] suggestions, Vector2 origin, LineSuggestion other)
	{
		LineSuggestion smallest = suggestions[0];

		for (int i = 1; i < suggestions.Length; i++)
		{
			if (suggestions[i].CompareLine(smallest.line, origin) > 0)
			{
				if (other == null)
					smallest = suggestions[i];
				else
				{
					if (suggestions[i].line.x == origin.x)
					{
						int sug1 = Math.Sign(suggestions[i].line.y - origin.y);
						int sug2 = Math.Sign(other.line.y - origin.y);
						// GD.Print("i: " + sug1 + ", other: " + sug2);
						//sjekk y
						if (sug1 != sug2)
						{
							smallest = suggestions[i];
						}
					}
					else
					{
						int sug1 = Math.Sign(suggestions[i].line.x - origin.x);
						int sug2 = Math.Sign(other.line.x - origin.x);
						// GD.Print("i(" + suggestions[i].line.x + ", " + other.line.x + "): " + sug1 + ", other: " + sug2);
						//sjekk x
						if (sug1 != sug2)
						{
							smallest = suggestions[i];
						}
					}
				}
			}
		}

		return smallest;
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
