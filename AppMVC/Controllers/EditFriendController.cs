using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AppMVC.Models.EditFriendViewModels;
using Services.Interfaces;
using Models.DTO;
using Models.Interfaces;

namespace AppMVC.Controllers
{
    public class EditFriendController : Controller
    {
        private readonly IFriendsService _friend_service;
        private readonly IPetsService _pet_service;
        private readonly IQuotesService _quote_service;
        private readonly IAddressesService _address_service;
        private readonly ILogger<EditFriendController> _logger;

        public EditFriendController(
            IFriendsService friend_service,
            IPetsService pet_service,
            IQuotesService quote_service,
            IAddressesService address_service,
            ILogger<EditFriendController> logger)
        {
            _friend_service = friend_service;
            _pet_service = pet_service;
            _quote_service = quote_service;
            _address_service = address_service;
            _logger = logger;
        }

        #region Helper to populate ViewModel
        private EditFriendViewModel CreateViewModel(FriendIM friendInput = null, string pageHeader = null)
        {
            var viewModel = new EditFriendViewModel
            {
                FriendInput = friendInput ?? new FriendIM(),
                PageHeader = pageHeader ?? "Edit Friend"
            };
            PopulateDropdowns(viewModel);
            return viewModel;
        }

        private void PopulateDropdowns(EditFriendViewModel viewModel)
        {
            viewModel.AnimalKindItems = Enum.GetValues(typeof(AnimalKind))
                .Cast<AnimalKind>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();

            viewModel.AnimalMoodItems = Enum.GetValues(typeof(AnimalMood))
                .Cast<AnimalMood>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();
        }
        #endregion

        #region GET: EditFriend
        //GET: EditFriend/EditFriend?id={guid}
        public async Task<IActionResult> EditFriend(Guid? id)
        {
                try
                {
                    var friend = await _friend_service.ReadFriendAsync(id.Value, false);

                    if (friend?.Item == null)
                    {
                        return NotFound();
                    }

                    var viewModel = CreateViewModel(
                        new FriendIM(friend.Item),
                        "Edit details of a friend"
                    );

                    return View(viewModel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading friend {FriendId}", id.Value);
                    return NotFound();
                }    
        }
        #endregion

        #region POST: Pet Actions
        //POST: EditFriend/AddPet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPet(EditFriendViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.FriendInput.NewPet.Name))
            {
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "Pet name is required." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }

            viewModel.FriendInput.NewPet.StatusIM = StatusIM.Inserted;
            viewModel.FriendInput.NewPet.PetId = Guid.NewGuid();

            viewModel.FriendInput.Pets.Add(new PetIM(viewModel.FriendInput.NewPet));

            //Clear for next add
            viewModel.FriendInput.NewPet = new PetIM();

            PopulateDropdowns(viewModel);
            return View("EditFriend", viewModel);
        }

        //POST: EditFriend/EditPet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditPet(EditFriendViewModel viewModel, Guid petId)
        {
            var pet = viewModel.FriendInput.Pets.FirstOrDefault(p => p.PetId == petId);
            if (pet == null)
            {
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "Pet not found." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }

            if (string.IsNullOrWhiteSpace(pet.editName))
            {
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "Pet name is required." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }

            if (pet.StatusIM != StatusIM.Inserted)
            {
                pet.StatusIM = StatusIM.Modified;
            }

            pet.Name = pet.editName;
            pet.Kind = pet.editKind;
            pet.Mood = pet.editMood;

            PopulateDropdowns(viewModel);
            return View("EditFriend", viewModel);
        }

        //POST: EditFriend/DeletePet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePet(EditFriendViewModel viewModel, Guid petId)
        {
            var pet = viewModel.FriendInput.Pets.FirstOrDefault(p => p.PetId == petId);
            if (pet != null)
            {
                pet.StatusIM = StatusIM.Deleted;
            }

            PopulateDropdowns(viewModel);
            return View("EditFriend", viewModel);
        }
        #endregion

        #region POST: Quote Actions
        //POST: EditFriend/AddQuote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddQuote(EditFriendViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.FriendInput.NewQuote.QuoteText) ||
                string.IsNullOrWhiteSpace(viewModel.FriendInput.NewQuote.Author))
            {
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "Quote text and author are required." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }

