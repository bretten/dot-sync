using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace com.brettnamba.DotSync.FileSystem.WebApp.Startup;

public static class AuthenticationExtensions
{
    public const string AuthenticationScheme = "oidc";
    public const string VerifierPolicy = "Verifier";
    public const string PusherPolicy = "Pusher";

    public static void AddOidc(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(AuthenticationScheme)
            .AddOpenIdConnect(AuthenticationScheme, oidcOptions =>
            {
                oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);
                oidcOptions.Scope.Add(OpenIdConnectScope.Email);

                oidcOptions.Authority = config["OAuth2:Authority"];
                oidcOptions.ClientId = config["OAuth2:ClientId"];

                oidcOptions.ResponseType = OpenIdConnectResponseType.Code;

                oidcOptions.SaveTokens = true;

                oidcOptions.Events = new OpenIdConnectEvents()
                {
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        // These parameters are required by AWS Cognito when logging out
                        context.ProtocolMessage.Parameters.Add("client_id", oidcOptions.ClientId);
                        context.ProtocolMessage.Parameters.Add("logout_uri",
                            context.ProtocolMessage.Parameters["post_logout_redirect_uri"]);
                        return Task.CompletedTask;
                    },
                    OnSignedOutCallbackRedirect = context =>
                    {
                        // After logging out, redirect to home page
                        context.Response.Redirect("/");
                        return Task.CompletedTask;
                    },
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

        services.AddAuthorizationBuilder()
            .AddPolicy(VerifierPolicy,
                policy => policy.RequireClaim(config["OAuth2:UserGroupsClaim"]!, config["OAuth2:VerifyGroup"]!))
            .AddPolicy(PusherPolicy,
                policy => policy.RequireClaim(config["OAuth2:UserGroupsClaim"]!, config["OAuth2:PushGroup"]!));

        services.AddCascadingAuthenticationState();
    }

    public static void MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/login",
            ([FromQuery] string? returnUrl) => TypedResults.Challenge(BuildAuthProperties(returnUrl))).AllowAnonymous();

        endpoints.MapPost("/logout",
            ([FromForm] string? returnUrl) => TypedResults.SignOut(BuildAuthProperties(returnUrl),
                [CookieAuthenticationDefaults.AuthenticationScheme, AuthenticationScheme]));
    }

    private static AuthenticationProperties BuildAuthProperties(string? returnUrl)
    {
        return new AuthenticationProperties()
        {
            RedirectUri = SanitizeUrl(returnUrl)
        };
    }

    /// <summary>
    /// Sanitizes the redirect URL by preventing open redirects to external sites. Eg, when the IdP redirects after
    /// login/logout with the auth code, this will strip the host/domain and leave only a relative URL and prevent interception
    /// </summary>
    private static string SanitizeUrl(string? returnUrl)
    {
        const string pathBase = "/";

        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return returnUrl;
    }
}