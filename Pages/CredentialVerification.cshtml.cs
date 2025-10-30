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
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CredentialData) || string.IsNullOrWhiteSpace(CredentialType))
            {
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
                }
                catch (JsonException)
                {
                    VerificationResult = "Invalid JSON format in credential data.";
                    return Page();
                }

                if (parsedData == null)
                {
                    VerificationResult = "Credential data cannot be empty.";
                    return Page();
                }

                if (CredentialType == "VC")
                {
                    var input = new VerifyCredentialsInput
                    {
                        VerifiableCredentials = new List<object> { parsedData }
                    };

                    //Console.WriteLine("Calling VerifyCredentialsAsync...");
                    var response = await _verifierClient.VerifyCredentialsAsync(input);

                    VerificationResult = response != null
                        ? JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })
                        : "Verification failed or invalid response.";
                }
                else if (CredentialType == "VP")
                {
                    var input = new VerifyPresentationInput
                    {
                        VerifiablePresentation = parsedData
                    };

                    //Console.WriteLine("Calling VerifyPresentationAsync...");
                    var response = await _verifierClient.VerifyPresentationAsync(input);

                    VerificationResult = response != null
                        ? JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true })
                        : "Verification failed or invalid response.";
                }
                else
                {
                    VerificationResult = $"Unknown credential type: {CredentialType}";
                }
            }
            catch (Exception ex)
            {
                VerificationResult = $"Error: {ex.Message}";
            }

            return Page();
        }
    }
}
