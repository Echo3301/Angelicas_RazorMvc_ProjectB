using Microsoft.AspNetCore.Mvc.Rendering;
using Models.Interfaces;

namespace AppMVC.Models.MembersViewModels
{

    public class FriendsByCountryCityViewModel
    {
        public List<IFriend> Friends { get; set; } = new();

        public int PageSize { get; } = 10;
        public int ThisPage { get; set; }
        public int PrevPage => Math.Max(ThisPage - 1, 0);
        public int NextPage => Math.Min(ThisPage + 1, TotalPages - 1);

        public int TotalPages { get; set; }
        public int TotalFriends { get; set; }

        public string SelectedCountry { get; set; }
        public string SelectedCity { get; set; }

        public List<SelectListItem> CountrySelection { get; set; } = new();
        public List<SelectListItem> CitySelection { get; set; } = new();
    }
}
