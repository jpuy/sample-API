using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SocialDistancing.API.Common.Middlewares;
using System;
using System.Threading.Tasks;

namespace SocialDistancing.API.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class APIKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "Authorization";
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            
            if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var validationKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            //var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            //var apiKey = configuration.GetValue<string>("ApiKey");

            var validation = new ApiKeyValidation();
            if (!validation.ValidateApiKey(validationKey))
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            
            await next();
        }

        

    }
}
