using Affinidi_Login_Demo_App.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Affinidi_Login_Demo_App.Pages
{
    public class IotaModel : PageModel
    {

        private async Task<IActionResult> _InitiatIota(string queryId)
        {
            // Console.WriteLine($"[Iota] Initiating Iota data sharing with QueryId: {queryId}");

            var client = new IotaClient();

            var input = new InitiateDataSharingRequestInput
            {
                QueryId = queryId,
                CorrelationId = Guid.NewGuid().ToString(),
                Nonce = Guid.NewGuid().ToString("N"),
                RedirectUri = "http://localhost:5068/Iota",
                ConfigurationId = Environment.GetEnvironmentVariable("IOTA_CONFIG_ID") ?? string.Empty,
                Mode = "redirect"
            };

            // Console.WriteLine($"[Iota] CorrelationId: {input.CorrelationId}");
            // Console.WriteLine($"[Iota] Nonce: {input.Nonce}");
            // Console.WriteLine($"[Iota] RedirectUri: {input.RedirectUri}");
            // Console.WriteLine($"[Iota] ConfigurationId: {input.ConfigurationId}");

            var result = await client.Start(input);

            if (result != null)
            {
                // Console.WriteLine($"[Iota] Start successful - TransactionId: {result.TransactionId}");
                HttpContext.Session.SetString("CorrelationId", result.CorrelationId);
                HttpContext.Session.SetString("TransactionId", result.TransactionId);
                var redirectUrl = $"https://vault.affinidi.com/login?request={Uri.EscapeDataString(result.Jwt)}";
                // Console.WriteLine($"[Iota] Redirecting to Affinidi Vault: {redirectUrl}");
                return Redirect(redirectUrl);
            }

            // Console.WriteLine($"[Iota] Failed to initiate data sharing");
            TempData["IotaMessage"] = "Not able to initiate data sharing. Please try again later.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSharePersonalInfo()
        {
            // Console.WriteLine($"[Iota] OnPostSharePersonalInfo triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_PERSONAL") ?? string.Empty;
            // Console.WriteLine($"[Iota] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareAddress()
        {
            // Console.WriteLine($"[Iota] OnPostShareAddress triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_ADDRESS") ?? string.Empty;
            // Console.WriteLine($"[Iota] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareEmployment()
        {
            // Console.WriteLine($"[Iota] OnPostShareEmployment triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_EMPLOYMENT") ?? string.Empty;
            // Console.WriteLine($"[Iota] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareEducation()
        {
            // Console.WriteLine($"[Iota] OnPostShareEducation triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_EDUCATION") ?? string.Empty;
            // Console.WriteLine($"[Iota] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnPostShareSelective()
        {
            // Console.WriteLine($"[Iota] OnPostShareSelective triggered");
            var queryId = Environment.GetEnvironmentVariable("IOTA_CREDENTIAL_QUERY_SELECTIVE_SHARING") ?? string.Empty;
            // Console.WriteLine($"[Iota] Using QueryId: {queryId}");
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "response_code")] string? responseCode)
        {
            // Console.WriteLine($"[Iota] OnGetAsync called with response_code: {responseCode}");

            if (!string.IsNullOrEmpty(responseCode))
            {
                var correlationId = HttpContext.Session.GetString("CorrelationId");
                var transactionId = HttpContext.Session.GetString("TransactionId");
                // Console.WriteLine($"[Iota] Retrieved from session - CorrelationId: {correlationId}, TransactionId: {transactionId}");

                if (string.IsNullOrEmpty(correlationId) || string.IsNullOrEmpty(transactionId))
                {
                    // Console.WriteLine($"[Iota] Missing session data, redirecting to page");
                    return RedirectToPage();
                }

                // Call Complete API
                // Console.WriteLine($"[Iota] Calling Complete API");
                var client = new IotaClient();
                var input = new FetchIOTAVPResponseInput
                {
                    CorrelationId = correlationId,
                    TransactionId = transactionId,
                    ResponseCode = responseCode,
                    ConfigurationId = Environment.GetEnvironmentVariable("IOTA_CONFIG_ID") ?? string.Empty
                };

                var result = await client.Complete(input);

                if (result != null)
                {
                    // Console.WriteLine($"[Iota] Complete successful");
                    // Console.WriteLine($"[Iota] VP Token received (length: {result.VpToken?.Length ?? 0} characters)");
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
                    // Console.WriteLine($"[Iota] Complete failed - no result returned");
                }

                return RedirectToPage();
            }

            // Return the default page if responseCode is null or empty
            // Console.WriteLine($"[Iota] No response_code provided, showing default page");
            return Page();
        }

    }
}