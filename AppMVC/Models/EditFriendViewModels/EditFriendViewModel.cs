using Microsoft.AspNetCore.Mvc.Rendering;
namespace AppMVC.Models.EditFriendViewModels
{
    public class EditFriendViewModel
    {
        public FriendIM FriendInput { get; set; } = new FriendIM();
        public string PageHeader { get; set; } = "Edit Friend";
        public List<SelectListItem> AnimalKindItems { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AnimalMoodItems { get; set; } = new List<SelectListItem>();
        public ModelValidationResult ValidationResult { get; set; } = new ModelValidationResult(false, null, null);
    }
}