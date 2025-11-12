using Affinidi_Login_Demo_App.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Affinidi_Login_Demo_App.Pages
{
    public class IdvModel : PageModel
    {

        private async Task<IActionResult> _InitiatIota(string queryId)
        {
            // Console.WriteLine($"[IDV] Initiating IDV data sharing with QueryId: {queryId}");

            var client = new IotaClient();

            var input = new InitiateDataSharingRequestInput
            {
                QueryId = queryId,
                CorrelationId = Guid.NewGuid().ToString(),
                Nonce = Guid.NewGuid().ToString("N"),
                RedirectUri = "http://localhost:5068/IDV",
                ConfigurationId = Environment.GetEnvironmentVariable("IOTA_CONFIG_ID_IDV") ?? string.Empty,
                Mode = "redirect"
            };

            // Console.WriteLine($"[IDV] CorrelationId: {input.CorrelationId}");
            // Console.WriteLine($"[IDV] ConfigurationId: {input.ConfigurationId}");

            var result = await client.Start(input);

            if (result != null)
            {
                // Console.WriteLine($"[IDV] Start successful - TransactionId: {result.TransactionId}");
                HttpContext.Session.SetString("CorrelationId", result.CorrelationId);
                HttpContext.Session.SetString("TransactionId", result.TransactionId);
                var redirectUrl = $"https://vault.affinidi.com/login?request={Uri.EscapeDataString(result.Jwt)}";
                // Console.WriteLine($"[IDV] Redirecting to Affinidi Vault");
                return Redirect(redirectUrl);
            }

            // Console.WriteLine($"[IDV] Failed to initiate data sharing");
            TempData["IotaMessage"] = "Not able to initiate data sharing. Please try again later.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostVerifyDrivingLicense()
        {
            // Console.WriteLine($"[IDV] OnPostVerifyDrivingLicense triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_IDV_DL") ?? string.Empty;
            // Console.WriteLine($"[IDV] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostVerifyAnyDocument()
        {
            // Console.WriteLine($"[IDV] OnPostVerifyAnyDocument triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_IDV_ANYDOC") ?? string.Empty;
            // Console.WriteLine($"[IDV] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostVerifyPassport()
        {
            // Console.WriteLine($"[IDV] OnPostVerifyPassport triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_IDV_PASSPORT") ?? string.Empty;
            // Console.WriteLine($"[IDV] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "response_code")] string? responseCode)
        {
            // Console.WriteLine($"[IDV] OnGetAsync called with response_code: {responseCode}");

            if (!string.IsNullOrEmpty(responseCode))
            {
                var correlationId = HttpContext.Session.GetString("CorrelationId");
                var transactionId = HttpContext.Session.GetString("TransactionId");
                // Console.WriteLine($"[IDV] Retrieved from session - CorrelationId: {correlationId}, TransactionId: {transactionId}");

                if (string.IsNullOrEmpty(correlationId) || string.IsNullOrEmpty(transactionId))
                {
                    // Console.WriteLine($"[IDV] Missing session data, redirecting to page");
                    return RedirectToPage();
                }

                // Call Complete API
                // Console.WriteLine($"[IDV] Calling Complete API");
                var client = new IotaClient();
                var input = new FetchIOTAVPResponseInput
                {
                    CorrelationId = correlationId,
                    TransactionId = transactionId,
                    ResponseCode = responseCode,
                    ConfigurationId = Environment.GetEnvironmentVariable("IOTA_CONFIG_ID_IDV") ?? string.Empty
                };

                var result = await client.Complete(input);

                if (result != null)
                {
                    // Console.WriteLine($"[IDV] Complete successful");
                    // Console.WriteLine($"[IDV] VP Token received (length: {result.VpToken?.Length ?? 0} characters)");
                    TempData["IotaMessage"] = $"Iota Complete successful";
                    if (!string.IsNullOrEmpty(result.VpToken))
                    {
                        var parsedJson = System.Text.Json.JsonDocument.Parse(result.VpToken);
                        var prettyJson = System.Text.Json.JsonSerializer.Serialize(
                            parsedJson,
                            new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                        );
                        TempData["VpToken"] = prettyJson;
                    }
                }
                else
                {
                    // Console.WriteLine($"[IDV] Complete failed - no result returned");
                }

                return RedirectToPage();
            }

            // Return the default page if responseCode is null or empty
            // Console.WriteLine($"[IDV] No response_code provided, showing default page");
            return Page();
        }

    }
}