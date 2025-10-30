using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Affinidi_Login_Demo_App.Util
{
    public class RevokeClientConfiguration
    {
        public required string BasePath { get; set; }
        public required string ProjectId { get; set; }
        public required string ConfigurationId { get; set; }
    }

    public class RevokeClient
    {
        private readonly AuthProvider _authProvider;
        private readonly RevokeClientConfiguration _config;

        public RevokeClient()
        {
            var authProviderParams = new AuthProviderParams
            {
                ProjectId = Environment.GetEnvironmentVariable("PROJECT_ID") ?? string.Empty,
                TokenId = Environment.GetEnvironmentVariable("TOKEN_ID") ?? string.Empty,
                KeyId = Environment.GetEnvironmentVariable("KEY_ID") ?? string.Empty,
                PrivateKey = Environment.GetEnvironmentVariable("PRIVATE_KEY") ?? string.Empty,
                Passphrase = Environment.GetEnvironmentVariable("PASSPHRASE") ?? string.Empty,
                ApiGatewayUrl = Environment.GetEnvironmentVariable("API_GATEWAY_URL") ?? string.Empty,
                TokenEndpoint = Environment.GetEnvironmentVariable("TOKEN_ENDPOINT") ?? string.Empty
            };

            _authProvider = new AuthProvider(authProviderParams);

            _config = new RevokeClientConfiguration
            {
                BasePath = Environment.GetEnvironmentVariable("API_GATEWAY_URL") ?? string.Empty,
                ProjectId = Environment.GetEnvironmentVariable("PROJECT_ID") ?? string.Empty,
                ConfigurationId = Environment.GetEnvironmentVariable("CONFIGURATION_ID") ?? string.Empty
            };
        }

        private async Task<string?> SendRequestAsync(HttpMethod method, string url, object? body = null)
        {
            var token = await _authProvider.FetchProjectScopedTokenAsync();

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpContent? content = null;
            if (body != null)
            {
                var options = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(body, options);
                content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                //Console.WriteLine($"Revoke API request body: {json}");
            }

            var request = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            var response = await httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            //Console.WriteLine($"Revoke API response ({response.StatusCode}): {responseBody}");
            return responseBody;
        }

        public async Task<IssuanceListResponse?> ListIssuanceRecordsAsync()
        {
            var url = $"{_config.BasePath}/cis/v1/{_config.ProjectId}/configurations/{_config.ConfigurationId}/issuance/issuance-data-records";
            var responseBody = await SendRequestAsync(HttpMethod.Get, url);

            return string.IsNullOrWhiteSpace(responseBody)
                ? null
                : JsonSerializer.Deserialize<IssuanceListResponse>(responseBody);
        }

        public async Task<RevokeCredentialResponse?> RevokeCredentialAsync(RevokeCredentialInput input)
        {
            var url = $"{_config.BasePath}/cis/v1/{_config.ProjectId}/configurations/{_config.ConfigurationId}/issuance/change-status";
            var responseBody = await SendRequestAsync(HttpMethod.Post, url, input);

            return string.IsNullOrWhiteSpace(responseBody)
                ? null
                : JsonSerializer.Deserialize<RevokeCredentialResponse>(responseBody);
        }
    }

    // Models
    public class IssuanceListResponse
    {
        [JsonPropertyName("flowData")]
        public List<object>? FlowData { get; set; }
    }

    public class RevokeCredentialInput
    {
        [JsonPropertyName("changeReason")]
        public string ChangeReason { get; set; } = string.Empty;

        [JsonPropertyName("issuanceRecordId")]
        public string IssuanceRecordId { get; set; } = string.Empty;
    }

    public class RevokeCredentialResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; }

        [JsonPropertyName("projectId")]
        public string? ProjectId { get; set; }

        [JsonPropertyName("flowId")]
        public string? FlowId { get; set; }

        [JsonPropertyName("credentialTypeId")]
        public string? CredentialTypeId { get; set; }

        [JsonPropertyName("jsonLdContextUrl")]
        public string? JsonLdContextUrl { get; set; }

        [JsonPropertyName("jsonSchemaUrl")]
        public string? JsonSchemaUrl { get; set; }

        [JsonPropertyName("configurationId")]
        public string? ConfigurationId { get; set; }

        [JsonPropertyName("walletId")]
        public string? WalletId { get; set; }

        [JsonPropertyName("statusListsDetails")]
        public List<StatusListDetail>? StatusListsDetails { get; set; }
    }

    public class StatusListDetail
    {
        [JsonPropertyName("standard")]
        public string? Standard { get; set; }

        [JsonPropertyName("statusListIndex")]
        public string? StatusListIndex { get; set; }

        [JsonPropertyName("statusListId")]
        public string? StatusListId { get; set; }

        [JsonPropertyName("isActive")]
        public bool IsActive { get; set; }

        [JsonPropertyName("statusActivationReason")]
        public string? StatusActivationReason { get; set; }

        [JsonPropertyName("statusListPurpose")]
        public string? StatusListPurpose { get; set; }
    }
}
