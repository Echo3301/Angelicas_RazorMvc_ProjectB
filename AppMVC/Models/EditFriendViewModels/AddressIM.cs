using System.ComponentModel.DataAnnotations;
using Models.DTO;
using Models.Interfaces;

namespace AppMVC.Models.EditFriendViewModels
{
    public class AddressIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid? AddressId { get; set; }

        public string StreetAddress { get; set; }

        [RegularExpression(@"^[0-9]{3,10}$", ErrorMessage = "Zip code must be 3-10 digits")]
        public string ZipCode { get; set; }

        public string City { get; set; }
        public string Country { get; set; }

        public AddressIM() { }

        public AddressIM(IAddress model)
        {
            StatusIM = StatusIM.Unchanged;
            AddressId = model.AddressId;
            StreetAddress = model.StreetAddress;
            ZipCode = model.ZipCode.ToString();
            City = model.City;
            Country = model.Country;
        }

        public IAddress UpdateModel(IAddress model)
        {
            model.StreetAddress = this.StreetAddress;
            model.ZipCode = string.IsNullOrWhiteSpace(this.ZipCode) ? 0 : int.Parse(this.ZipCode);
            model.City = this.City;
            model.Country = this.Country;
            return model;
        }

        public AddressCuDto CreateCUdto() => new AddressCuDto()
        {
            AddressId = null,
            StreetAddress = this.StreetAddress,
            ZipCode = string.IsNullOrWhiteSpace(this.ZipCode) ? 0 : int.Parse(this.ZipCode),
            City = this.City,
            Country = this.Country
        };
    }
}