namespace uwu.Snippets.Load
{
	public interface ILoadDependent
	{
		float Progress { get; }
		bool Completed { get; }
	}
}