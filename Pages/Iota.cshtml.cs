using Affinidi_Login_Demo_App.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Affinidi_Login_Demo_App.Pages
{
    public class IotaModel : PageModel
    {

        private async Task<IActionResult> _InitiatIota(string queryId)
        {
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

            var result = await client.Start(input);

            if (result != null)
            {
                HttpContext.Session.SetString("CorrelationId", result.CorrelationId);
                HttpContext.Session.SetString("TransactionId", result.TransactionId);
                var redirectUrl = $"https://vault.affinidi.com/login?request={Uri.EscapeDataString(result.Jwt)}";
                return Redirect(redirectUrl);
            }

            TempData["IotaMessage"] = "Not able to initiate data sharing. Please try again later.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSharePersonalInfo()
        {
            var queryId = Environment.GetEnvironmentVariable("IOTA_AVVANZ_CREDENTIAL_QUERY") ?? string.Empty;
            return await _InitiatIota(queryId);
        }

        public async Task<IActionResult> OnGetAsync([FromQuery(Name = "response_code")] string? responseCode)
        {
            if (!string.IsNullOrEmpty(responseCode))
            {
                var correlationId = HttpContext.Session.GetString("CorrelationId");
                var transactionId = HttpContext.Session.GetString("TransactionId");
                //Console.WriteLine($"Iota Complete called with CorrelationId: {correlationId}, TransactionId: {transactionId}, ResponseCode: {responseCode}");
                if (string.IsNullOrEmpty(correlationId) || string.IsNullOrEmpty(transactionId))
                {
                    return RedirectToPage();
                }

                // Call Complete API
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
                    TempData["IotaMessage"] = $"Iota Complete successful";
                    var parsedJson = System.Text.Json.JsonDocument.Parse(result.VpToken);
                    var prettyJson = System.Text.Json.JsonSerializer.Serialize(
                        parsedJson,
                        new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
                    );
                    TempData["VpToken"] = prettyJson;
                }

                return RedirectToPage();
            }

            // Return the default page if responseCode is null or empty
            return Page();
        }

    }
}