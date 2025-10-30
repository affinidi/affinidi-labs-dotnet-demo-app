using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Affinidi_Login_Demo_App.Util
{
    public class VerifyCredentialsInput
    {
        [JsonPropertyName("verifiableCredentials")]
        public List<object> VerifiableCredentials { get; set; } = new();
    }

    public class VerifyPresentationInput
    {
        [JsonPropertyName("verifiablePresentation")]
        public object VerifiablePresentation { get; set; } = new();
    }

    public class VerifyResponse
    {
        [JsonPropertyName("isValid")]
        public bool IsValid { get; set; }

        [JsonPropertyName("errors")]
        public List<string>? Errors { get; set; }
    }

    public class VerifyErrorDetail
    {
        [JsonPropertyName("issue")]
        public string Issue { get; set; } = string.Empty;
    }

    public class VerifyErrorResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("traceId")]
        public string TraceId { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("details")]
        public List<VerifyErrorDetail>? Details { get; set; }
    }

    public class VerifierConfiguration
    {
        public required string BasePath { get; set; }
    }

    public class VerifierApi
    {
        private readonly AuthProvider _authProvider;
        private readonly VerifierConfiguration _config;

        public VerifierApi(AuthProvider authProvider, VerifierConfiguration config)
        {
            _authProvider = authProvider;
            _config = config;
        }

        public async Task<VerifyResponse?> VerifyCredentialsAsync(VerifyCredentialsInput input)
        {
            var url = $"{_config.BasePath}/ver/v1/verifier/verify-vcs";
            return await PostVerificationRequestAsync(url, input);
        }

        public async Task<VerifyResponse?> VerifyPresentationAsync(VerifyPresentationInput input)
        {
            var url = $"{_config.BasePath}/ver/v1/verifier/verify-vp";
            return await PostVerificationRequestAsync(url, input);
        }

        private async Task<VerifyResponse?> PostVerificationRequestAsync(string url, object input)
        {
            var token = await _authProvider.FetchProjectScopedTokenAsync();

            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonPayload = JsonSerializer.Serialize(input, options);
            //Console.WriteLine($"Verifier API request to {url}: {jsonPayload}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync(url, new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json"));
            var responseBody = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"Verifier API response: {responseBody}");

            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<VerifyResponse>(responseBody, options);
            }
            else
            {
                //Console.WriteLine($"Verifier API error: {response.StatusCode}");
                return null;
            }
        }
    }

    public class VerifierClient
    {
        private readonly VerifierApi _verifierApi;
        private readonly AuthProviderParams _authProviderParams;

        public VerifierClient()
        {
            _authProviderParams = new AuthProviderParams
            {
                ProjectId = Environment.GetEnvironmentVariable("PROJECT_ID") ?? string.Empty,
                TokenId = Environment.GetEnvironmentVariable("TOKEN_ID") ?? string.Empty,
                KeyId = Environment.GetEnvironmentVariable("KEY_ID") ?? string.Empty,
                PrivateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY") ?? string.Empty,
                Passphrase = Environment.GetEnvironmentVariable("PASSPHRASE") ?? string.Empty,
                ApiGatewayUrl = Environment.GetEnvironmentVariable("API_GATEWAY_URL") ?? string.Empty,
                TokenEndpoint = Environment.GetEnvironmentVariable("TOKEN_ENDPOINT") ?? string.Empty
            };

            var authProvider = new AuthProvider(_authProviderParams);
            var verifierConfig = new VerifierConfiguration { BasePath = $"{_authProviderParams.ApiGatewayUrl}" };
            _verifierApi = new VerifierApi(authProvider, verifierConfig);
        }

        public async Task<VerifyResponse?> VerifyCredentialsAsync(VerifyCredentialsInput input)
        {
            //Console.WriteLine($"VerifierClient: VerifyCredentialsAsync with Project ID {_authProviderParams.ProjectId}");
            return await _verifierApi.VerifyCredentialsAsync(input);
        }

        public async Task<VerifyResponse?> VerifyPresentationAsync(VerifyPresentationInput input)
        {
            //Console.WriteLine($"VerifierClient: VerifyPresentationAsync with Project ID {_authProviderParams.ProjectId}");
            return await _verifierApi.VerifyPresentationAsync(input);
        }
    }
}
