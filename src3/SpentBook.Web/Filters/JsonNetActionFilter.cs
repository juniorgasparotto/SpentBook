using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System;

namespace SpentBook.Web.Filters
{
    public class JsonNetActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.Result.GetType() == typeof(JsonResult))
            {
                // Get the standard result object with unserialized data
                JsonResult result = filterContext.Result as JsonResult;

                // Replace it with our new result object and transfer settings
                //filterContext.Result = new JsonNetResult
                //{
                //    ContentEncoding = result.ContentEncoding,
                //    ContentType = result.ContentType,
                //    Data = result.Data,
                //    JsonRequestBehavior = result.JsonRequestBehavior
                //};

                filterContext.Result = new JsonNetResult(result.Value)
                {
                    ContentType = result.ContentType,
                    Value = result.Value,
                    SerializerSettings = result.SerializerSettings,
                    StatusCode = result.StatusCode
                };

                // Later on when ExecuteResult will be called it will be the
                // function in JsonNetResult instead of in JsonResult
            }
            base.OnActionExecuted(filterContext);
        }
    }

    public class JsonNetResult : JsonResult
    {
        public JsonNetResult(object value) : base(value)
        {
            Settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Error
            };
        }

        public JsonSerializerSettings Settings { get; private set; }

        public override void ExecuteResult(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;

            //if (this.ContentEncoding != null)
            //    response.ContentEncoding = this.ContentEncoding;

            //if (this.Data == null)
            //    return;

            if (this.Value == null)
                return;

            response.ContentType = string.IsNullOrEmpty(this.ContentType) ? "application/json" : this.ContentType;

            this.Settings.Converters.Add(new CustomConverter());

            var scriptSerializer = JsonSerializer.Create(this.Settings);

            // Serialize the data to the Output stream of the response
            //scriptSerializer.Serialize(response.Output, this.Data);
        }
    }

    public class CustomConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //writer.WriteValue((double?)(DotNet.Highcharts.Helpers.Number?)value);
            writer.WriteRawValue(DotNet.Highcharts.Extensions.FormatWith("{0}", DotNet.Highcharts.JsonSerializer.Serialize(value)));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            //return false;
            return objectType.FullName.StartsWith("DotNet.");
            return typeof(DotNet.Highcharts.Helpers.Number) == objectType
                || typeof(DotNet.Highcharts.Helpers.Number?) == objectType;
        }
    }
}