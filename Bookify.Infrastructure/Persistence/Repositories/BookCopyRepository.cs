namespace Bookify.Infrastructure.Persistence.Repositories
{
    public class BookCopyRepository : BaseRepository<BookCopy>, IBookCopyRepository
    {
        public BookCopyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public void SetAllAsNotAvailable(int bookId)
        {
             _context.BookCopies
                .Where(bc => bc.BookId == bookId)
                .ExecuteUpdate(bc => bc.SetProperty(b => b.IsAvailableForRental, false));
        }
    }
}
