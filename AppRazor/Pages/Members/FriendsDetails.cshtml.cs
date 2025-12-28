using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Models.Interfaces;
using Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DbContext;
using Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace AppRazor.Pages.Members
{
    public class FriendsDetailsModel : PageModel
    {
        readonly IFriendsService _service = null;
        readonly ILogger<FriendsDetailsModel> _logger = null;
        
        public IFriend Friend { get; set; }

        public FriendsDetailsModel(IFriendsService service, ILogger<FriendsDetailsModel> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            Guid _friendId = Guid.Parse(Request.Query["id"]);
            Friend = (await _service.ReadFriendAsync(_friendId, false)).Item;
            return Page();
        }
    }
}
