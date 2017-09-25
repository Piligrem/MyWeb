namespace InSearch.Core.Infrastructure
{
    public interface IStartupTask
    {
        void Execute();
        int Order { get; }
    }
}
