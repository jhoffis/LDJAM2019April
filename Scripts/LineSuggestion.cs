using Godot;
using System;

public class LineSuggestion
{
	public Vector2 line { get; private set; }
	public int index { get; private set; }
    public int tempindex { get; private set; }
	public LineSuggestion()
	{
	}

	public LineSuggestion(Vector2 line, int i, int tempindex)
	{
		this.line = line;
		this.index = i;
        this.tempindex = tempindex;
	}
	/**First value represents its Comparable value, second value represents this lines 'i'
	 */
	public int CompareLine(Vector2 otherLine, Vector2 origin)
	{
		int comparable = 0;
		//Is it not the same?
		if (!(line.x == otherLine.x && line.y == otherLine.y))
		{
			double d1 = (otherLine.y - origin.y) / (otherLine.x - origin.x);
			double d2 = (line.y - origin.y) / (line.x - origin.x);

			double distance1 = Math.Abs(d1 * (line.x - origin.x));
			double distance2 = Math.Abs(d2 * (otherLine.x - origin.x));

			if (distance1 <= distance2)
			{
				comparable = 1;
			}
			else
			{
				comparable = -1;
			}
		}

		return comparable;
	}
}
