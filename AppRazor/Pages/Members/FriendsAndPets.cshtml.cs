using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using DbContext;

namespace AppRazor.Pages.Members
{
    public class FriendsAndPetsModel : PageModel
    {
        private readonly MainDbContext _context;

        public FriendsAndPetsModel(MainDbContext context)
        {
            _context = context;
        }

        public string Country { get; set; } = string.Empty;

        public List<CityStat> CityStats { get; set; } = new();

        public class CityStat
        {
            public string City { get; set; } = string.Empty;
            public int FriendsCount { get; set; }
            public int PetsCount { get; set; }
        }
        public async Task<IActionResult> OnGetAsync(string country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return RedirectToPage("/Index");
            }

            Country = country;

            var friends = await _context.Friends
                    .Where(f => f.AddressDbM != null && f.AddressDbM.Country == country)
                    .Select(f => new { f.FriendId, City = f.AddressDbM.City })
                    .ToListAsync();

            if (friends.Count == 0)
            {
                CityStats = new List<CityStat>();
                return Page();
            }

            var friendIds = friends.Select(f => f.FriendId).Distinct().ToList();
            var pets = await _context.Pets
                .Where(p => friendIds.Contains(p.FriendId))
                .Select(p => new { p.PetId, p.FriendId })
                .ToListAsync();

            CityStats = friends
            .GroupBy(f => string.IsNullOrWhiteSpace(f.City) ? "Unknown" : f.City!)
            .Select(g => new CityStat
            {
                City = g.Key,
                FriendsCount = g.Select(x => x.FriendId).Distinct().Count(),
                PetsCount = pets.Count(p => g.Select(x => x.FriendId).Contains(p.FriendId))
            })
            .OrderBy(s => s.City)
            .ToList();

            return Page();
        }

    }
}
