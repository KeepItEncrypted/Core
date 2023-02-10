using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTransferBackend.Data;
using SecureTransferBackend.Services.Auth.Models;
using SecureTransferBackend.Services.Transfer.Dtos;
using SecureTransferBackend.Services.Transfer.Models;

namespace SecureTransferBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PublicKeyPairController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        // GET: api/PublicKeyPair
        public PublicKeyPairController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<List<PublicKeyPairDto>> GetPublicKeys()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            return await _dbContext.PublicKeyPairs.Where(pkp => pkp.ApplicationUser == user).Select(pkp =>
                new PublicKeyPairDto()
                {
                    Id = pkp.Id,
                    PublicEncryptionKey = pkp.PublicEncryptionKey,
                    PublicVerifierKey = pkp.PublicVerifierKey,
                    ApplicationUserId = pkp.ApplicationUser.Id
                }).ToListAsync();
        }

        // GET: api/PublicKeyPair/5
        [HttpGet("for-user/{userId}/")]
        public async Task<List<PublicKeyPairDto>> GetPublicKeysForUser(Guid userId)
        {
            return await _dbContext.PublicKeyPairs
                .Where(pkp => pkp.ApplicationUserId == userId && pkp.IsArchived == false).Select(pkp =>
                    new PublicKeyPairDto()
                    {
                        Id = pkp.Id,
                        PublicEncryptionKey = pkp.PublicEncryptionKey,
                        PublicVerifierKey = pkp.PublicVerifierKey,
                        ApplicationUserId = pkp.ApplicationUser.Id
                    }).ToListAsync();
        }
        
        [HttpGet("{publicKeyId}")]
        public async Task<ActionResult<PublicKeyPairDto?>> GetPublicKey(Guid publicKeyId)
        {
            var key = await _dbContext.PublicKeyPairs
                .Where(pkp => pkp.Id == publicKeyId && pkp.IsArchived == false).Select(pkp =>
                    new PublicKeyPairDto()
                    {
                        Id = pkp.Id,
                        PublicEncryptionKey = pkp.PublicEncryptionKey,
                        PublicVerifierKey = pkp.PublicVerifierKey,
                        ApplicationUserId = pkp.ApplicationUser.Id
                    }).FirstOrDefaultAsync();
            if (key == null)
            {
                return NotFound("Key does not exist");
            }
            return key;
        }

        // POST: api/PublicKeyPair
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CreatePublicKeyPair body)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            await _dbContext.PublicKeyPairs.AddAsync(new PublicKeyPair()
            {
                ApplicationUser = user!,
                PublicEncryptionKey = body.PublicEncryptionKey,
                PublicVerifierKey = body.PublicVerifierKey
            });
            await _dbContext.SaveChangesAsync();
            return Ok();
        }
    }
}