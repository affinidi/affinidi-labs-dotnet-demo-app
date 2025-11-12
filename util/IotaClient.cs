using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Affinidi_Login_Demo_App.Util
{
    public class InitiateDataSharingRequestInput
    {
        [JsonPropertyName("queryId")]
        public required string QueryId { get; set; }

        [JsonPropertyName("correlationId")]
        public required string CorrelationId { get; set; }

        [JsonPropertyName("tokenMaxAge")]
        public int? TokenMaxAge { get; set; }

        [JsonPropertyName("nonce")]
        public required string Nonce { get; set; }

        [JsonPropertyName("redirectUri")]
        public required string RedirectUri { get; set; }

        [JsonPropertyName("configurationId")]
        public required string ConfigurationId { get; set; }

        [JsonPropertyName("mode")]
        public required string Mode { get; set; }
    }

    public class ApiResponseWrapper<T>
    {
        [JsonPropertyName("data")]
        public required T Data { get; set; }
    }
    public class InitiateDataSharingResponse
    {
        [JsonPropertyName("jwt")]
        public required string Jwt { get; set; }

        [JsonPropertyName("correlationId")]
        public required string CorrelationId { get; set; }

        [JsonPropertyName("transactionId")]
        public required string TransactionId { get; set; }
    }

    public class FetchIOTAVPResponseInput
    {
        [JsonPropertyName("correlationId")]
        public required string CorrelationId { get; set; }

        [JsonPropertyName("transactionId")]
        public required string TransactionId { get; set; }

        [JsonPropertyName("responseCode")]
        public required string ResponseCode { get; set; }

        [JsonPropertyName("configurationId")]
        public required string ConfigurationId { get; set; }
    }

    public class FetchIOTAVPResponse
    {
        [JsonPropertyName("correlationId")]
        public required string CorrelationId { get; set; }

        [JsonPropertyName("presentationSubmission")]
        public required string PresentationSubmission { get; set; }

        [JsonPropertyName("vpToken")]
        public required string VpToken { get; set; }
    }

    public class IotaConfiguration { public required string BasePath { get; set; } }

    public class IotaApi
    {
        AuthProvider _authProvider;
        IotaConfiguration _config;
        public IotaApi(AuthProvider authProvider, IotaConfiguration config)
        {
            _authProvider = authProvider;
            _config = config;
        }
        public virtual async Task<InitiateDataSharingResponse?> IotaStart(InitiateDataSharingRequestInput input)
        {
            // Console.WriteLine($"[IotaApi] IotaStart called");
            var localVarPath = $"ais/v1/initiate-data-sharing-request";
            var fullUrl = new Uri(new Uri(_config.BasePath), localVarPath).ToString();
            // Console.WriteLine($"[IotaApi] Request URL: {fullUrl}");

            var token = await _authProvider.FetchProjectScopedTokenAsync();

            // Use System.Text.Json with options to ignore null values
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true // Optional: for readability in console logs
            };

            var jsonPayload = JsonSerializer.Serialize(input, options);

            // Console.WriteLine($"[IotaApi] Iota Start Request Payload: {jsonPayload}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Console.WriteLine($"[IotaApi] Response Status: {response.StatusCode}");
                // Console.WriteLine($"[IotaApi] Response Body: {responseBody}");
                var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<InitiateDataSharingResponse>>(responseBody);
                if (apiResponse?.Data != null)
                {
                    // Console.WriteLine($"[IotaApi] CorrelationId: {apiResponse.Data.CorrelationId}");
                    // Console.WriteLine($"[IotaApi] TransactionId: {apiResponse.Data.TransactionId}");
                    // Console.WriteLine($"[IotaApi] JWT token length: {apiResponse.Data.Jwt?.Length ?? 0}");
                }
                return apiResponse?.Data;
            }
            else
            {
                // Console.WriteLine($"[IotaApi] Error Response Status: {response.StatusCode}");
                // Console.WriteLine($"[IotaApi] Error Response Body: {responseBody}");
                return null;
            }
        }

        public virtual async Task<FetchIOTAVPResponse?> IotaComplete(FetchIOTAVPResponseInput input)
        {
            // Console.WriteLine($"[IotaApi] IotaComplete called");
            var localVarPath = $"ais/v1/fetch-iota-response";
            var fullUrl = new Uri(new Uri(_config.BasePath), localVarPath).ToString();
            // Console.WriteLine($"[IotaApi] Request URL: {fullUrl}");

            var token = await _authProvider.FetchProjectScopedTokenAsync();

            // Use System.Text.Json with options to ignore null values
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true // Optional: for readability in console logs
            };

            var jsonPayload = JsonSerializer.Serialize(input, options);

            // Console.WriteLine($"[IotaApi] Iota Complete Request Payload: {jsonPayload}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // Console.WriteLine($"[IotaApi] Response Status: {response.StatusCode}");
                // Console.WriteLine($"[IotaApi] Response Body: {responseBody}");
                var apiResponse = JsonSerializer.Deserialize<FetchIOTAVPResponse>(responseBody);
                if (apiResponse != null)
                {
                    // Console.WriteLine($"[IotaApi] CorrelationId: {apiResponse.CorrelationId}");
                    // Console.WriteLine($"[IotaApi] PresentationSubmission length: {apiResponse.PresentationSubmission?.Length ?? 0}");
                    // Console.WriteLine($"[IotaApi] VpToken length: {apiResponse.VpToken?.Length ?? 0}");
                }
                return apiResponse;
            }
            else
            {
                // Console.WriteLine($"[IotaApi] Error Response Status: {response.StatusCode}");
                // Console.WriteLine($"[IotaApi] Error Response Body: {responseBody}");
                return null;
            }
        }
    }

    public class IotaClient
    {
        private readonly IotaApi _iotaApi;
        private readonly AuthProviderParams _authProviderParams;

        public IotaClient()
        {
            // Console.WriteLine("[IotaClient] Initializing IotaClient");

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

            Console.WriteLine($"[IotaClient] ProjectId: {_authProviderParams.ProjectId}");
            Console.WriteLine($"[IotaClient] ApiGatewayUrl: {_authProviderParams.ApiGatewayUrl}");

            AuthProvider authProvider = new AuthProvider(_authProviderParams);


            // Assuming SDK configuration objects
            var iotaConfig = new IotaConfiguration { BasePath = $"{_authProviderParams.ApiGatewayUrl}/ais" };
            Console.WriteLine($"[IotaClient] Iota API Base Path: {iotaConfig.BasePath}");
            _iotaApi = new IotaApi(authProvider, iotaConfig);

        }

        public async Task<InitiateDataSharingResponse?> Start(InitiateDataSharingRequestInput apiData)
        {
            Console.WriteLine($"[IotaClient] Start called with QueryId: {apiData.QueryId}");
            var response = await _iotaApi.IotaStart(apiData);
            return response;
        }

        public async Task<FetchIOTAVPResponse?> Complete(FetchIOTAVPResponseInput apiData)
        {
            Console.WriteLine($"[IotaClient] Complete called with CorrelationId: {apiData.CorrelationId}");
            var response = await _iotaApi.IotaComplete(apiData);
            return response;
        }

    }


}
