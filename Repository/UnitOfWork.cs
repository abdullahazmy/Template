using Microsoft.EntityFrameworkCore;
using Template.Repository.Interfaces;

namespace Template.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;

        public UnitOfWork(DbContext context) => _context = context;


        #region Repositories
        // Add your repositories here, assign Geters
        #region example
        /*    public GenericRepository<Book> BookRepo
        {
            get
            {
                if (BookRepository == null)
                    BookRepository = new GenericRepository<Book>(_context);
                return BookRepository;
            }
        }
        */
        #endregion
        #endregion

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
