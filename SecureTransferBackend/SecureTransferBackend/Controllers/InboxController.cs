using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureTransferBackend.Data;
using SecureTransferBackend.Services.Auth.Models;
using SecureTransferBackend.Services.Transfers;
using SecureTransferBackend.Services.Transfers.Dtos;

namespace SecureTransferBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InboxController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly ITransferService _transferService;

        public InboxController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, ITransferService transferService)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _transferService = transferService;
        }

        [HttpGet]
        public async Task<List<InboxItem>> GetInbox()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _transferService.GetInbox(user!.Id);
        }
        
        [HttpGet("{recipientId}")]
        public async Task<ActionResult<InboxItemDetail>> GetInboxItem(Guid recipientId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var detail = await _transferService.GetInboxItem(recipientId, user!.Id);
            if (detail == null)
            {
                return NotFound();
            }
            return Ok(detail);
        }
        
    }
}
