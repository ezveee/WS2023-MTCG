namespace MTCG.Database.Schemas;

public class TradingDeal
{
	public Guid Id { get; set; }
	public Guid CardToTrade { get; set; }
	public string Type { get; set; }
	public float MinimumDamage { get; set; }

}
