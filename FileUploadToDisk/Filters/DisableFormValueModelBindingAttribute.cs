using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace FileUploadToDisk.Filters
{
    /// <summary>
    /// This filter is needed to disable model binding when streaming files to
    /// a controller action and you want to handle validation manually.
    /// 
    /// Use it as an attribute like:
    /// [HttpPost]
    /// [DisableFormValueModelBinding]
    /// public async Task<IActionResult> ControllerAction() { ... }
    /// 
    /// See https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-6.0#upload-large-files-with-streaming
    /// and https://github.com/dotnet/AspNetCore.Docs/blob/6571c5cacb89c346832906f777759ced947344e4/aspnetcore/mvc/models/file-uploads/samples/3.x/SampleApp/Filters/ModelBinding.cs
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var factories = context.ValueProviderFactories;
            factories.RemoveType<FormValueProviderFactory>();
            factories.RemoveType<FormFileValueProviderFactory>();
            factories.RemoveType<JQueryFormValueProviderFactory>();
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
    }
}
