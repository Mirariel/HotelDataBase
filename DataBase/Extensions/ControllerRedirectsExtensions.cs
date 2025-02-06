using Microsoft.AspNetCore.Mvc;

namespace DataBase.Extensions
{
    public static class ControllerRedirectsExtensions
    {
        public static IActionResult RedirectWithTempError(this Controller controller, string errorMessage, string redirectTo, object? routeValues = null)
        {
            controller.TempData["Error"] = errorMessage;
            return controller.RedirectToAction(redirectTo, routeValues);
        }
        public static IActionResult ViewWithModelError(this Controller controller, string errorMessage, string viewName, object? routeValues = null)
        {
            controller.ModelState.AddModelError("", errorMessage);
            return controller.View(viewName, routeValues);
        }
        public static IActionResult ViewWithModelError(this Controller controller, string errorMessage, object model, object? routeValues = null)
        {
            controller.ModelState.AddModelError("", errorMessage);
            return controller.View(model);
        }
    }
}
