using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KvizCommando.Server.Controllers.NonAuthenticatedControllers;

[ApiController]
[Route("api/lang")]
[AllowAnonymous]
public class LanguageController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    public LanguageController(IWebHostEnvironment env) => _env = env;

    [HttpGet("manifest")]
    public IActionResult GetManifest([FromQuery] string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return BadRequest("Culture is required.");

        var path = Path.Combine(
            _env.ContentRootPath,               // <— projekt futtatási gyökér
            "Resources", "Client", "Localization", culture, "manifest.json");

        if (!System.IO.File.Exists(path))
            return NotFound($"Manifest not found for culture '{culture}'.");

        // opcionális: ne cache-elje agresszíven a böngésző
        Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
        return PhysicalFile(path, "application/json");
    }
}
