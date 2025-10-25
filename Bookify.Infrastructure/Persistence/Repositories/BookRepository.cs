namespace Bookify.Infrastructure.Persistence.Repositories
{
    public class BookRepository : BaseRepository<Book>, IBookRepository
    {
        public BookRepository(ApplicationDbContext context) : base(context)
        {
        }

        public IQueryable<Book> GetDetails()
        {
            return _context.Books
                .Include(b => b.Author)
                .Include(b => b.Copies)
                .Include(Book => Book.Categories)
                    .ThenInclude(bc => bc.Category);
        }
    }
}
