using System.Net.Http.Headers;
using AffinidiTdk.CredentialVerificationClient.Api;
using AffinidiTdk.CredentialVerificationClient.Client;
using AffinidiTdk.CredentialVerificationClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Affinidi_Login_Demo_App.Util
{
    public class VerifierClient
    {
        public async Task<VerifyCredentialOutput?> VerifyCredentialsAsync(VerifyCredentialInput input)
        {
            Console.WriteLine("[VerifyCredentialsAsync] Starting Verification process...");
            Console.WriteLine($"[VerifyCredentialsAsync] Input: {JsonConvert.SerializeObject(input)}");

            var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
            Console.WriteLine($"[VerifyCredentialsAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

            var configuration = new Configuration();
            configuration.AddApiKey("authorization", projectScopedToken);

            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new DefaultApi(httpClient, configuration, httpClientHandler);
            Console.WriteLine("[VerifyCredentialsAsync] DefaultApi instance created.");

            try
            {
                VerifyCredentialOutput result = await Task.Run(() => apiInstance.VerifyCredentials(input));
                Console.WriteLine($"[VerifyCredentialsAsync] Verification result: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VerifyCredentialsAsync] Exception occurred during verification: {ex}");
                throw;
            }
        }

        public async Task<VerifyPresentationOutput?> VerifyPresentationAsync(VerifyPresentationInput input)
        {
            Console.WriteLine("[VerifyPresentationAsync] Starting Verification process...");
            Console.WriteLine($"[VerifyPresentationAsync] Input: {JsonConvert.SerializeObject(input)}");

            var projectScopedToken = await AuthProviderClient.Instance.GetProjectScopedToken();
            Console.WriteLine($"[VerifyPresentationAsync] projectScopedToken: {(string.IsNullOrEmpty(projectScopedToken) ? "EMPTY" : "REDACTED")}");

            var configuration = new Configuration();
            configuration.AddApiKey("authorization", projectScopedToken);

            HttpClient httpClient = new HttpClient();
            HttpClientHandler httpClientHandler = new HttpClientHandler();
            var apiInstance = new DefaultApi(httpClient, configuration, httpClientHandler);
            Console.WriteLine("[VerifyPresentationAsync] DefaultApi instance created.");

            try
            {
                VerifyPresentationOutput result = await Task.Run(() => apiInstance.VerifyPresentation(input));
                Console.WriteLine($"[VerifyPresentationAsync] Verification result: {JsonConvert.SerializeObject(result)}");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VerifyPresentationAsync] Exception occurred during verification: {ex}");
                throw;
            }
        }
    }
}
