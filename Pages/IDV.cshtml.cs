using Affinidi_Login_Demo_App.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AffinidiTdk.IotaClient.Model;
using Newtonsoft.Json;

namespace Affinidi_Login_Demo_App.Pages
{
    public class IdvModel : PageModel
    {

        private async Task<IActionResult> _InitiatIota(string queryId)
        {
            var client = new IotaClient();

            // Use constructor, not object initializer
            var input = new InitiateDataSharingRequestInput(
                queryId,
                Guid.NewGuid().ToString(), // correlationId
                0, // tokenMaxAge (set as needed)
                Guid.NewGuid().ToString("N"), // nonce
                "http://localhost:5068/IDV", // redirectUri
                Environment.GetEnvironmentVariable("IOTA_CONFIG_ID_IDV") ?? string.Empty, // configurationId
                InitiateDataSharingRequestInput.ModeEnum.Redirect // mode
            );

            Console.WriteLine($"[IDV] Initiating data sharing with input: {JsonConvert.SerializeObject(input)}");

            var result = await client.InitiateDataSharingRequest(input);

            if (result != null)
            {
                Console.WriteLine($"[IDV] InitiateDataSharingRequest result: {JsonConvert.SerializeObject(result)}");
                HttpContext.Session.SetString("CorrelationId", result.CorrelationId);
                HttpContext.Session.SetString("TransactionId", result.TransactionId);
                var redirectUrl = $"https://vault.affinidi.com/login?request={Uri.EscapeDataString(result.Jwt)}";
                Console.WriteLine($"[IDV] Redirecting to: {redirectUrl}");
                return Redirect(redirectUrl);
            }

            Console.WriteLine("[IDV] Not able to initiate data sharing. Please try again later.");
            TempData["IotaMessage"] = "Not able to initiate data sharing. Please try again later.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostVerifyDrivingLicense()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_IDV_DL") ?? string.Empty;
            Console.WriteLine($"[IDV] OnPostVerifyDrivingLicense called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostVerifyAnyDocument()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_IDV_ANYDOC") ?? string.Empty;
            Console.WriteLine($"[IDV] OnPostVerifyAnyDocument called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostVerifyPassport()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_IDV_PASSPORT") ?? string.Empty;
            Console.WriteLine($"[IDV] OnPostVerifyPassport called with QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "response_code")] string? responseCode)
        {
            Console.WriteLine($"[IDV] OnGetAsync called with response_code: {responseCode}");

            if (!string.IsNullOrEmpty(responseCode))
            {
                var correlationId = HttpContext.Session.GetString("CorrelationId");
                var transactionId = HttpContext.Session.GetString("TransactionId");
                Console.WriteLine($"[IDV] CorrelationId: {correlationId}, TransactionId: {transactionId}, ResponseCode: {responseCode}");

                if (string.IsNullOrEmpty(correlationId) || string.IsNullOrEmpty(transactionId))
                {
                    Console.WriteLine("[IDV] Missing CorrelationId or TransactionId in session.");
                    return RedirectToPage();
                }

                // Use constructor, not object initializer
                var input = new FetchIOTAVPResponseInput(
                    correlationId,
                    transactionId,
                    responseCode,
                    Environment.GetEnvironmentVariable("IOTA_CONFIG_ID") ?? string.Empty
                );

                Console.WriteLine($"[IDV] FetchIOTAVPResponse input: {JsonConvert.SerializeObject(input)}");

                var client = new IotaClient();
                var result = await client.FetchIOTAVPResponse(input);

                if (result != null)
                {
                    Console.WriteLine($"[IDV] FetchIOTAVPResponse result: {JsonConvert.SerializeObject(result)}");
                    TempData["IotaMessage"] = $"Iota Complete successful";
                    var parsedJson = JsonConvert.DeserializeObject(result.VpToken);
                    var prettyJson = JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
                    TempData["VpToken"] = prettyJson;
                }
                else
                {
                    Console.WriteLine("[IDV] FetchIOTAVPResponse returned null.");
                }

                return RedirectToPage();
            }

            // Return the default page if responseCode is null or empty
            Console.WriteLine("[IDV] No response_code provided, returning default page.");
            return Page();
        }

    }
}