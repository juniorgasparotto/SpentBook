using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Internal;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace SpentBook.Web.Binders
{
    public static class EnumBinderExtensions
    { 
        public static MvcOptions AddFlagsEnumModelBinderProvider(this MvcOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            InsertFlagsEnumModelBinderProvider(options.ModelBinderProviders);
            return options;
        }

        private static void InsertFlagsEnumModelBinderProvider(IList<IModelBinderProvider> modelBinders)
        {
            // Argument Check
            if (modelBinders == null)
                throw new ArgumentNullException(nameof(modelBinders));

            var providerToInsert = new FlagsEnumModelBinderProvider();

            // Find the location of SimpleTypeModelBinder, the FlagsEnumModelBinder must be inserted before it.
            var index = FirstIndexOfOrDefault(modelBinders, i => i is SimpleTypeModelBinderProvider);

            if (index != -1)
                modelBinders.Insert(index, providerToInsert);
            else
                modelBinders.Add(providerToInsert);
        }

        private static int FirstIndexOfOrDefault<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            var result = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    return result;

                result++;
            }

            return -1;
        }
    }

    public class FlagsEnumModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return context.Metadata.IsFlagsEnum ? new FlagsEnumModelBinder() : null;
        }
    }

    public class FlagsEnumModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Only accept enum values
            if (!bindingContext.ModelMetadata.IsFlagsEnum)
                return TaskCache.CompletedTask;

            var provideValue = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            // Do nothing if there is no actual values
            if (provideValue == ValueProviderResult.None)
                return TaskCache.CompletedTask;

            // Get the real enum type
            var enumType = bindingContext.ModelType;
            enumType = Nullable.GetUnderlyingType(enumType) ?? enumType;

            // Each value self may contains a series of actual values, split it with comma
            var strs = provideValue.Values.SelectMany(s => s.Split(','));

            // Convert all items into enum items.
            var actualValues = strs.Select(valueString => Enum.Parse(enumType, valueString));

            // Merge to final result
            var result = actualValues.Aggregate(0, (current, value) => current | (int)value);

            // Convert to Enum object
            var realResult = Enum.ToObject(enumType, result);

            // Result
            bindingContext.Result = ModelBindingResult.Success(realResult);

            return TaskCache.CompletedTask;
        }
    }
}