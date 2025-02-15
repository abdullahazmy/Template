namespace Template.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync();
    }
}
