using System.ComponentModel.DataAnnotations;
using Models.DTO;
using Models.Interfaces;

namespace AppMVC.Models.EditFriendViewModels
{
    public class FriendIM
    {
        public StatusIM StatusIM { get; set; }
        public Guid FriendId { get; set; }

        [Required(ErrorMessage = "You must provide a first name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "You must provide a last name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "You must provide a valid email")]
        [EmailAddress(ErrorMessage = "You must provide a valid email")]
        public string Email { get; set; }

        public DateTime? Birthday { get; set; }
        public Guid? AddressId { get; set; }

        public List<PetIM> Pets { get; set; } = new List<PetIM>();
        public List<QuoteIM> Quotes { get; set; } = new List<QuoteIM>();
        public PetIM NewPet { get; set; } = new PetIM();
        public QuoteIM NewQuote { get; set; } = new QuoteIM();
        public AddressIM Address { get; set; }

        public FriendIM() { }

        public FriendIM(IFriend model)
        {
            StatusIM = StatusIM.Unchanged;
            FriendId = model.FriendId;
            FirstName = model.FirstName;
            LastName = model.LastName;
            Email = model.Email;
            Birthday = model.Birthday;
            AddressId = model.Address?.AddressId;

            if (model.Address != null)
            {
                Address = new AddressIM(model.Address);
            }
            else
            {
                Address = new AddressIM { StatusIM = StatusIM.Unchanged };
            }

            Pets = model.Pets?.Select(p => new PetIM(p)).ToList() ?? new List<PetIM>();
            Quotes = model.Quotes?.Select(q => new QuoteIM(q)).ToList() ?? new List<QuoteIM>();
        }

        public IFriend UpdateModel(IFriend model)
        {
            model.FirstName = this.FirstName;
            model.LastName = this.LastName;
            model.Email = this.Email;
            model.Birthday = this.Birthday;
            return model;
        }

        public FriendCuDto CreateCUdto() => new()
        {
            FriendId = null,
            FirstName = this.FirstName,
            LastName = this.LastName,
            Email = this.Email,
            Birthday = this.Birthday,
            AddressId = this.AddressId
        };
    }
}