using AffinidiTdk.AuthProvider;
using Newtonsoft.Json;

namespace Affinidi_Login_Demo_App.Util
{
    public class AuthProviderClient
    {
        private static readonly Lazy<AuthProviderClient> _instance = new(() => new AuthProviderClient());
        public static AuthProviderClient Instance => _instance.Value;

        private readonly AuthProvider _authProvider;

        private AuthProviderClient()
        {
            // Optionally load .env if needed
            // Env.TraversePath().Load();

            var authProviderParams = new AuthProviderParams
            {
                ProjectId = Environment.GetEnvironmentVariable("PROJECT_ID") ?? string.Empty,
                TokenId = Environment.GetEnvironmentVariable("TOKEN_ID") ?? string.Empty,
                KeyId = Environment.GetEnvironmentVariable("KEY_ID") ?? string.Empty,
                PrivateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY") ?? string.Empty,
                Passphrase = Environment.GetEnvironmentVariable("PASSPHRASE") ?? string.Empty
            };

            // Log the parameters using Newtonsoft.Json for debugging (do not log secrets in production)
            Console.WriteLine($"[AuthProvider] AuthProviderParams: {JsonConvert.SerializeObject(authProviderParams)}");

            _authProvider = new AuthProvider(authProviderParams);
        }

        public async Task<string> GetProjectScopedToken()
        {
            var token = await _authProvider.FetchProjectScopedTokenAsync();
            // Log the fetched token (do not log sensitive information in production)
            Console.WriteLine($"[AuthProvider] Fetched Project Scoped Token: {token}");
            return token;
        }
    }
}