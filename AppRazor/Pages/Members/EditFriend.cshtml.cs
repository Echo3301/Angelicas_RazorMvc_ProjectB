using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Models.DTO;
using Models.Interfaces;
using Services.Interfaces;
using AppRazor.SeidoHelpers;

namespace AppRazor.Pages.Members
{
    public class EditFriendModel : PageModel
    {
        //Just like for WebApi
        readonly IFriendsService _friend_service = null;
        readonly IPetsService _pet_service = null;
        readonly IQuotesService _quote_service = null;
        readonly IAddressesService _address_service = null;
        readonly ILogger<EditFriendModel> _logger = null;

        //InputModel (IM) is locally declared classes that contains ONLY the properties of the Model
        //that are bound to the <form> tag
        //EVERY property must be bound to an <input> tag in the <form>
        [BindProperty]
        public FriendIM FriendInput { get; set; }

        //I also use BindProperty to keep between several posts, bound to hidden <input> field
        [BindProperty]
        public string PageHeader { get; set; }

        //Used to populate the dropdown select
        //Notice how it will be populate every time the class is instansiated, i.e. before every get and post
        public List<SelectListItem> AnimalKindItems { get; set; } = new List<SelectListItem>().PopulateSelectList<AnimalKind>();
        public List<SelectListItem> AnimalMoodItems { get; set; } = new List<SelectListItem>().PopulateSelectList<AnimalMood>();

        //For Validation
        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);

        #region HTTP Requests
        public async Task<IActionResult> OnGet()
        {
            if (Guid.TryParse(Request.Query["id"], out Guid _friendId))
            {
                //Read a friend from 
                var friend = await _friend_service.ReadFriendAsync(_friendId, false);

                //Populate the InputModel from the friend
                FriendInput = new FriendIM(friend.Item);
                PageHeader = "Edit details of a friend";

            }
            else
            {
                //Create an empty friend
                FriendInput = new FriendIM();
                FriendInput.StatusIM = StatusIM.Inserted;

                FriendInput.Address = new AddressIM();
                FriendInput.Address.StatusIM = StatusIM.Unchanged;

                PageHeader = "Create a new friend";
            }

            return Page();
        }

        public IActionResult OnPostDeletePet(Guid petId)
        {
            //Set the Pet as deleted, it will not be rendered
            FriendInput.Pets.First(p => p.PetId == petId).StatusIM = StatusIM.Deleted;

            return Page();
        }

