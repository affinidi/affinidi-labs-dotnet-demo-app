using Newtonsoft.Json;
using AffinidiTdk.CredentialIssuanceClient.Api;
using AffinidiTdk.CredentialIssuanceClient.Client;
using AffinidiTdk.CredentialIssuanceClient.Model;

namespace Affinidi_Login_Demo_App.Util
{
    public class CredentialsClient
    {
        private static readonly HttpClient httpClient = new HttpClient();

        private readonly string projectId;
        private readonly string vaultUrl;

        public CredentialsClient(string projectId, string vaultUrl)
        {
            this.projectId = projectId;
            this.vaultUrl = vaultUrl;
        }

        public async Task<object> StartIssuanceAsync(List<object> data, string claimMode)
        {
            try
            {
                Console.WriteLine("[StartIssuanceAsync] Starting issuance process...");
                Console.WriteLine($"[StartIssuanceAsync] projectId: {projectId}");
                Console.WriteLine($"[StartIssuanceAsync] vaultUrl: {vaultUrl}");
                Console.WriteLine($"[StartIssuanceAsync] claimMode: {claimMode}");
                Console.WriteLine($"[StartIssuanceAsync] data: {JsonConvert.SerializeObject(data)}");

                // Fetch the project scoped token asynchronously
                var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
                Console.WriteLine($"[StartIssuanceAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

                // Configure the API client
                var configuration = new Configuration();
                configuration.AddApiKey("authorization", projectScopedToken);

                HttpClient httpClient = new HttpClient();
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                var apiInstance = new IssuanceApi(httpClient, configuration, httpClientHandler);
                Console.WriteLine("[StartIssuanceAsync] IssuanceApi instance created.");

                var requestJson = new
                {
                    data = data,
                    claimMode = claimMode
                };

                Console.WriteLine($"[StartIssuanceAsync] requestJson: {JsonConvert.SerializeObject(requestJson)}");

                // Map requestJson to StartIssuanceInput
                var startIssuanceInput = JsonConvert.DeserializeObject<StartIssuanceInput>(
                    JsonConvert.SerializeObject(requestJson)
                );

                Console.WriteLine($"[StartIssuanceAsync] startIssuanceInput: {JsonConvert.SerializeObject(startIssuanceInput)}");

                if (startIssuanceInput == null)
                {
                    Console.WriteLine("[StartIssuanceAsync] Error: startIssuanceInput is null.");
                    return new { success = false, error = "Failed to map requestJson to StartIssuanceInput." };
                }

                var apiResponse = await apiInstance.StartIssuanceAsync(projectId, startIssuanceInput);

                Console.WriteLine($"[StartIssuanceAsync] apiResponse: {JsonConvert.SerializeObject(apiResponse)}");

                var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(apiResponse)
                );

                // Add vaultLink if needed
                if (response != null && response.ContainsKey("credentialOfferUri"))
                {
                    response["vaultLink"] = $"{vaultUrl}/claim?credential_offer_uri={response["credentialOfferUri"]}";
                    Console.WriteLine($"[StartIssuanceAsync] vaultLink: {response["vaultLink"]}");
                }

                Console.WriteLine("[StartIssuanceAsync] Issuance process completed.");
                return response ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StartIssuanceAsync] Exception: {ex}");
                return new { success = false, error = ex.Message };
            }
        }

        public async Task<object?> ListIssuanceRecordsAsync(string projectId, string configurationId, int? limit = null, string? exclusiveStartKey = null)
        {
            try
            {
                Console.WriteLine("[ListIssuanceRecordsAsync] Fetching issuance list...");
                Console.WriteLine($"[ListIssuanceRecordsAsync] projectId: {projectId}");

                // Fetch the project scoped token asynchronously
                var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
                Console.WriteLine($"[ListIssuanceRecordsAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

                // Configure the API client
                var configuration = new Configuration();
                configuration.AddApiKey("authorization", projectScopedToken);

                HttpClient httpClient = new HttpClient();
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                var apiInstance = new DefaultApi(httpClient, configuration, httpClientHandler);
                Console.WriteLine("[ListIssuanceRecordsAsync] IssuanceApi instance created.");

                var apiResponse = await apiInstance.ListIssuanceDataRecordsAsync(projectId, configurationId, limit, exclusiveStartKey);

                Console.WriteLine($"[ListIssuanceRecordsAsync] apiResponse: {JsonConvert.SerializeObject(apiResponse)}");

                return apiResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IssuanceListAsync] Exception: {ex}");
                return null;
            }
        }

        public async Task<object?> RevokeCredentialAsync(string projectId, string configurationId, ChangeCredentialStatusInput changeCredentialStatusInput)
        {
            try
            {
                Console.WriteLine("[RevokeCredentialAsync] Revoking credential...");
                Console.WriteLine($"[RevokeCredentialAsync] projectId: {projectId}");
                Console.WriteLine($"[RevokeCredentialAsync] configurationId: {configurationId}");
                Console.WriteLine($"[RevokeCredentialAsync] changeCredentialStatusInput: {JsonConvert.SerializeObject(changeCredentialStatusInput)}");

                // Fetch the project scoped token asynchronously
                var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
                Console.WriteLine($"[RevokeCredentialAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

                // Configure the API client
                var configuration = new Configuration();
                configuration.AddApiKey("authorization", projectScopedToken);

                HttpClient httpClient = new HttpClient();
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                var apiInstance = new DefaultApi(httpClient, configuration, httpClientHandler);
                Console.WriteLine("[RevokeCredentialAsync] IssuanceApi instance created.");

                // Use the provided ChangeCredentialStatusInput directly
                var apiResponse = apiInstance.ChangeCredentialStatus(projectId, configurationId, changeCredentialStatusInput);

                Console.WriteLine($"[RevokeCredentialAsync] apiResponse: {JsonConvert.SerializeObject(apiResponse)}");

                return apiResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RevokeCredentialAsync] Exception: {ex}");
                return null;
            }
        }

        public async Task<object> CheckCredentialStatusAsync(string projectId, string configurationId, string issuanceRecordId)
        {
            try
            {
                Console.WriteLine("[CheckCredentialStatusAsync] Checking credential status...");
                Console.WriteLine($"[CheckCredentialStatusAsync] projectId: {projectId}");
                Console.WriteLine($"[CheckCredentialStatusAsync] configurationId: {configurationId}");
                Console.WriteLine($"[CheckCredentialStatusAsync] issuanceRecordId: {issuanceRecordId}");

                // Fetch the project scoped token asynchronously
                var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
                Console.WriteLine($"[CheckCredentialStatusAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

                // Configure the API client
                var configuration = new Configuration();
                configuration.AddApiKey("authorization", projectScopedToken);

                HttpClient httpClient = new HttpClient();
                HttpClientHandler httpClientHandler = new HttpClientHandler();
                var apiInstance = new CredentialsApi(httpClient, configuration, httpClientHandler);
                Console.WriteLine("[CheckCredentialStatusAsync] IssuanceApi instance created.");

                var apiResponse = await apiInstance.GetIssuanceIdClaimedCredentialAsync(projectId, configurationId, issuanceRecordId);

                Console.WriteLine($"[CheckCredentialStatusAsync] apiResponse: {JsonConvert.SerializeObject(apiResponse)}");

                return apiResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CheckCredentialStatusAsync] Exception: {ex}");
                return new { success = false, error = ex.Message };
            }
        }
    }
}




