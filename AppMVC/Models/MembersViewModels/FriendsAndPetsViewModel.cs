namespace AppMVC.Models.MembersViewModels
{

    public class FriendsAndPetsViewModel
    {
        public string Country { get; set; } = string.Empty;
        public List<CityStat> CityStats { get; set; } = new();
    }

    public class CityStat
    {
        public string City { get; set; } = string.Empty;
        public int FriendsCount { get; set; }
        public int PetsCount { get; set; }
    }
}