            viewModel.FriendInput.NewQuote.StatusIM = StatusIM.Inserted;
            viewModel.FriendInput.NewQuote.QuoteId = Guid.NewGuid();

            viewModel.FriendInput.Quotes.Add(new QuoteIM(viewModel.FriendInput.NewQuote));

            viewModel.FriendInput.NewQuote = new QuoteIM();

            PopulateDropdowns(viewModel);
            return View("EditFriend", viewModel);
        }

        //POST: EditFriend/EditQuote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditQuote(EditFriendViewModel viewModel, Guid quoteId)
        {
            var quote = viewModel.FriendInput.Quotes.FirstOrDefault(q => q.QuoteId == quoteId);
            if (quote == null)
            {
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "Quote not found." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }

            if (string.IsNullOrWhiteSpace(quote.editQuoteText) ||
                string.IsNullOrWhiteSpace(quote.editAuthor))
            {
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "Quote text and author are required." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }

            if (quote.StatusIM != StatusIM.Inserted)
            {
                quote.StatusIM = StatusIM.Modified;
            }

            quote.QuoteText = quote.editQuoteText;
            quote.Author = quote.editAuthor;

            PopulateDropdowns(viewModel);
            return View("EditFriend", viewModel);
        }

        //POST: EditFriend/DeleteQuote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteQuote(EditFriendViewModel viewModel, Guid quoteId)
        {
            var quote = viewModel.FriendInput.Quotes.FirstOrDefault(q => q.QuoteId == quoteId);
            if (quote != null)
            {
                quote.StatusIM = StatusIM.Deleted;
            }

            PopulateDropdowns(viewModel);
            return View("EditFriend", viewModel);
        }
        #endregion

        #region POST: Undo and Save
        //POST: EditFriend/Undo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Undo(EditFriendViewModel viewModel)
        {
            try
            {
                //Reload from database
                var friend = await _friend_service.ReadFriendAsync(viewModel.FriendInput.FriendId, false);

                viewModel.FriendInput = new FriendIM(friend.Item);
                viewModel.PageHeader = "Edit details of a friend";

                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error undoing changes for friend {FriendId}", viewModel.FriendInput.FriendId);
                return RedirectToAction("EditFriend", new { id = viewModel.FriendInput.FriendId });
            }
        }

        //POST: EditFriend/Save
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(EditFriendViewModel viewModel)
        {
            //Validate required fields
            if (string.IsNullOrWhiteSpace(viewModel.FriendInput.FirstName) ||
                string.IsNullOrWhiteSpace(viewModel.FriendInput.LastName) ||
                string.IsNullOrWhiteSpace(viewModel.FriendInput.Email))
            {
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "First name, last name, and email are required." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }

            try
            {
                //Create new friend if inserting
                if (viewModel.FriendInput.StatusIM == StatusIM.Inserted)
                {
                    var newFriend = await _friend_service.CreateFriendAsync(viewModel.FriendInput.CreateCUdto());
                    viewModel.FriendInput.FriendId = newFriend.Item.FriendId;
                }

                //Save Address
                await SaveAddress(viewModel.FriendInput);

                //Save Pets
                await SavePets(viewModel.FriendInput);

                //Save Quotes
                var friend = await SaveQuotes(viewModel.FriendInput);

                //Update the Friend
                friend = viewModel.FriendInput.UpdateModel(friend);
                await _friend_service.UpdateFriendAsync(new FriendCuDto(friend));

                //Redirect based on whether it was insert or update
                if (viewModel.FriendInput.StatusIM == StatusIM.Inserted)
                {
                    return RedirectToAction("ListOfFriends", "Members");
                }

                return RedirectToAction("FriendsDetails", "Members", new { id = viewModel.FriendInput.FriendId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving friend");
                viewModel.ValidationResult = new ModelValidationResult(true,
                    new[] { "An error occurred while saving. Please try again." }, null);
                PopulateDropdowns(viewModel);
                return View("EditFriend", viewModel);
            }
        }
        #endregion

        #region Private Helper Methods for Saving
        private async Task SaveAddress(FriendIM friendInput)
        {
            if (friendInput.Address == null)
            {
                friendInput.AddressId = null;
                return;
            }

            bool hasAddressData = !string.IsNullOrWhiteSpace(friendInput.Address.StreetAddress) ||
                                 !string.IsNullOrWhiteSpace(friendInput.Address.City) ||
                                 !string.IsNullOrWhiteSpace(friendInput.Address.Country) ||
                                 !string.IsNullOrWhiteSpace(friendInput.Address.ZipCode);

            if (!hasAddressData)
            {
                friendInput.AddressId = null;
                return;
            }

            try
            {
                var existingAddress = await FindExistingAddress(friendInput.Address);

                if (existingAddress != null)
                {
                    friendInput.AddressId = existingAddress.AddressId;
                    friendInput.Address.AddressId = existingAddress.AddressId;
                    return;
                }

                if (!friendInput.Address.AddressId.HasValue)
                {
                    var cuDto = friendInput.Address.CreateCUdto();
                    var newAddress = await _address_service.CreateAddressAsync(cuDto);

                    if (newAddress?.Item != null)
                    {
                        friendInput.AddressId = newAddress.Item.AddressId;
                        friendInput.Address.AddressId = newAddress.Item.AddressId;
                    }
                }
                else
                {
                    var addressResponse = await _address_service.ReadAddressAsync(friendInput.Address.AddressId.Value, false);

                    if (addressResponse?.Item != null)
                    {
                        var updatedAddress = friendInput.Address.UpdateModel(addressResponse.Item);
                        await _address_service.UpdateAddressAsync(new AddressCuDto(updatedAddress));
                        friendInput.AddressId = friendInput.Address.AddressId;
                    }
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("already exist"))
            {
                throw new ArgumentException(
                    $"Address duplicate: The address '{friendInput.Address.StreetAddress}, " +
                    $"{friendInput.Address.ZipCode} {friendInput.Address.City}, {friendInput.Address.Country}' " +
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

        private async Task<IFriend> SavePets(FriendIM friendInput)
        {
            //Delete removed pets
            var deletedPets = friendInput.Pets.FindAll(p => p.StatusIM == StatusIM.Deleted);
            foreach (var item in deletedPets)
            {
                await _pet_service.DeletePetAsync(item.PetId);
            }

            //Create new pets
            var newPets = friendInput.Pets.FindAll(p => p.StatusIM == StatusIM.Inserted);
            foreach (var item in newPets)
            {
                var cuDto = item.CreateCUdto();
                cuDto.FriendId = friendInput.FriendId;
                await _pet_service.CreatePetAsync(cuDto);
            }

            //Read updated friend
            var friend = await _friend_service.ReadFriendAsync(friendInput.FriendId, false);

            //Update modified pets
            var modifiedPets = friendInput.Pets.FindAll(p => p.StatusIM == StatusIM.Modified);
            foreach (var item in modifiedPets)
            {
                var model = friend.Item.Pets.First(p => p.PetId == item.PetId);
                model = item.UpdateModel(model);
                await _pet_service.UpdatePetAsync(new PetCuDto(model));
            }

            return friend.Item;
        }

        private async Task<IFriend> SaveQuotes(FriendIM friendInput)
        {
            //Delete removed quotes
            var deletedQuotes = friendInput.Quotes.FindAll(q => q.StatusIM == StatusIM.Deleted);
            foreach (var item in deletedQuotes)
            {
                await _quote_service.DeleteQuoteAsync(item.QuoteId);
            }

            //Create new quotes
            var newQuotes = friendInput.Quotes.FindAll(q => q.StatusIM == StatusIM.Inserted);
            foreach (var item in newQuotes)
            {
                try
                {
                    var cuDto = item.CreateCUdto();
                    cuDto.FriendsId = new List<Guid> { friendInput.FriendId };
                    await _quote_service.CreateQuoteAsync(cuDto);
                }
                catch (ArgumentException ex) when (ex.Message.Contains("already exist"))
                {
                    throw new ArgumentException(
                        $"Quote duplicate: The quote \"{item.QuoteText}\" by {item.Author} " +
                        $"already exists in the database. {ex.Message}", ex);
                }
            }

            //Read updated friend
            var friend = await _friend_service.ReadFriendAsync(friendInput.FriendId, false);

            //Update modified quotes
            var modifiedQuotes = friendInput.Quotes.FindAll(q => q.StatusIM == StatusIM.Modified);
            foreach (var item in modifiedQuotes)
            {
                try
                {
                    var model = friend.Item.Quotes.First(q => q.QuoteId == item.QuoteId);
                    model = item.UpdateModel(model);
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
    }
}