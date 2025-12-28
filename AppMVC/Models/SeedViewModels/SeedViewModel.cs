using System.ComponentModel.DataAnnotations;
namespace AppMVC.Models.SeedViewModels;

public class SeedViewModel
{
    public int FriendsCount { get; set; } = 0;
    public int AddressesCount { get; set; } = 0;
    public int PetsCount { get; set; } = 0;
    public int QuotesCount { get; set; } = 0;

    [Required(ErrorMessage = "You must enter nr of items to seed")]
    [Range(1, 10000, ErrorMessage = "Number must be between 1 and 10000")]
    public int NrOfItemsToSeed { get; set; } = 100;

    public bool RemoveSeeds { get; set; } = true;
}
