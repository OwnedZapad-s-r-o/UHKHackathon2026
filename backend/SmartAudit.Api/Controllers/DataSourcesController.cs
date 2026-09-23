using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAudit.Api.Extentions;
using SmartAudit.Api.Features.DataSources.GetDataSources;
using SmartAudit.Api.Features.DataSources.ImportDataSource;
using SmartAudit.Core.Common;

namespace SmartAudit.Api.Controllers
{
    [Route("api/data-sources")]
    [ApiController]
    public class DataSourcesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DataSourcesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetDataSourcesCommand(), ct);

            return result.ToActionResult();
        }

        [HttpPost]
        [Route("{key}/import")]
        [Authorize(Roles = AppRoles.Region)]
        public async Task<IActionResult> Import(string key, CancellationToken ct)
        {
            var command = new ImportDataSourceCommand(key);

            var result = await _mediator.Send(command, ct);

            return result.ToActionResult();
        }
    }
}
