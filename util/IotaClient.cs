using Newtonsoft.Json;
using AffinidiTdk.IotaClient.Api;
using AffinidiTdk.IotaClient.Client;
using AffinidiTdk.IotaClient.Model;

namespace Affinidi_Login_Demo_App.Util
{

    public class IotaClient
    {
        public virtual async Task<InitiateDataSharingRequestOKData?> InitiateDataSharingRequest(InitiateDataSharingRequestInput input)
        {
            var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
            Console.WriteLine($"[StartIssuanceAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

            // Configure the API client
            var configuration = new Configuration();
            configuration.AddApiKey("authorization", projectScopedToken);

            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new IotaApi(httpClient, configuration, httpClientHandler);
            Console.WriteLine("[StartIssuanceAsync] IotaApi instance created.");
            {
                // Starting IOTA
                InitiateDataSharingRequestOK? result = await Task.Run(() => apiInstance.InitiateDataSharingRequest(input));
                Console.WriteLine($"[InitiateDataSharingRequest] IOTA start result: {JsonConvert.SerializeObject(result)}");

                return result?.Data;
            }

        }

        public virtual async Task<FetchIOTAVPResponseOK?> FetchIOTAVPResponse(FetchIOTAVPResponseInput input)
        {
            var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
            Console.WriteLine($"[StartIssuanceAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

            // Configure the API client
            var configuration = new Configuration();
            configuration.AddApiKey("authorization", projectScopedToken);

            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new IotaApi(httpClient, configuration, httpClientHandler);
            Console.WriteLine("[StartIssuanceAsync] IotaApi instance created.");
            {
                // Fetching IOTA VP response
                FetchIOTAVPResponseOK? result = await Task.Run(() => apiInstance.FetchIotaVpResponse(input));
                Console.WriteLine($"[FetchIOTAVPResponse] IOTA fetch result: {JsonConvert.SerializeObject(result)}");

                return result;
            }

        }
    }

}
