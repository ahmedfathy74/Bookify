
using Bookify.Domain.Entities;

namespace Bookify.Web.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public BookCopiesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        [AjaxOnly]
        public IActionResult Create(int bookId)
        {
            var book = _unitOfWork.Books.GetById(bookId);

            if (book is null)
                return NotFound();

            var viewModel = new BookCopyFormViewModel
            {
                BookId = bookId,
                ShowRentalInput = book.IsAvailableForRental
            };

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _unitOfWork.Books.GetById(model.BookId);

            if (book is null)
                return NotFound();

            BookCopy copy = new()
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvailableForRental && model.IsAvailableForRental,
                CreatedById = User.GetUserId()
            };

            book.Copies.Add(copy);
            _unitOfWork.Complete();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _unitOfWork.BookCopies.Find(c => c.Id == id, 
                    includes: c => c.Include(x => x.Book)!);

            if (copy is null)
                return NotFound();

            var viewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            viewModel.ShowRentalInput = copy.Book!.IsAvailableForRental;

            return PartialView("Form", viewModel);
        }

        [HttpPost]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _unitOfWork.BookCopies.Find(c => c.Id == model.Id,
                    includes: c => c.Include(x => x.Book)!);

            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book!.IsAvailableForRental && model.IsAvailableForRental;
            copy.LastUpdatedById = User.GetUserId();
            copy.LastUpdatedOn = DateTime.Now;

            _unitOfWork.Complete();

            var viewModel = _mapper.Map<BookCopyViewModel>(copy);

            return PartialView("_BookCopyRow", viewModel);
        }

        public IActionResult RentalHistory(int id)
        {
            var copyHistory = _unitOfWork.RentalCopies
                .FindAll(predicate: c => c.BookCopyId == id,
                    includes: c => c.Include(x => x.Rental)!.ThenInclude(r => r!.Subscriber)!,
                    orderBy: c => c.RentalDate,
                    orderByDirection: OrderBy.Descending);
                //.Include(c => c.Rental)
                //.ThenInclude(r => r!.Subscriber)
                //.Where(c => c.BookCopyId == id)
                //.OrderByDescending(c => c.RentalDate)
                //.ToList();

            var viewModel = _mapper.Map<IEnumerable<CopyHistoryViewModel>>(copyHistory);

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var copy = _unitOfWork.BookCopies.GetById(id);

            if (copy is null)
            {
                return NotFound();
            }

            copy.IsDeleted = !copy.IsDeleted;
            copy.LastUpdatedById = User.GetUserId();
            copy.LastUpdatedOn = DateTime.UtcNow;

            _unitOfWork.Complete();

            return Ok();
        }
    }
}
