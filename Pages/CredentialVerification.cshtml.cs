using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Affinidi_Login_Demo_App.Util;
using System.Text.Json;

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
            // Console.WriteLine("[CredentialVerification] Page loaded");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Console.WriteLine($"[CredentialVerification] OnPostAsync triggered - Type: {CredentialType}");

            if (string.IsNullOrWhiteSpace(CredentialData) || string.IsNullOrWhiteSpace(CredentialType))
            {
                // Console.WriteLine("[CredentialVerification] Missing credential data or type");
                VerificationResult = "Please select a type and provide credential data.";
                return Page();
            }

            try
            {
                // Try to parse the JSON from the input field
                object? parsedData;
                try
                {
                    parsedData = JsonSerializer.Deserialize<object>(CredentialData);
                    // Console.WriteLine($"[CredentialVerification] Successfully parsed credential data");
                }
                catch (JsonException ex)
                {
                    // Console.WriteLine($"[CredentialVerification] JSON parsing error: {ex.Message}");
                    VerificationResult = "Invalid JSON format in credential data.";
                    return Page();
                }

                if (parsedData == null)
                {
                    // Console.WriteLine("[CredentialVerification] Parsed data is null");
                    VerificationResult = "Credential data cannot be empty.";
                    return Page();
                }

                if (CredentialType == "VC")
                {
                    // Console.WriteLine("[CredentialVerification] Verifying Verifiable Credential");
                    var input = new VerifyCredentialsInput
                    {
                        VerifiableCredentials = new List<object> { parsedData }
                    };

                    // Console.WriteLine("[CredentialVerification] Calling VerifyCredentialsAsync...");
                    var response = await _verifierClient.VerifyCredentialsAsync(input);

                    VerificationResult = response != null
                        ? JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })
                        : "Verification failed or invalid response.";

                    // Console.WriteLine($"[CredentialVerification] VC Verification complete - Valid: {response?.IsValid}");
                }
                else if (CredentialType == "VP")
                {
                    // Console.WriteLine("[CredentialVerification] Verifying Verifiable Presentation");
                    var input = new VerifyPresentationInput
                    {
                        VerifiablePresentation = parsedData
                    };

                    // Console.WriteLine("[CredentialVerification] Calling VerifyPresentationAsync...");
                    var response = await _verifierClient.VerifyPresentationAsync(input);

                    VerificationResult = response != null
                        ? JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })
                        : "Verification failed or invalid response.";

                    // Console.WriteLine($"[CredentialVerification] VP Verification complete - Valid: {response?.IsValid}");
                }
                else
                {
                    // Console.WriteLine($"[CredentialVerification] Unknown credential type: {CredentialType}");
                    VerificationResult = $"Unknown credential type: {CredentialType}";
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"[CredentialVerification] Error during verification: {ex.Message}");
                // Console.WriteLine($"[CredentialVerification] Stack trace: {ex.StackTrace}");
                VerificationResult = $"Error: {ex.Message}";
            }

            return Page();
        }
    }
}