        public IActionResult OnPostAddPet()
        {
            string[] keys = { "FriendInput.NewPet.Name",
                              "FriendInput.NewPet.Kind",
                              "FriendInput.NewPet.Mood"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Pet as Inserted, it will later be inserted in the database
            FriendInput.NewPet.StatusIM = StatusIM.Inserted;

            //Need to add a temp Guid so it can be deleted and editited in the form
            //A correct Guid will be created by the DTO when Inserted into the database
            FriendInput.NewPet.PetId = Guid.NewGuid();

            //Add it to the Input Models pets
            FriendInput.Pets.Add(new PetIM(FriendInput.NewPet));

            //Clear the NewPet so another pet can be added
            FriendInput.NewPet = new PetIM();

            return Page();
        }

        public IActionResult OnPostEditPet(Guid petId)
        {
            int idx = FriendInput.Pets.FindIndex(p => p.PetId == petId);
            string[] keys = { $"FriendInput.Pets[{idx}].editName",
                            $"FriendInput.Pets[{idx}].editKind",
                            $"FriendInput.Pets[{idx}].editMood"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Pet as Modified, it will later be updated in the database
            var p = FriendInput.Pets.First(p => p.PetId == petId);
            if (p.StatusIM != StatusIM.Inserted)
            {
                p.StatusIM = StatusIM.Modified;
            }

            //Implement the changes
            p.Name = p.editName;
            p.Kind = p.editKind;
            p.Mood = p.editMood;

            return Page();
        }


        public IActionResult OnPostDeleteQuote(Guid quoteId)
        {
            //Set the Quote as deleted, it will not be rendered
            FriendInput.Quotes.First(q => q.QuoteId == quoteId).StatusIM = StatusIM.Deleted;

            return Page();
        }

        public IActionResult OnPostAddQuote()
        {
            string[] keys = { "FriendInput.NewQuote.QuoteText",
                              "FriendInput.NewQuote.Author"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Quote as Inserted, it will later be inserted in the database
            FriendInput.NewQuote.StatusIM = StatusIM.Inserted;

            //Need to add a temp Guid so it can be deleted and editited in the form
            //A correct Guid will be created by the DTO when Inserted into the database
            FriendInput.NewQuote.QuoteId = Guid.NewGuid();

            //Add it to the Input Models quotes
            FriendInput.Quotes.Add(new QuoteIM(FriendInput.NewQuote));

            //Clear the NewQuote so another quote can be added
            FriendInput.NewQuote = new QuoteIM();

            return Page();
        }

        public IActionResult OnPostEditQuote(Guid quoteId)
        {
            int idx = FriendInput.Quotes.FindIndex(q => q.QuoteId == quoteId);
            string[] keys = { $"FriendInput.Quotes[{idx}].editQuoteText",
                            $"FriendInput.Quotes[{idx}].editAuthor"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            //Set the Quote as Modified, it will later be updated in the database
            var q = FriendInput.Quotes.First(q => q.QuoteId == quoteId);
            if (q.StatusIM != StatusIM.Inserted)
            {
                q.StatusIM = StatusIM.Modified;
            }

            //Implement the changes
            q.QuoteText = q.editQuoteText;
            q.Author = q.editAuthor;

            return Page();
        }

        public async Task<IActionResult> OnPostUndo()
        {
            //Reload Friend from Database
            var friend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);

            //Repopulate the InputModel
            FriendInput = new FriendIM(friend.Item);
            return Page();
        }

        public async Task<IActionResult> OnPostSave()
        {
            string[] keys = { "FriendInput.FirstName",
                      "FriendInput.LastName",
                      "FriendInput.Email"};

            if (!ModelState.IsValidPartially(out ModelValidationResult validationResult, keys))
            {
                ValidationResult = validationResult;
                return Page();
            }

            try
            {
                //This is where the music plays
                //First, are we creating a new Friend or editing another
                if (FriendInput.StatusIM == StatusIM.Inserted)
                {
                    var newFriend = await _friend_service.CreateFriendAsync(FriendInput.CreateCUdto());
                    //get the newly created FriendId
                    FriendInput.FriendId = newFriend.Item.FriendId;
                }

                //Do all updates for Address
                await SaveAddress();

                //Do all updates for Pets
                await SavePets();

                //Do all updates for Quotes
                var friend = await SaveQuotes();

                //Finally, update the Friend itself
                friend = FriendInput.UpdateModel(friend);
                await _friend_service.UpdateFriendAsync(new FriendCuDto(friend));

                if (FriendInput.StatusIM == StatusIM.Inserted)
                {
                    return Redirect($"~/Members/ListOfFriends");
                }

                return Redirect($"~/Members/FriendsDetails?id={FriendInput.FriendId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving friend");
                ValidationResult = new ModelValidationResult(true,
                    new[] { "An error occurred while saving. Please try again." }, null);
                return Page();
            }
        }
        #endregion

        #region InputModel Pets, Quotes and Address saved to database
        private async Task SaveAddress()
        {
            if (FriendInput.Address == null)
            {
                FriendInput.AddressId = null;
                return;
            }

            bool hasAddressData = !string.IsNullOrWhiteSpace(FriendInput.Address.StreetAddress) ||
                                 !string.IsNullOrWhiteSpace(FriendInput.Address.City) ||
                                 !string.IsNullOrWhiteSpace(FriendInput.Address.Country) ||
                                 !string.IsNullOrWhiteSpace(FriendInput.Address.ZipCode);

            if (!hasAddressData)
            {
                FriendInput.AddressId = null;
                return;
            }
            try
            {
                var existingAddress = await FindExistingAddress(FriendInput.Address);

                if (existingAddress != null)
                {
                    FriendInput.AddressId = existingAddress.AddressId;
                    FriendInput.Address.AddressId = existingAddress.AddressId;
                    return;
                }

                if (!FriendInput.Address.AddressId.HasValue)
                {
                    var cuDto = FriendInput.Address.CreateCUdto();
                    var newAddress = await _address_service.CreateAddressAsync(cuDto);

                    if (newAddress?.Item != null)
                    {
                        FriendInput.AddressId = newAddress.Item.AddressId;
                        FriendInput.Address.AddressId = newAddress.Item.AddressId;
                    }
                }
                else
                {
                    var addressResponse = await _address_service.ReadAddressAsync(FriendInput.Address.AddressId.Value, false);

                    if (addressResponse?.Item != null)
                    {
                        var updatedAddress = FriendInput.Address.UpdateModel(addressResponse.Item);
                        await _address_service.UpdateAddressAsync(new AddressCuDto(updatedAddress));
                        FriendInput.AddressId = FriendInput.Address.AddressId;
                    }
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("already exist"))
            {
                throw new ArgumentException(
                    $"Address duplicate: The address '{FriendInput.Address.StreetAddress}, " +
                    $"{FriendInput.Address.ZipCode} {FriendInput.Address.City}, {FriendInput.Address.Country}' " +
                    $"already exists in the database. {ex.Message}", ex);
            }
        }

        private async Task<IAddress?> FindExistingAddress(AddressIM address)
        {
            try
            {
                var addresses = await _address_service.ReadAddressesAsync(true, true, "", 0, 1000);
                return addresses.PageItems.FirstOrDefault(a =>
                    a.StreetAddress == address.StreetAddress &&
                    a.ZipCode.ToString() == address.ZipCode &&
                    a.City == address.City &&
                    a.Country == address.Country);
            }
            catch
            {
                return null;
            }
        }

        private async Task<IFriend> SavePets()
        {
            //Check if there are deleted pets, if so simply remove them
            var deletedPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedPets)
            {
                //Remove from the database
                await _pet_service.DeletePetAsync(item.PetId);
            }

            //Note that now the deleted pets will be removed and I can focus on Pet creation
            await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);

            //Check if there are any new pets added, if so create them in the database
            var newPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Inserted));
            foreach (var item in newPets)
            {
                //Create the corresposning model and CUdto objects
                var cuDto = item.CreateCUdto();

                //Set the relationships of a newly created item and write to database
                cuDto.FriendId = FriendInput.FriendId;
                await _pet_service.CreatePetAsync(cuDto);
            }

            //Note that now the deleted pets will be removed and created pets added. I can focus on Pet update
            var friend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);

