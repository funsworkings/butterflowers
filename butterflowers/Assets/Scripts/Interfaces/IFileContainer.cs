namespace Interfaces
{
	public interface IFileContainer
	{
		float Capacity { get; set; }
		float FillAmount { get; }
		
		string[] GetFiles();

		void AddFile(string file);
		void RemoveFile(string file);
	}
}