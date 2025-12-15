using Microsoft.AspNetCore.Mvc;
using RefactAI.Common;
using System.Text.Json;
using System.Threading.Tasks;

namespace RefactAI.Api.Controllers
{
    [ApiController]
    [Route("config")]
    public class ConfigController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly string _tokenFile;
        private readonly string _repoFile;

        public ConfigController(IWebHostEnvironment env)
        {
            _env = env;

            // Files will be stored inside API project's root folder
            _tokenFile = Path.Combine(env.ContentRootPath, "config", "Token.json");
            _repoFile = Path.Combine(_env.ContentRootPath, "config", "Repo.json");
        }

        // ---------------------------------------------------------
        // 1. SET TOKEN
        // ---------------------------------------------------------
        [HttpPost("set-token")]
        public async Task<IActionResult> SetToken([FromBody] TokenUpdateRequest req)
        {
            GlobalTokenStore.Save(req.Token);
            await ReloadWebhookAsync();
            return Ok(new { message = "GitHub token saved to local file." });
        }
        // --------ß-------------------------------------------------
        // 2. GET TOKEN
        // ---------------------------------------------------------
        public static async Task ReloadWebhookAsync()
        {
            using var client = new HttpClient();

            string url = "http://localhost:5121/webhooks/run?repo=https://github.com/SurajGangwani0711/Chec-I";

            await client.GetAsync(url);
        }

        [HttpGet("get-token")]
        public async Task<IActionResult> GetToken()
        {
            if (!System.IO.File.Exists(_tokenFile))
                return NotFound(new { error = "Token file missing." });

            var json = await System.IO.File.ReadAllTextAsync(_tokenFile);
            return Content(json, "application/json");
        }

        // ---------------------------------------------------------
        // 3. SET REPO URL
        // ---------------------------------------------------------
        [HttpPost("set-repo")]
        public async Task<IActionResult> SetRepo([FromBody] RepoUpdateRequest req)
        {
             GlobalRepoStore.Save(req.RepoUrl);
            return Ok(new { message = "Repo URL saved to local file." });
        }

        // ---------------------------------------------------------
        // 4. GET REPO URL
        // ---------------------------------------------------------
        [HttpGet("get-repo")]
        public async Task<IActionResult> GetRepo()
        {
            if (!System.IO.File.Exists(_repoFile))
                return NotFound(new { error = "Repo file missing." });

            var json = await System.IO.File.ReadAllTextAsync(_repoFile);
            return Content(json, "application/json");
        }
    }

    // REQUEST MODELS
    public class TokenUpdateRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class RepoUpdateRequest
    {
        public string RepoUrl { get; set; } = string.Empty;
    }
}