            //Check if there are any modified pets , if so update them in the database
            var modifiedPets = FriendInput.Pets.FindAll(p => (p.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedPets)
            {
                var model = friend.Item.Pets.First(p => p.PetId == item.PetId);

                //Update the model from the InputModel
                model = item.UpdateModel(model);

                //Update the model in the database
                await _pet_service.UpdatePetAsync(new PetCuDto(model));
            }

            return friend.Item;
        }

        private async Task<IFriend> SaveQuotes()
        {
            //Check if there are deleted quotes, if so simply remove them
            var deletedQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Deleted));
            foreach (var item in deletedQuotes)
            {
                //Remove from the database
                await _quote_service.DeleteQuoteAsync(item.QuoteId);
            }

            //Check if there are any new quote added, if so create them in the database
            var newQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Inserted));
            foreach (var item in newQuotes)
            {
                try
                {
                    //Create the corresposning model and CUdto objects
                    var cuDto = item.CreateCUdto();

                    //Set the relationships of a newly created item and write to database
                    cuDto.FriendsId = [FriendInput.FriendId];

                    //Create if does not exists. 
                    await _quote_service.CreateQuoteAsync(cuDto);
                }
                catch (ArgumentException ex) when (ex.Message.Contains("already exist"))
                {
                    throw new ArgumentException(
                        $"Quote duplicate: The quote \"{item.QuoteText}\" by {item.Author} " +
                        $"already exists in the database. {ex.Message}", ex);
                }
            }

            //To update modified and deleted Quotes, lets first read the original
            //Note that now the deleted quotes will be removed and created quotes will be nicely included
            var friend = await _friend_service.ReadFriendAsync(FriendInput.FriendId, false);


            //Check if there are any modified quotes , if so update them in the database
            var modifiedQuotes = FriendInput.Quotes.FindAll(q => (q.StatusIM == StatusIM.Modified));
            foreach (var item in modifiedQuotes)
            {
                try
                {
                    var model = friend.Item.Quotes.First(q => q.QuoteId == item.QuoteId);

                    //Update the model from the InputModel
                    model = item.UpdateModel(model);

                    //Update the model in the database
                    await _quote_service.UpdateQuoteAsync(new QuoteCuDto(model));
                }
                catch (ArgumentException ex) when (ex.Message.Contains("already exist"))
                {
                    throw new ArgumentException(
                        $"Quote duplicate: Cannot update to \"{item.QuoteText}\" by {item.Author} " +
                        $"because this quote already exists. {ex.Message}", ex);
                }
            }
            return friend.Item;
        }
        #endregion

        #region Constructors
        //Inject services just like in WebApi
        public EditFriendModel(IFriendsService friend_service, IPetsService pet_service,
                              IQuotesService quote_service, IAddressesService address_service,
                              ILogger<EditFriendModel> logger)
        {
            _friend_service = friend_service;
            _pet_service = pet_service;
            _quote_service = quote_service;
            _address_service = address_service;
            _logger = logger;
        }
        #endregion

        #region Input Model
        //InputModel (IM) is locally declared classes that contains ONLY the properties of the Model
        //that are bound to the <form> tag
        //EVERY property must be bound to an <input> tag in the <form>
        //These classes are in center of ModelBinding and Validation
        public enum StatusIM { Unknown, Unchanged, Inserted, Modified, Deleted }

        public class PetIM
        {
            public StatusIM StatusIM { get; set; }

            public Guid PetId { get; set; }

            [Required(ErrorMessage = "You must provide a pet name")]
            public string Name { get; set; }

            [Required(ErrorMessage = "You must select an animal kind")]
            public AnimalKind Kind { get; set; }

            [Required(ErrorMessage = "You must select an animal mood")]
            public AnimalMood Mood { get; set; }

            //This is because I want to confirm modifications in PostEditPet
            [Required(ErrorMessage = "You must provide a pet name")]
            public string editName { get; set; }

            [Required(ErrorMessage = "You must select an animal kind")]
            public AnimalKind editKind { get; set; }

            [Required(ErrorMessage = "You must select an animal mood")]
            public AnimalMood editMood { get; set; }

            public PetIM() { }
            public PetIM(PetIM original)
            {
                StatusIM = original.StatusIM;
                PetId = original.PetId;
                Name = original.Name;
                Kind = original.Kind;
                Mood = original.Mood;

                editName = original.editName;
                editKind = original.editKind;
                editMood = original.editMood;
            }
            public PetIM(IPet model)
            {
                StatusIM = StatusIM.Unchanged;
                PetId = model.PetId;
                Name = editName = model.Name;
                Kind = editKind = model.Kind;
                Mood = editMood = model.Mood;
            }

            //to update the model in database
            public IPet UpdateModel(IPet model)
            {
                model.PetId = this.PetId;
                model.Name = this.Name;
                model.Kind = this.Kind;
                model.Mood = this.Mood;
                return model;
            }

            //to create new pet in the database
            public PetCuDto CreateCUdto() => new PetCuDto()
            {

                PetId = null,
                Name = this.Name,
                Kind = this.Kind,
                Mood = this.Mood
            };
        }

        public class QuoteIM
        {
            public StatusIM StatusIM { get; set; }

            public Guid QuoteId { get; set; }

            [Required(ErrorMessage = "You must provide a quote")]
            public string QuoteText { get; set; }

            [Required(ErrorMessage = "You must provide an author")]
            public string Author { get; set; }

            [Required(ErrorMessage = "You must provide a quote")]
            public string editQuoteText { get; set; }

            [Required(ErrorMessage = "You must provide an author")]
            public string editAuthor { get; set; }

            public QuoteIM() { }
            public QuoteIM(QuoteIM original)
            {
                StatusIM = original.StatusIM;
                QuoteId = original.QuoteId;
                QuoteText = original.QuoteText;
                Author = original.Author;


                editQuoteText = original.editQuoteText;
                editAuthor = original.editAuthor;
            }
            public QuoteIM(IQuote model)
            {
                StatusIM = StatusIM.Unchanged;
                QuoteId = model.QuoteId;
                QuoteText = editQuoteText = model.QuoteText;
                Author = editAuthor = model.Author;
            }

            //to update the model in database
            public IQuote UpdateModel(IQuote model)
            {
                model.QuoteId = this.QuoteId;
                model.QuoteText = this.QuoteText;
                model.Author = this.Author;
                return model;
            }

            //to create new quote in the database
            public QuoteCuDto CreateCUdto() => new QuoteCuDto()
            {

                QuoteId = null,
                Quote = this.QuoteText,
                Author = this.Author
            };
        }

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
                    Address = new AddressIM();
                    Address.StatusIM = StatusIM.Unchanged;
                }

                Pets = model.Pets?.Select(p => new PetIM(p)).ToList();
                Quotes = model.Quotes?.Select(q => new QuoteIM(q)).ToList();
            }

            //to update the model in database
            public IFriend UpdateModel(IFriend model)
            {
                model.FirstName = this.FirstName;
                model.LastName = this.LastName;
                model.Email = this.Email;
                model.Birthday = this.Birthday;
                return model;
            }

            //to create new friend in the database
            public FriendCuDto CreateCUdto() => new()
            {
                FriendId = null,
                FirstName = this.FirstName,
                LastName = this.LastName,
                Email = this.Email,
                Birthday = this.Birthday,
                AddressId = this.AddressId
            };

            //to allow a new pet being specified and bound in the form
            public PetIM NewPet { get; set; } = new PetIM();

            //to allow a new quote being specified and bound in the form
            public QuoteIM NewQuote { get; set; } = new QuoteIM();

            //Address
            public AddressIM Address { get; set; }
        }
        #endregion
    }
}