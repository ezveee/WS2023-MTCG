namespace MTCG.Database.Schemas;

public class UserStats
{
	public string Name { get; set; }
	public int Elo { get; set; }
	public int Wins { get; set; }
	public int Losses { get; set; }
}
