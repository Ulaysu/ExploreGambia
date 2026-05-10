using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ExploreGambia.API.CustomActionFilters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        private readonly ILogger<ValidateModelAttribute> _logger;

        public ValidateModelAttribute(ILogger<ValidateModelAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errorId = Guid.NewGuid();
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors
                            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                                ? "The input was not valid."
                                : error.ErrorMessage)
                            .ToArray());

                var actionName = context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor
                    ? controllerActionDescriptor.ActionName
                    : "Unknown";

                _logger.LogWarning(
                    "Model validation failed for action '{ActionName}' in controller '{ControllerName}'. ErrorId: {ErrorId}. Errors: {@ValidationErrors}",
                    actionName,
                    context.Controller.GetType().Name,
                    errorId,
                    errors);

                context.Result = new BadRequestObjectResult(new
                {
                    ErrorId = errorId,
                    Code = "validation_failed",
                    Message = "One or more validation errors occurred.",
                    Details = errors
                });
            }
        }
    }
}
