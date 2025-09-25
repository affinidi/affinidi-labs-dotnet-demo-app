using Affinidi_Login_Demo_App.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AffinidiTdk.IotaClient.Model;
using Newtonsoft.Json;

namespace Affinidi_Login_Demo_App.Pages
{
    public class IotaModel : PageModel
    {

        private async Task<IActionResult> _InitiatIota(string queryId)
        {
            var client = new IotaClient();

            var input = new InitiateDataSharingRequestInput(
                queryId,
                Guid.NewGuid().ToString(), // correlationId
                0, // tokenMaxAge (use 0 or your desired value)
                Guid.NewGuid().ToString("N"), // nonce
                "http://localhost:5068/Iota", // redirectUri
                Environment.GetEnvironmentVariable("IOTA_CONFIG_ID") ?? string.Empty, // configurationId
                InitiateDataSharingRequestInput.ModeEnum.Redirect // mode
            );

            Console.WriteLine($"[Iota] Initiating data sharing with input: {JsonConvert.SerializeObject(input)}");

            var result = await client.InitiateDataSharingRequest(input);

            if (result != null)
            {
                Console.WriteLine($"[Iota] InitiateDataSharingRequest result: {JsonConvert.SerializeObject(result)}");
                HttpContext.Session.SetString("CorrelationId", result.CorrelationId);
                HttpContext.Session.SetString("TransactionId", result.TransactionId);
                var redirectUrl = $"https://vault.affinidi.com/login?request={Uri.EscapeDataString(result.Jwt)}";
                Console.WriteLine($"[Iota] Redirecting to: {redirectUrl}");
                return Redirect(redirectUrl);
            }

            Console.WriteLine("[Iota] Not able to initiate data sharing. Please try again later.");
            TempData["IotaMessage"] = "Not able to initiate data sharing. Please try again later.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSharePersonalInfo()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_PERSONAL") ?? string.Empty;
            Console.WriteLine($"[Iota] OnPostSharePersonalInfo called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareAddress()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_ADDRESS") ?? string.Empty;
            Console.WriteLine($"[Iota] OnPostShareAddress called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareEmployment()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_EMPLOYMENT") ?? string.Empty;
            Console.WriteLine($"[Iota] OnPostShareEmployment called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareEducation()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_EDUCATION") ?? string.Empty;
            Console.WriteLine($"[Iota] OnPostShareEducation called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareSelective()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_SELECTIVE_SHARING") ?? string.Empty;
            Console.WriteLine($"[Iota] OnPostShareSelective called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "response_code")] string? responseCode)
        {
            Console.WriteLine($"[Iota] OnGetAsync called with response_code: {responseCode}");

            if (!string.IsNullOrEmpty(responseCode))
            {
                var correlationId = HttpContext.Session.GetString("CorrelationId");
                var transactionId = HttpContext.Session.GetString("TransactionId");
                Console.WriteLine($"[Iota] CorrelationId: {correlationId}, TransactionId: {transactionId}, ResponseCode: {responseCode}");

                if (string.IsNullOrEmpty(correlationId) || string.IsNullOrEmpty(transactionId))
                {
                    Console.WriteLine("[Iota] Missing CorrelationId or TransactionId in session.");
                    return RedirectToPage();
                }

                // Call Complete API
                var client = new IotaClient();
                var input = new FetchIOTAVPResponseInput(
                    correlationId,
                    transactionId,
                    responseCode,
                    Environment.GetEnvironmentVariable("IOTA_CONFIG_ID") ?? string.Empty
                );

                Console.WriteLine($"[Iota] FetchIOTAVPResponse input: {JsonConvert.SerializeObject(input)}");

                var result = await client.FetchIOTAVPResponse(input);

                if (result != null)
                {
                    Console.WriteLine($"[Iota] FetchIOTAVPResponse result: {JsonConvert.SerializeObject(result)}");
                    TempData["IotaMessage"] = $"Iota Complete successful";
                    var parsedJson = JsonConvert.DeserializeObject(result.VpToken);
                    var prettyJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                    TempData["VpToken"] = prettyJson;
                }
                else
                {
                    Console.WriteLine("[Iota] FetchIOTAVPResponse returned null.");
                }

                return RedirectToPage();
            }

            // Return the default page if responseCode is null or empty
            Console.WriteLine("[Iota] No response_code provided, returning default page.");
            return Page();
        }

    }
}