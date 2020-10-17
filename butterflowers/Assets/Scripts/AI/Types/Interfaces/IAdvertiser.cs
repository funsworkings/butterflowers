namespace AI.Types.Interfaces
{
	public interface IAdvertiser
	{
		Advertiser Advertiser { get; set; }
		void UpdateAllAdvertisements();
	}
}