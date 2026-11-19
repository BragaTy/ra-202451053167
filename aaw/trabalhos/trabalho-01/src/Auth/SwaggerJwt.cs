using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace SuporteApi.Auth;

// Acrescenta o esquema "Bearer" ao documento OpenAPI. É isso que faz aparecer
// o botão Authorize no Swagger: cola-se o token e ele vai em todas as chamadas.
public static class SwaggerJwt
{
    public static void AdicionarBearer(this OpenApiOptions options) =>
        options.AddDocumentTransformer((document, context, ct) =>
        {
            document.Info.Title = "HelpDesk API";
            document.Info.Description = "Clientes e chamados de suporte técnico, com SLA calculado por prioridade e plano.";

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Faça login em /api/v1/auth/login e cole aqui só o token."
            };

            document.Security ??= [];
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
            return Task.CompletedTask;
        });
}
