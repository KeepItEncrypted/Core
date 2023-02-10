using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SecureTransferBackend.Services.Auth.Models;
using SecureTransferBackend.Services.Transfers;
using SecureTransferBackend.Services.Transfers.Dtos;


namespace SecureTransferBackend.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TransfersController : ControllerBase
    {
        private readonly ITransferService _transferService;
        private readonly UserManager<ApplicationUser> _userManager;

        public TransfersController(ITransferService transferService, UserManager<ApplicationUser> userManager)
        {
            _transferService = transferService;
            _userManager = userManager;
        }

        // GET api/Transfers
        [HttpPost]
        public async Task CreateFile([FromForm] TransferBundle transferBundle)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _transferService.SendBundle(transferBundle, user!.Id);
        }
        
        [HttpPost("CreateBundle")]
        public async Task<ActionResult<CreatedBundle>> CreateBundle([FromBody] CreateBundle createBundle)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var createdBundlId = await _transferService.CreateBundle(createBundle, user!.Id);
            return Ok(new CreatedBundle(createdBundlId));
        }
        
        [HttpPost("Bundle/{bundleId}/AddFile")]
        public async Task<ActionResult> AddFileToBundle(Guid bundleId, [FromForm] EncryptedFileDto encryptedFile)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _transferService.AddFileToBundle(bundleId, user!.Id, encryptedFile);
            return Ok();
        }

        [HttpDelete("{bundleId}")]
        public async Task<ActionResult> DeleteBundle(Guid bundleId)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            try
            {
                await _transferService.CancelSentBundle(bundleId, user!.Id);
            }
            catch (NotOwnerOfBundleException)
            {
                return Forbid();
            }

            return NoContent();
        }
    }
}
