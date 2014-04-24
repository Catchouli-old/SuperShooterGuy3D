using System;

[Serializable]
public class Point
{
	public int x, y;
	
	public Point()
	{
		x = 0;
		y = 0;
	}
	
	public Point(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public override bool Equals(object other)
	{
		if (other is Point)
		{
			Point otherPoint = (Point)other;

			return x == otherPoint.x && y == otherPoint.y;
		}

		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode ();
	}
}
