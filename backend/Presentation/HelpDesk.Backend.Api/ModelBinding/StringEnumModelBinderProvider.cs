using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HelpDesk.Backend.Api.ModelBinding;

internal sealed class StringEnumModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var enumType = Nullable.GetUnderlyingType(context.Metadata.ModelType) ??
            context.Metadata.ModelType;
        return enumType.IsEnum
            ? new StringEnumModelBinder(enumType)
            : null;
    }

    private sealed class StringEnumModelBinder(Type enumType) : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(
                bindingContext.ModelName);
            if (result == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(
                bindingContext.ModelName,
                result);
            var value = result.FirstValue;
            if (string.IsNullOrWhiteSpace(value))
            {
                return Task.CompletedTask;
            }

            if (value.All(char.IsDigit) ||
                !Enum.TryParse(enumType, value, true, out var parsed) ||
                !Enum.IsDefined(enumType, parsed!))
            {
                bindingContext.ModelState.TryAddModelError(
                    bindingContext.ModelName,
                    $"El valor '{value}' no es válido para {enumType.Name}. Use el nombre textual.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(parsed);
            return Task.CompletedTask;
        }
    }
}
