namespace Interfaces
{
	public interface IFlammable
	{
		bool IsOnFire { get; }
		
		void Fire();
		void Extinguish();
	}
}