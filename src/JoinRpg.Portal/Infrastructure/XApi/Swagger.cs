using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace JoinRpg.Portal.Infrastructure
{

    internal class Swagger
    {
        internal static void ConfigureSwagger(SwaggerGenOptions c)
        {
            c.SwaggerDoc("v1", new OpenApiInfo()
            {
                Title = "My API",
                Version = "v1"
            });
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"JoinRpg.Web.XGameApi.Contract.xml"));

            c.DocumentFilter<SwaggerXGameApiFilter>();
        }

        internal static void Configure(SwaggerOptions options) { }

        internal static void ConfigureUI(SwaggerUIOptions c)
        {
            c.SwaggerEndpoint("v1/swagger.json", "My API V1");
            c.ConfigObject.DeepLinking = true;
        }

        internal static Task RedirectToSwagger(HttpContext ctx)
        {
            ctx.Response.Redirect("swagger/");
            return Task.CompletedTask;
        }

        private class SwaggerXGameApiFilter : IDocumentFilter
        {
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                foreach (var item in swaggerDoc.Paths.ToList())
                {
                    var key = item.Key.ToLower();
                    if (!key.IsApiPath())
                    {
                        _ = swaggerDoc.Paths.Remove(item.Key);
                    }
                }

            }


        }
    }
}
