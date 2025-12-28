using System.ComponentModel.DataAnnotations;
using Models.DTO;
using Models.Interfaces;

namespace AppMVC.Models.EditFriendViewModels
{
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

        public IQuote UpdateModel(IQuote model)
        {
            model.QuoteId = this.QuoteId;
            model.QuoteText = this.QuoteText;
            model.Author = this.Author;
            return model;
        }

        public QuoteCuDto CreateCUdto() => new QuoteCuDto()
        {
            QuoteId = null,
            Quote = this.QuoteText,
            Author = this.Author
        };
    }
}