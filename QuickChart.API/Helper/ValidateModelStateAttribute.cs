using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace QuickChart.API.Helper
{

    public class ValidateModelStateAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );

                var response = new 
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors.ToArray()
                    //Errors = errors.SelectMany(e => e.Value).ToList()
                };

                context.Result = new BadRequestObjectResult(response);
            }
        }
    }
}

