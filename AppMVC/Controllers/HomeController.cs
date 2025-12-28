using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DbRepos;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using DbContext;
using AppMVC.Models.HomeViewModels;
using static AppMVC.Models.HomeViewModels.IndexViewModel;

namespace AppMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly FriendsDbRepos _friendsRepo;
        private readonly AddressesDbRepos _addressesRepo;
        private readonly PetsDbRepos _petsRepo;
        private readonly QuotesDbRepos _quotesRepo;
        private readonly IAdminService _adminService;
        private readonly MainDbContext _context;

        public HomeController(ILogger<HomeController> logger, IAdminService adminService, MainDbContext context, FriendsDbRepos friendsRepo, AddressesDbRepos addressesRepo, PetsDbRepos petsRepo, QuotesDbRepos quotesRepo)
        {
            _logger = logger;
            _friendsRepo = friendsRepo;
            _addressesRepo = addressesRepo;
            _petsRepo = petsRepo;
            _quotesRepo = quotesRepo;
            _adminService = adminService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new IndexViewModel
            {
                FriendsCount = await _context.Friends.CountAsync()
            };

            var countries = await _context.Addresses
                    .Where(a => !string.IsNullOrEmpty(a.Country))
                    .Select(a => a.Country)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

            if (countries.Count == 0)
            {
                viewModel.CountryCounts = new List<CountryListed>();
                return View(viewModel);
            }

            var grouped = await _context.Addresses
                    .Where(a => !string.IsNullOrEmpty(a.Country))
                    .SelectMany(a => a.FriendsDbM.Select(f => new { Country = a.Country, FriendId = f.FriendId }))
                    .Distinct()
                    .GroupBy(x => x.Country)
                    .Select(g => new { Country = g.Key, Count = g.Count() })
                    .ToListAsync();

            var dict = grouped.ToDictionary(x => x.Country, x => x.Count, StringComparer.OrdinalIgnoreCase);

            viewModel.CountryCounts = countries
                    .Select(c => new CountryListed
                    {
                        Country = c,
                        Count = dict.ContainsKey(c) ? dict[c] : 0
                    })
                    .ToList();

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}