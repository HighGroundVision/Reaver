using DSharpPlus;
using HGV.Reaver.Data;
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
        private readonly IProfileService profileService;

        public AccountController(IAccountService accountService, IProfileService profileService)
        {
            this.accountService = accountService;
            this.profileService = profileService;
        }

        [Authorize(AuthenticationSchemes = "Discord")]
        public IActionResult Link(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));

            var entity = new UserLinkEntity();
            entity.GuidId = ulong.Parse(id);

            foreach (var claim in this.User.Claims)
            {
                switch (claim.Type)
                {
                    case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier":
                        entity.UserId = ulong.Parse(claim.Value);            
                        break;
                    case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress":
                        entity.Email = claim.Value;
                        break;
                    // http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name
                    // urn:discord:avatar:url
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
            if (json is null)
                throw new NullReferenceException("AccountController::Verify::Json");

            var entity = JsonConvert.DeserializeObject<UserLinkEntity>(json);
            if (entity is null)
                throw new NullReferenceException("AccountController::Verify::DeserializeObject::UserLinkEntity");

            foreach (var claim in this.User.Claims)
            {
                switch (claim.Type)
                {
                    case "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier":
                        var openId = claim.Value.Replace("https://steamcommunity.com/openid/id/", "");
                        var steamId = ulong.Parse(openId);
                        entity.SteamId = steamId;
                        break;
                    // http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name RGBKnights
                    default:
                        break;
                }
            }

            //var dota = await this.profileService.GetDotaProfile(entity.SteamId);
            //var steam = await this.profileService.GetSteamProfile(entity.SteamId);
            //entity.DotaId = dota.AccountId.GetValueOrDefault();

            await this.accountService.Add(entity);

            return RedirectToAction("Linked");
        }

        public IActionResult Linked()
        {
            return View();
        }
    }
}

