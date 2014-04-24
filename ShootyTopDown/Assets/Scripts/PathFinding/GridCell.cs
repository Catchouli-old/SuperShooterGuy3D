public class GridCell
{
	public int x, y;

	public bool Accessible { get; set; }

	public GridCell parent;
	public float gScore;
	public float overallScore;

	public GridCell()
	{
		x = y = 0;
	}
	
	public GridCell(int x, int y)
	{
		this.x = x;
		this.y = y;
	}
	
	public GridCell(int x, int y, bool accessible)
	{
		this.x = x;
		this.y = y;
		this.Accessible = accessible;
	}
}