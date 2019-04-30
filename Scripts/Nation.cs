using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
public class Nation : Node2D
{
	private ArrayList borders;
	private ArrayList points;
	private Random r;
	//Perhaps FIXME
	public int playernr;
	bool preferTop;
	public Vector2 genericPointer;
	public Line2D genericPointerLine;
	private Color nationColor;
	private Land land;
	private CollisionPolygon2D collisionArea;
	private Polygon2D nationArea;
	public bool island;
	public int islandWithin;

	public Nation()
	{
		GD.Print("PLEASE DONT USE ME");
		Init(false, -1);
	}
	public Nation(double deg, int playernr, Vector2 genericPointer, Color nationColor, Land land)
	{
		Init(deg >= 180, playernr);
		this.nationColor = nationColor;
		genericPointerLine = new Line2D();
		SetGenericPointer(genericPointer);

		island = false;
		islandWithin = -1;
		this.land = land;
		land.ReferenceToParent(this);
		collisionArea = (CollisionPolygon2D)land.FindNode("Collision");
		nationArea = new Polygon2D();

		nationArea.SetName("NationArea");

		AddChild(land);
		AddChild(nationArea);
		// AddChild(genericPointerLine);
	}

	private void Init(bool preferTop, int playernr)
	{
		this.playernr = playernr;
		this.preferTop = preferTop;
		borders = new ArrayList();
		points = new ArrayList();
		r = new Random();
		//Points in a generic direction from (0,0)
		//Peik mot der flest (tyngst) med punkter står lengst vekke i gjennomsnitt. Alle punkt som er ulik (0,0) drar, men desto lengre vekke den er desto mer drar den mot sitt punkt.
		//Når man skal bruke FollowEdgeNewBorder så skal den "følge" genericPointer.

	}

	public void NewBorder(Vector2 from, Vector2 to)
	{
		Line2D line = new Line2D();
		line.SetWidth(10);
		line.SetName("Border" + borders.Count);
		borders.Add(line);

		// line.AddPoint(from);
		// line.AddPoint(to);

		if (!points.Contains(from))
		{
			points.Add(from);
		}
		if (!points.Contains(to))
		{
			points.Add(to);
		}
	}

