using Microsoft.AspNetCore.Mvc;
using SmartAudit.Core.Domain;

namespace SmartAudit.Api.Extentions
{
    public static class ResponseExtension
    {
        public static IActionResult ToActionResult<T>(this Response<T> response)
        {
            return response.Status switch
            {
                ResponseStatus.Success => new OkObjectResult(response.Data),
                ResponseStatus.NotFound => new NotFoundObjectResult(new { response.Message }),
                ResponseStatus.ValidationFailed => new BadRequestObjectResult(new { response.Message }),
                ResponseStatus.Forbidden => new ObjectResult(new { response.Message })
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                },
                ResponseStatus.ServiceUnavailable => new ObjectResult(new { response.Message })
                {
                    StatusCode = StatusCodes.Status503ServiceUnavailable,
                },
                ResponseStatus.Error => new ObjectResult(new { response.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                },
                _ => new StatusCodeResult(StatusCodes.Status500InternalServerError),
            };
        }
    }
}
