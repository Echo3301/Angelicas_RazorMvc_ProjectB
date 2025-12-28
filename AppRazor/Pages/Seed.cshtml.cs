using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using DbContext;

namespace AppRazor.Pages
{

public class SeedModel : PageModel
{
    //Just like for WebApi
    readonly IAdminService _admin_service = null;
    readonly ILogger<SeedModel> _logger = null;
    private readonly MainDbContext _context;

    public int FriendsCount { get; set; } = 0;
    public int AddressesCount { get; set; } = 0;
    public int PetsCount { get; set; } = 0;
    public int QuotesCount { get; set; } = 0;

    [BindProperty]
    [Required(ErrorMessage = "You must enter nr of items to seed")]
    public int NrOfItemsToSeed { get; set; } = 100;

    [BindProperty]
    public bool RemoveSeeds { get; set; } = true;

    public async Task OnGetAsync()
    {
        FriendsCount = await _context.Friends.CountAsync();
        AddressesCount = await _context.Addresses.CountAsync();
        PetsCount = await _context.Pets.CountAsync();
        QuotesCount = await _context.Quotes.CountAsync();
    }

    public async Task<IActionResult> OnPost()
    {
        if (ModelState.IsValid)
        {
            if (RemoveSeeds)
            {
                await _admin_service.RemoveSeedAsync(true);
                await _admin_service.RemoveSeedAsync(false);
            }
            await _admin_service.SeedAsync(NrOfItemsToSeed);

            _logger.LogInformation("RemoveSeeds = {RemoveSeeds}", RemoveSeeds);

            return Redirect($"~/Seed");
        }
        return Page();
    }

    //Inject services just like in WebApi
    public SeedModel(IAdminService admin_service, ILogger<SeedModel> logger, MainDbContext context)
    {
        _admin_service = admin_service;
        _logger = logger;
        _context = context;
    }
}
}
