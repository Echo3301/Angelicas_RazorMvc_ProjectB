using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DbContext;
using Models.Interfaces;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppRazor.Pages.Members
{
    public class FriendsByCountryCityModel : PageModel
    {
        private readonly IFriendsService? _service = null;
        private readonly ILogger<FriendsByCountryCityModel>? _logger = null;
        private readonly MainDbContext _context;

        public FriendsByCountryCityModel(MainDbContext context, IFriendsService? service, ILogger<FriendsByCountryCityModel>? logger)
        {
            _context = context;
            _service = service;
            _logger = logger;
        }

        public List<IFriend> Friends { get; set; } = new();

        public int PageSize { get; } = 10;
        [BindProperty]
        public int ThisPage { get; set; }
        public int PrevPage => Math.Max(ThisPage - 1, 0);
        public int NextPage => Math.Min(ThisPage + 1, TotalPages - 1);

        public int TotalPages { get; set; }
        public int TotalFriends { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedCountry { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SelectedCity { get; set; }

        [BindProperty]
        public Guid FriendId { get; set; }

        public List<SelectListItem> CountrySelection { get; set; } = new();
        public List<SelectListItem> CitySelection { get; set; } = new();
        private bool seededFilter = true;

        public async Task<IActionResult> OnGet()
        {
            if (int.TryParse(Request.Query["pagenr"], out int page))
                ThisPage = page;
            else
                ThisPage = 0;

            var countries = await _context.Addresses
                .Where(a => !string.IsNullOrEmpty(a.Country))
                .Select(a => a.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            CountrySelection = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All countries", Selected = string.IsNullOrEmpty(SelectedCountry) }
            };
            CountrySelection.AddRange(countries.Select(c => new SelectListItem { Value = c, Text = c, Selected = c == SelectedCountry }));

            var citiesQuery = _context.Addresses.AsQueryable();
            if (!string.IsNullOrEmpty(SelectedCountry))
                citiesQuery = citiesQuery.Where(a => a.Country == SelectedCountry);

            var cities = await citiesQuery
                .Where(a => !string.IsNullOrEmpty(a.City))
                .Select(a => a.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            CitySelection = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All cities", Selected = string.IsNullOrEmpty(SelectedCity) }
            };
            CitySelection.AddRange(cities.Select(c => new SelectListItem { Value = c, Text = c, Selected = c == SelectedCity }));

            var resp = await _service.ReadFriendsAsync(
                 seeded: seededFilter,
                 flat: false,
                 country: string.IsNullOrWhiteSpace(SelectedCountry) ? null : SelectedCountry,
                 city: string.IsNullOrWhiteSpace(SelectedCity) ? null : SelectedCity,
                 filter: null,
                 pageNumber: ThisPage,
                 pageSize: PageSize
             );

            Friends = resp.PageItems;
            TotalFriends = resp.DbItemsCount;
            TotalPages = (int)Math.Ceiling((double)TotalFriends / PageSize);

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            try
            {
                int petCount = 0;
                int quoteCount = 0;

                var pets = await _context.Pets
                .Where(p => p.FriendId == FriendId)
                .ToListAsync();

                if (pets.Any())
                {
                    petCount = pets.Count;
                    _context.Pets.RemoveRange(pets);
                }

                var friend = await _context.Friends
                .Include(f => f.QuotesDbM)
                .FirstOrDefaultAsync(f => f.FriendId == FriendId);

                if (friend?.QuotesDbM != null && friend.QuotesDbM.Any())
                {
                    quoteCount = friend.QuotesDbM.Count;
                    friend.QuotesDbM.Clear();
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation("Deleted {PetCount} pets and {QuoteCount} quotes for friend {FriendId}",
                    petCount, quoteCount, FriendId);

                TempData["SuccessMessage"] = $"Successfully deleted {petCount} pets and {quoteCount} quotes.";

                return RedirectToPage(new { pagenr = ThisPage, SelectedCountry, SelectedCity });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting pets and quotes for friend {FriendId}", FriendId);
                TempData["ErrorMessage"] = "An error occurred while deleting. Please try again.";
                return RedirectToPage(new { pagenr = ThisPage, SelectedCountry, SelectedCity });
            }
        }
    }
}