	public void FollowEdgeNewBorder(Vector2 from, Vector2 to)
	{
		//Dobbeltsjekk:
		//FIXME

		//Først lag en strek som følger x aksen og så en som følger y aksen.
		Line2D line = new Line2D();
		float thing = r.Next(100) / 100f;
		GD.Print("Color: " + thing);
		// line.SetDefaultColor(new Color(thing, thing, thing, 1));
		line.SetWidth(1);
		line.SetName("Border" + borders.Count);
		borders.Add(line);

		/*
         * Hvis genericPointer er (0,0) så har du alt av banen!!! Du har vunnet!
         */

		// kjør igjennom en kø med punkter. Finn nærmeste hjørnet og så legg til neste nærmeste hjørnet og så 'to'
		Queue<Vector2> dest = new Queue<Vector2>();

		dest.Enqueue(from);

		//Opposite point of map
		if ((from.y == BorderHandler.size && to.y == -BorderHandler.size) || (from.y == -BorderHandler.size && to.y == BorderHandler.size) ||
			(from.x == BorderHandler.size && to.x == -BorderHandler.size) || (from.x == -BorderHandler.size && to.x == BorderHandler.size))
		{
			dest.Enqueue(NearestCorner(from));
			dest.Enqueue(NearestCorner(to));
			dest.Enqueue(to);
		}
		else
		{

			//Hva om du har et 'to' point som ikke er opposite - legg til et tredje punkt i mellom som er den neste etter den landet ditt peker i mot. Altså gjør det som står under etter at du finner ut av hvordan man kan få landet til å peke.
			//hvis genericPointer peker mot from og to så kan du ta nærmeste vei mot de ellers ta lengste vei.


			if (from.y == -BorderHandler.size)
			{
				//X first (Står helt oppe)
				if (to.x > from.x)
				{
					//Til høyre
					if (genericPointer.x >= from.x && genericPointer.y <= to.y)
					{
						ShortRoadFirstX(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstX(from, to, dest);
					}
				}
				else
				{
					//Til venstre
					if (genericPointer.x <= from.x && genericPointer.y <= to.y)
					{
						ShortRoadFirstX(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstX(from, to, dest);
					}
				}

			}
			else if (from.y == BorderHandler.size)
			{
				//X first (Står helt nede)
				if (to.x > from.x)
				{
					//Til høyre
					if (genericPointer.x >= from.x && genericPointer.y >= to.y)
					{
						ShortRoadFirstX(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstX(from, to, dest);
					}
				}
				else
				{
					//Til venstre
					if (genericPointer.x <= from.x && genericPointer.y >= to.y)
					{
						ShortRoadFirstX(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstX(from, to, dest);
					}
				}
			}
			else if (from.x == BorderHandler.size)
			{
				//Y first (Står helt til høyre)

				if (to.y > from.y)
				{
					//Ned
					Vector2 g = genericPointer;
					if (genericPointer.y >= from.y && genericPointer.x >= to.x)
					{
						ShortRoadFirstY(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstY(from, to, dest);
					}
				}
				else
				{
					//Opp
					if (genericPointer.y <= from.y && genericPointer.x >= to.x)
					{
						ShortRoadFirstY(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstY(from, to, dest);
					}
				}
			}
			else
			{
				//Y first (Står helt til venstre)
				if (to.y > from.y)
				{
					//Ned
					if (genericPointer.y >= from.y && genericPointer.x <= to.x)
					{
						ShortRoadFirstY(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstY(from, to, dest);
					}
				}
				else
				{
					//Opp
					if (genericPointer.y <= from.y && genericPointer.x <= to.x)
					{
						ShortRoadFirstY(from, to, dest);
					}
					else
					{
						//Lang vei
						LongRoadFirstY(from, to, dest);
					}
				}
			}
		}

		while (dest.Count != 0)
		{
			Vector2 point = dest.Dequeue();
			line.AddPoint(point);
			if (!points.Contains(point))
			{
				points.Add(point);
			}
		}
	}

	private void LongRoadFirstY(Vector2 from, Vector2 to, Queue<Vector2> dest)
	{
		Vector2 oppositeY1 = new Vector2(from.x, Math.Sign(from.y - to.y) * BorderHandler.size);
		Vector2 oppositeX = new Vector2(-from.x, oppositeY1.y);
		Vector2 oppositeY2 = new Vector2(oppositeX.x, -oppositeY1.y);
		dest.Enqueue(oppositeY1);
		dest.Enqueue(oppositeX);
		dest.Enqueue(oppositeY2);
		ShortRoadFirstX(oppositeY2, to, dest);
		dest.Enqueue(to);
	}

	private void LongRoadFirstX(Vector2 from, Vector2 to, Queue<Vector2> dest)
	{
		Vector2 oppositeX1 = new Vector2(Math.Sign(from.x - to.x) * BorderHandler.size, from.y);
		Vector2 oppositeY = new Vector2(oppositeX1.x, -from.y);
		Vector2 oppositeX2 = new Vector2(-oppositeX1.x, oppositeY.y);
		dest.Enqueue(oppositeX1);
		dest.Enqueue(oppositeY);
		dest.Enqueue(oppositeX2);
		ShortRoadFirstY(oppositeX2, to, dest);
		dest.Enqueue(to);
	}
	private void ShortRoadFirstY(Vector2 from, Vector2 to, Queue<Vector2> dest)
	{
		//Korte vei
		if (to.y != from.y)
			dest.Enqueue(new Vector2(from.x, to.y));
		if (to.x != from.x)
			dest.Enqueue(to);
	}

	private void ShortRoadFirstX(Vector2 from, Vector2 to, Queue<Vector2> dest)
	{
		//Korte vei
		if (to.x != from.x)
			dest.Enqueue(new Vector2(to.x, from.y));
		if (to.y != from.y)
			dest.Enqueue(to);
	}

	public Vector2 NearestCorner(Vector2 point)
	{
		float x;
		float y;

		if (point.x < 0)
			x = -BorderHandler.size;
		else
			x = BorderHandler.size;

		if (point.y < 0)
			y = -BorderHandler.size;
		else if (point.y == 0)
		{
			if (genericPointer.y > 0)
				y = -BorderHandler.size;
			else
				y = BorderHandler.size;
		}
		else
			y = BorderHandler.size;

		return new Vector2(x, y);
	}

	public void CleanNewBorders()
	{
		//Do stuff

		nationArea.SetPolygon(points.ToArray(typeof(Vector2)) as Vector2[]);
		nationArea.SetColor(nationColor);
		collisionArea.SetPolygon(points.ToArray(typeof(Vector2)) as Vector2[]);

		for (int l = 0; l < borders.Count; l++)
		{
			AddChild((Line2D)borders[l]);
		}

	}

	internal void UpdateIsland(int playernr, Vector2[] vector2)
	{
		throw new NotImplementedException();
	}

	internal void ReleaseIsland(int playernr)
	{
		throw new NotImplementedException();
	}
	internal void AddIsland(int playernr)
	{
		throw new NotImplementedException();
	}

	public ArrayList GetBorders()
	{
		return borders;
	}
	public Color GetNationColor()
	{
		return nationColor;
	}

	public Polygon2D GetNationArea()
	{
		return nationArea;
	}

	public ArrayList GetPoints()
	{
		return points;
	}

	public Vector2 CalculateGenericPointer()
	{
		float negMeanX = 0;
		float posMeanX = 0;
		float negMeanY = 0;
		float posMeanY = 0;
		int[] amount = new int[4];
		for (int n = 0; n < amount.Length; n++)
		{
			amount[n] = 1;
		}

		for (int i = 0; i < points.Count; i++)
		{
			Vector2 v = (Vector2)points[i];
			if (v.x < 0)
			{
				negMeanX += v.x;
				amount[0]++;
			}
			else if (v.x != 0)
			{
				posMeanX += v.x;
				amount[1]++;
			}
			if (v.y < 0)
			{
				negMeanY += v.y;
				amount[2]++;
			}
			else if (v.y != 0)
			{
				posMeanY += v.y;
				amount[3]++;
			}
		}

		negMeanX = negMeanX / amount[0];
		posMeanX = posMeanX / amount[1];
		negMeanY = negMeanY / amount[2];
		posMeanY = posMeanY / amount[3];
		Vector2 point = new Vector2((posMeanX + negMeanX) / 2, (posMeanY + negMeanY) / 2);

		return point;
	}

	public void SetGenericPointer(Vector2 point)
	{
		genericPointer = point;
		while (genericPointerLine.GetPointCount() != 0)
		{
			genericPointerLine.RemovePoint(genericPointerLine.GetPointCount() - 1);
		}
		float n = 1.3f;
		genericPointerLine.SetDefaultColor(new Color(nationColor.r / n, nationColor.g / n, nationColor.b / n, 1));
		genericPointerLine.AddPoint(new Vector2(0, 0));
		genericPointerLine.AddPoint(genericPointer);
	}

	public void ClearBorderPoints()
	{
		points.Clear();
		borders.Clear();
	}

}