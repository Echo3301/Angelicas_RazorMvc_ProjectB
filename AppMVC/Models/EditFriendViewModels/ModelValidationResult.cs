namespace AppMVC.Models.EditFriendViewModels
{
    public class ModelValidationResult
    {
        public bool HasErrors { get; set; }
        public IEnumerable<string> ErrorMsgs { get; set; }
        public IEnumerable<string> Keys { get; set; }

        public ModelValidationResult(bool hasErrors, IEnumerable<string> errorMsgs, IEnumerable<string> keys)
        {
            HasErrors = hasErrors;
            ErrorMsgs = errorMsgs ?? new List<string>();
            Keys = keys ?? new List<string>();
        }
    }
}