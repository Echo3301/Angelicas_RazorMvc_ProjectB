using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DbContext;
using AppMVC.Models.SeedViewModels;
using Services.Interfaces;

namespace AppMVC.Controllers
{
    public class SeedController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly ILogger<SeedController> _logger;
        private readonly MainDbContext _context;

        public SeedController(IAdminService adminService, ILogger<SeedController> logger, MainDbContext context)
        {
            _adminService = adminService;
            _logger = logger;
            _context = context;
        }

        //GET: SeedController
        public async Task<IActionResult> Index()
        {
            var viewModel = new SeedViewModel
            {
                FriendsCount = await _context.Friends.CountAsync(),
                AddressesCount = await _context.Addresses.CountAsync(),
                PetsCount = await _context.Pets.CountAsync(),
                QuotesCount = await _context.Quotes.CountAsync(),
                NrOfItemsToSeed = 100,
                RemoveSeeds = true
            };

            return View(viewModel);
        }

        //POST: Seed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SeedViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.RemoveSeeds)
                {
                    await _adminService.RemoveSeedAsync(true);
                    await _adminService.RemoveSeedAsync(false);
                }

                await _adminService.SeedAsync(model.NrOfItemsToSeed);
                _logger.LogInformation("RemoveSeeds = {RemoveSeeds}", model.RemoveSeeds);

                return RedirectToAction(nameof(Index));
            }

            model.FriendsCount = await _context.Friends.CountAsync();
            model.AddressesCount = await _context.Addresses.CountAsync();
            model.PetsCount = await _context.Pets.CountAsync();
            model.QuotesCount = await _context.Quotes.CountAsync();

            return View(model);
        }
    }
}
