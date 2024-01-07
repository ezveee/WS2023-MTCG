namespace MTCG.Database.Schemas;

public class Card
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public float Damage { get; set; }
}
