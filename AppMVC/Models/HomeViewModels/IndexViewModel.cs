namespace AppMVC.Models.HomeViewModels;

public class IndexViewModel
{
    public int FriendsCount { get; set; } = 10;
    public int AddressesCount { get; set; } = 10;
    public int PetsCount { get; set; } = 10;
    public int QuotesCount { get; set; } = 10;

    public List<CountryListed> CountryCounts { get; set; } = new();

    public class CountryListed
    {
        public string? Country { get; set; }
        public int Count { get; set; }
    }
}