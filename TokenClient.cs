using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace Affinidi_Login_Demo_App
{
    /*
     * A utility class for working with tokens
     */
    public class TokenClient
    {
        public static string ACCESS_TOKEN = "access_token";
        public static string REFRESH_TOKEN = "refresh_token";
        public static string ID_TOKEN = "id_token";

        private readonly IConfiguration configuration;

        public TokenClient(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /*
         * Return the current access token for display
         */
        public async Task<string> GetAccessToken(HttpContext context)
        {
            return await context.GetTokenAsync(ACCESS_TOKEN) ?? string.Empty;
        }

        /*
         * Return the current refresh token for display
         */
        public async Task<string> GetRefreshToken(HttpContext context)
        {
            return await context.GetTokenAsync(REFRESH_TOKEN) ?? string.Empty;
        }

        /*
         * Do the work of getting new tokens and updating cookies
         */
        public async Task RefreshAccessToken(HttpContext context)
        {
            var tokens = await this.RefreshTokens(context);
            await RewriteCookies(tokens, context);
        }

        /*
         * Send the refresh token grant message
         */
        private async Task<JObject> RefreshTokens(HttpContext context)
        {
            var tokenEndpoint = this.configuration.GetValue<string>("OpenIdConnect:TokenEndpoint") ?? string.Empty;

            var clientId = this.configuration.GetValue<string>("OpenIdConnect:ClientId") ?? string.Empty;
            var clientSecret = this.configuration.GetValue<string>("OpenIdConnect:ClientSecret") ?? string.Empty;
            var refreshToken = await context.GetTokenAsync(REFRESH_TOKEN) ?? string.Empty;

            var requestData = new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>(REFRESH_TOKEN, refreshToken),
            };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("accept", "application/json");

                var response = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(requestData));
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var node = JObject.Parse(json);
                if (node == null)
                {
                    throw new InvalidOperationException("Failed to parse JSON response.");
                }
                return node;
            }
        }

        /*
         * Write updated cookies with new tokens
         */
        private async Task RewriteCookies(JObject tokens, HttpContext context)
        {
            var accessToken = tokens[ACCESS_TOKEN]?.ToString() ?? string.Empty;
            var refreshToken = tokens[REFRESH_TOKEN]?.ToString();
            var idToken = tokens[ID_TOKEN]?.ToString();

            // An access token is always returned
            var newTokens = new List<AuthenticationToken>
            {
                new AuthenticationToken { Name = ACCESS_TOKEN, Value = accessToken }
            };

            // A refresh token will be returned when rotating refresh tokens are used, which is the default in the Curity Identity Server
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                refreshToken = await context.GetTokenAsync(REFRESH_TOKEN) ?? string.Empty;
            }
            newTokens.Add(new AuthenticationToken { Name = REFRESH_TOKEN, Value = refreshToken });

            // A new ID token is optional
            if (string.IsNullOrWhiteSpace(idToken))
            {
                idToken = await context.GetTokenAsync(ID_TOKEN) ?? string.Empty;
            }
            newTokens.Add(new AuthenticationToken { Name = ID_TOKEN, Value = idToken });

            // Rewrite cookies
            var properties = context.Features.Get<IAuthenticateResultFeature>()?.AuthenticateResult?.Properties;
            if (properties != null)
            {
                properties.StoreTokens(newTokens);
                await context.SignInAsync(context.User, properties);
            }
        }
    }
}
