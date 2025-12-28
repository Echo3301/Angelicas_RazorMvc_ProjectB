using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DbContext;
using AppMVC.Models.MembersViewModels;
using Services.Interfaces;

namespace AppMVC.Controllers
{
    public class MembersController : Controller
    {
        private readonly IFriendsService _service;
        private readonly MainDbContext _context;
        private readonly ILogger<MembersController> _logger;

        public MembersController(MainDbContext context, IFriendsService service, ILogger<MembersController> logger)
        {
            _service = service;
            _context = context;
            _logger = logger;
        }

        #region FriendsAndPets
        //GET: Members/FriendsAndPets?country=CountryName
        public async Task<IActionResult> FriendsAndPets(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return RedirectToAction("Index", "Home");
            }

            var viewModel = new FriendsAndPetsViewModel
            {
                Country = country
            };

            var friends = await _context.Friends
                .Where(f => f.AddressDbM != null && f.AddressDbM.Country == country)
                .Select(f => new { f.FriendId, City = f.AddressDbM.City })
                .ToListAsync();

            if (friends.Count == 0)
            {
                viewModel.CityStats = new List<CityStat>();
                return View(viewModel);
            }

            var friendIds = friends.Select(f => f.FriendId).Distinct().ToList();

            var pets = await _context.Pets
                .Where(p => friendIds.Contains(p.FriendId))
                .Select(p => new { p.PetId, p.FriendId })
                .ToListAsync();

            viewModel.CityStats = friends
                .GroupBy(f => string.IsNullOrWhiteSpace(f.City) ? "Unknown" : f.City!)
                .Select(g => new CityStat
                {
                    City = g.Key,
                    FriendsCount = g.Select(x => x.FriendId).Distinct().Count(),
                    PetsCount = pets.Count(p => g.Select(x => x.FriendId).Contains(p.FriendId))
                })
                .OrderBy(s => s.City)
                .ToList();

            return View(viewModel);
        }
        #endregion

        #region FriendsByCountryCity
        //GET: Members/FriendsByCountryCity
        public async Task<IActionResult> FriendsByCountryCity(
            string selectedCountry = null,
            string selectedCity = null,
            int pagenr = 0)
        {
            var viewModel = new FriendsByCountryCityViewModel
            {
                ThisPage = pagenr,
                SelectedCountry = selectedCountry,
                SelectedCity = selectedCity
            };

            var countries = await _context.Addresses
                .Where(a => !string.IsNullOrEmpty(a.Country))
                .Select(a => a.Country)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            viewModel.CountrySelection = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All countries", Selected = string.IsNullOrEmpty(selectedCountry) }
            };
            viewModel.CountrySelection.AddRange(
                countries.Select(c => new SelectListItem { Value = c, Text = c, Selected = c == selectedCountry })
            );

            var citiesQuery = _context.Addresses.AsQueryable();
            if (!string.IsNullOrEmpty(selectedCountry))
                citiesQuery = citiesQuery.Where(a => a.Country == selectedCountry);

            var cities = await citiesQuery
                .Where(a => !string.IsNullOrEmpty(a.City))
                .Select(a => a.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            viewModel.CitySelection = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "All cities", Selected = string.IsNullOrEmpty(selectedCity) }
            };
            viewModel.CitySelection.AddRange(
                cities.Select(c => new SelectListItem { Value = c, Text = c, Selected = c == selectedCity })
            );

            var resp = await _service.ReadFriendsAsync(
                seeded: true,
                flat: false,
                country: string.IsNullOrWhiteSpace(selectedCountry) ? null : selectedCountry,
                city: string.IsNullOrWhiteSpace(selectedCity) ? null : selectedCity,
                filter: null,
                pageNumber: pagenr,
                pageSize: viewModel.PageSize
            );

            viewModel.Friends = resp.PageItems;
            viewModel.TotalFriends = resp.DbItemsCount;
            viewModel.TotalPages = (int)Math.Ceiling((double)viewModel.TotalFriends / viewModel.PageSize);

            return View(viewModel);
        }

        //POST: Members/DeleteFriendPetsAndQuotes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFriendPetsAndQuotes(
            Guid friendId,
            int thisPage = 0,
            string selectedCountry = null,
            string selectedCity = null)
        {
            try
            {
                int petCount = 0;
                int quoteCount = 0;

                var pets = await _context.Pets
                    .Where(p => p.FriendId == friendId)
                    .ToListAsync();

                if (pets.Any())
                {
                    petCount = pets.Count;
                    _context.Pets.RemoveRange(pets);
                }

                var friend = await _context.Friends
                    .Include(f => f.QuotesDbM)
                    .FirstOrDefaultAsync(f => f.FriendId == friendId);

                if (friend?.QuotesDbM != null && friend.QuotesDbM.Any())
                {
                    quoteCount = friend.QuotesDbM.Count;
                    friend.QuotesDbM.Clear();
                }

                await _context.SaveChangesAsync();

                _logger?.LogInformation(
                    "Deleted {PetCount} pets and {QuoteCount} quotes for friend {FriendId}",
                    petCount, quoteCount, friendId
                );

                TempData["SuccessMessage"] = $"Successfully deleted {petCount} pets and {quoteCount} quotes.";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting pets and quotes for friend {FriendId}", friendId);
                TempData["ErrorMessage"] = "An error occurred while deleting.";
            }

            return RedirectToAction(nameof(FriendsByCountryCity), new
            {
                pagenr = thisPage,
                selectedCountry,
                selectedCity
            });
        }
        #endregion

        #region FriendsDetails
        //GET: Members/FriendsDetails/
        public async Task<IActionResult> FriendsDetails(Guid? id)
        {
            if (id == null)
            {
                _logger.LogWarning("FriendDetails called with null id");
                return RedirectToAction("FriendsByCountryCity");
            }

            var result = await _service.ReadFriendAsync(id.Value, false);

            if (result.Item == null)
            {
                _logger.LogWarning("Friend with id {FriendId} not found", id);
                TempData["ErrorMessage"] = "Friend not found.";
                return RedirectToAction("FriendsByCountryCity");
            }

            var viewModel = new FriendsDetailsViewModel
            {
                Friend = result.Item
            };

            return View(viewModel);
        }
        #endregion
    }
}
