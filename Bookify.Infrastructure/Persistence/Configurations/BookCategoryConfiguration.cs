namespace Bookify.Infrastructure.Persistence.Configurations
{
    internal class BookCategoryConfiguration : IEntityTypeConfiguration<BookCategory>
    {
        public void Configure(EntityTypeBuilder<BookCategory> builder)
        {
            // generate composite key in many to many relationship table
            builder.HasKey(e => new { e.BookId, e.CategoryId });
        }
    }
}