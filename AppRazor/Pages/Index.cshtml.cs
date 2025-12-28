using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppRazor.Pages;

public class IndexModel : PageModel
{
    readonly IAdminService? _admin_service = null;
    readonly ILogger<SeedModel>? _logger = null;
    private readonly MainDbContext _context;

    public IndexModel(IAdminService admin_service, ILogger<SeedModel> logger, MainDbContext context)
    {
        _admin_service = admin_service;
        _logger = logger;
        _context = context;
    }

    public int FriendsCount { get; set; } = 0;

    public List<CountryListed> CountryCounts { get; set; } = new();

    public class CountryListed
    {
        public string? Country { get; set; }
        public int Count { get; set; }
    }

    public async Task OnGetAsync()
    {
        FriendsCount = await _context.Friends.CountAsync();

        var countries = await _context.Addresses
            .Where(a => !string.IsNullOrEmpty(a.Country))
            .Select(a => a.Country)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        if (countries.Count == 0)
        {
            CountryCounts = new List<CountryListed>();
            return;
        }

        var grouped = await _context.Addresses
                .Where(a => !string.IsNullOrEmpty(a.Country))
                .SelectMany(a => a.FriendsDbM.Select(f => new { Country = a.Country, FriendId = f.FriendId }))
                .Distinct()
                .GroupBy(x => x.Country)
                .Select(g => new { Country = g.Key, Count = g.Count() })
                .ToListAsync();

            var dict = grouped.ToDictionary(x => x.Country, x => x.Count, StringComparer.OrdinalIgnoreCase);
        CountryCounts = countries
            .Select(c => new CountryListed { Country = c, Count = dict.ContainsKey(c) ? dict[c] : 0 })
            .ToList();
    }
}
