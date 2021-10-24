using DSharpPlus;
using HGV.Reaver.Handlers;
using HGV.Reaver.Models;
using HGV.Reaver.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;


namespace HGV.Reaver.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService accountService;
        private readonly IChangeNicknameHandler handler;

        public AccountController(IAccountService accountService, IChangeNicknameHandler handler)
        {
            this.accountService = accountService;
            this.handler = handler;
        }

        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Link(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var entity = new UserEntity();
            entity.PartitionKey = id;

            foreach (var claim in this.User.Claims)
            {
                switch (claim.Type)
                {
                    case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier":
                        entity.RowKey = claim.Value;
                        entity.DiscordId = claim.Value;
                        break;
                    case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress":
                        entity.Email = claim.Value;
                        break;
                    default:
                        break;
                }
            }

            var key = Guid.NewGuid().ToString();
            var json = JsonConvert.SerializeObject(entity);
            this.TempData[key] = json;

            return RedirectToAction("Verify", new { id = key });
        }

        [Authorize(AuthenticationSchemes = "Steam")]
        public async Task<IActionResult> Verify(string id)
        {
            var json = this.TempData[id] as string;
            var entity = JsonConvert.DeserializeObject<UserEntity>(json);

            foreach (var claim in this.User.Claims)
            {
                switch (claim.Type)
                {
                    case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier":
                        entity.SteamId = claim.Value.Replace("https://steamcommunity.com/openid/id/", "");
                        break;
                    default:
                        break;
                }
            }

            await this.accountService.AddLink(entity);

            await this.handler.ChangeNickname(entity);

            return RedirectToAction("Linked");
        }

        public IActionResult Linked()
        {
            return View();
        }
    }
}
