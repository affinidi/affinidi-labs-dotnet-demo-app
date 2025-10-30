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
            var localVarPath = $"ais/v1/initiate-data-sharing-request";
            var fullUrl = new Uri(new Uri(_config.BasePath), localVarPath).ToString();
            var token = await _authProvider.FetchProjectScopedTokenAsync();

            // Use System.Text.Json with options to ignore null values
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true // Optional: for readability in console logs
            };

            var jsonPayload = JsonSerializer.Serialize(input, options);

            //Console.WriteLine($"Iota Start API request: {jsonPayload}");
            //Console.WriteLine($"Authorization token: {token}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();
            // Console.WriteLine($"response details: {responseBody}");
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<ApiResponseWrapper<InitiateDataSharingResponse>>(responseBody);
                //Console.WriteLine($"Jwt: {apiResponse.Data?.Jwt}");
                //Console.WriteLine($"CorrelationId: {apiResponse.Data?.CorrelationId}");
                //Console.WriteLine($"TransactionId: {apiResponse.Data?.TransactionId}");
                return apiResponse.Data;
            }
            else
            {
                //Console.WriteLine($"Iota API error: {response.StatusCode}");
                //Console.WriteLine($"Error details: {responseBody}");
                return null;
            }
        }

        public virtual async Task<FetchIOTAVPResponse?> IotaComplete(FetchIOTAVPResponseInput input)
        {
            var localVarPath = $"ais/v1/fetch-iota-response";
            var fullUrl = new Uri(new Uri(_config.BasePath), localVarPath).ToString();
            var token = await _authProvider.FetchProjectScopedTokenAsync();

            // Use System.Text.Json with options to ignore null values
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true // Optional: for readability in console logs
            };

            var jsonPayload = JsonSerializer.Serialize(input, options);

            //Console.WriteLine($"Iota Complete API request: {jsonPayload}");
            //Console.WriteLine($"Authorization token: {token}");

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var request = new HttpRequestMessage(HttpMethod.Post, fullUrl)
            {
                Content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json")
            };
            var response = await httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"response details: {responseBody}");
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = JsonSerializer.Deserialize<FetchIOTAVPResponse>(responseBody);
                //Console.WriteLine($"CorrelationId: {apiResponse.CorrelationId}");
                //Console.WriteLine($"PresentationSubmission: {apiResponse.PresentationSubmission}");
                //Console.WriteLine($"VpToken: {apiResponse.VpToken}");
                return apiResponse;
            }
            else
            {
                //Console.WriteLine($"Iota Complete API error: {response.StatusCode}");
                //Console.WriteLine($"Error details: {responseBody}");
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
            AuthProvider authProvider = new AuthProvider(_authProviderParams);


            // Assuming SDK configuration objects
            var iotaConfig = new IotaConfiguration { BasePath = $"{_authProviderParams.ApiGatewayUrl}/ais" };
            //Console.WriteLine($"Iota API Base Path: {iotaConfig.BasePath}");
            _iotaApi = new IotaApi(authProvider, iotaConfig);

        }

        public async Task<InitiateDataSharingResponse?> Start(InitiateDataSharingRequestInput apiData)
        {
            //Console.WriteLine($"Iota Start called with Project ID: {_authProviderParams.ProjectId}");
            var response = await _iotaApi.IotaStart(apiData);
            return response;
        }

        public async Task<FetchIOTAVPResponse?> Complete(FetchIOTAVPResponseInput apiData)
        {
            //Console.WriteLine($"Iota Complete called with Project ID: {_authProviderParams.ProjectId}");
            var response = await _iotaApi.IotaComplete(apiData);
            return response;
        }

    }


}
