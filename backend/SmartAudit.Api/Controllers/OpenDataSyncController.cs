using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Services.OpenData;

namespace SmartAudit.Api.Controllers
{
    // Živé napojení na otevřená data (Data KHK, ČÚZK, Open-Meteo, Registr smluv) — stav a ruční synchronizace.
    // Bez přihlášení: tlačítko na stránce Zdroje dat smí zkusit i porotce; souběžné a opakované kontroly
    // (do 15 s) vrátí poslední stav, takže zdroje nejde zahltit.
    [Route("api/opendata")]
    [ApiController]
    [AllowAnonymous]
    [Tags("Synchronizace otevřených dat")]
    public class OpenDataSyncController : ControllerBase
    {
        private readonly IOpenDataSyncService _syncService;

        public OpenDataSyncController(IOpenDataSyncService syncService)
        {
            _syncService = syncService;
        }

        /// <summary>Stav napojení na zdroje — poslední synchronizace, počty záznamů, odkazy do katalogu a na živé API</summary>
        [HttpGet]
        [Route("status")]
        [ProducesResponseType<OpenDataSyncDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Status(CancellationToken ct)
        {
            return Ok(await _syncService.StatusAsync(ct));
        }

        /// <summary>Živá synchronizace: každý zdroj se zkontroluje s timeoutem 5 s; změněné sady Data KHK se na pozadí stáhnou znovu</summary>
        [HttpPost]
        [Route("sync")]
        [ProducesResponseType<OpenDataSyncDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Sync(CancellationToken ct)
        {
            return Ok(await _syncService.SyncAsync(ct));
        }
    }
}
