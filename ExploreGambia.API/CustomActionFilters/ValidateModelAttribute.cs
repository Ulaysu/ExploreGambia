using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Serilog; 
using System.Linq;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog.Core;

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
            if (context.ModelState.IsValid == false)
            {
                // Log the model state errors
                if (!context.ModelState.IsValid) 
                { 
                    var errors = context.ModelState
                        .Where(e => e.Value.Errors.Count > 0)
                        .ToDictionary(k => k.Key, 
                        v => v.Value.Errors.Select(e => e.ErrorMessage).ToArray());

                    if (context.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    {
                        _logger.LogWarning("Model validation failed for action '{ActionName}' in controller '{ControllerName}'. Errors: {@ValidationErrors}",
                                            controllerActionDescriptor.ActionName,
                                            context.Controller.GetType().Name,
                                            errors);
                    }
                    else
                    {
                        _logger.LogWarning("Model validation failed for an unknown action in controller '{ControllerName}'. Errors: {@ValidationErrors}",
                                            context.Controller.GetType().Name,
                                            errors);
                        
                    }

                }
                context.Result = new BadRequestResult();
            }
        }
    }
}
