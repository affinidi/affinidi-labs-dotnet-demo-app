using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Affinidi_Login_Demo_App.Util;
using AffinidiTdk.CredentialVerificationClient.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Affinidi_Login_Demo_App.Pages
{
    public class CredentialVerificationModel : PageModel
    {
        private readonly VerifierClient _verifierClient;

        [BindProperty]
        public string? CredentialData { get; set; }

        [BindProperty]
        public string? CredentialType { get; set; }

        public string? VerificationResult { get; set; }

        public CredentialVerificationModel()
        {
            // Initialize VerifierClient with environment variables & AuthProvider
            _verifierClient = new VerifierClient();
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"[CredentialVerification] POST called with CredentialType: {CredentialType}");

            if (string.IsNullOrWhiteSpace(CredentialData) || string.IsNullOrWhiteSpace(CredentialType))
            {
                VerificationResult = "Please select a type and provide credential data.";
                Console.WriteLine("[CredentialVerification] Missing CredentialType or CredentialData.");
                return Page();
            }

            try
            {
                // Try to parse the JSON from the input field using Newtonsoft.Json
                object? parsedData;
                try
                {
                    parsedData = JToken.Parse(CredentialData);
                    Console.WriteLine($"[CredentialVerification] CredentialData Input: {JsonConvert.SerializeObject(parsedData, Formatting.None)}");
                }
                catch (JsonReaderException ex)
                {
                    VerificationResult = "Invalid JSON format in credential data.";
                    Console.WriteLine($"[CredentialVerification] Invalid JSON format: {ex.Message}");
                    return Page();
                }

                if (parsedData == null)
                {
                    VerificationResult = "Credential data cannot be empty.";
                    Console.WriteLine("[CredentialVerification] Credential data is empty after parsing.");
                    return Page();
                }
                if (CredentialType == "VC")
                {
                    var input = new VerifyCredentialInput(new List<object> { parsedData });

                    Console.WriteLine("[CredentialVerification] Calling VerifyCredentialsAsync...");

                    var response = await _verifierClient.VerifyCredentialsAsync(input);

                    VerificationResult = response != null
                        ? JsonConvert.SerializeObject(response, Formatting.Indented)
                        : "Verification failed or invalid response.";

                    Console.WriteLine($"[CredentialVerification] VC VerificationResult: {VerificationResult}");
                }
                else if (CredentialType == "VP")
                {
                    var input = new VerifyPresentationInput
                    {
                        VerifiablePresentation = parsedData
                    };

                    Console.WriteLine("[CredentialVerification] Calling VerifyPresentationAsync...");
                    var response = await _verifierClient.VerifyPresentationAsync(input);

                    VerificationResult = response != null
                        ? JsonConvert.SerializeObject(response, Formatting.Indented)
                        : "Verification failed or invalid response.";

                    Console.WriteLine($"[CredentialVerification] VP VerificationResult: {VerificationResult}");
                }
                else
                {
                    VerificationResult = $"Unknown credential type: {CredentialType}";
                    Console.WriteLine($"[CredentialVerification] Unknown credential type: {CredentialType}");
                }
            }
            catch (Exception ex)
            {
                VerificationResult = $"Error: {ex.Message}";
                Console.WriteLine($"[CredentialVerification] Exception occurred: {ex}");
            }

            return Page();
        }
    }
}
