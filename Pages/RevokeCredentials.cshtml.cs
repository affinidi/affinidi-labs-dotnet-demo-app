using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Affinidi_Login_Demo_App.Util;
using System.Threading.Tasks;

namespace Affinidi_Login_Demo_App.Pages
{
    public class RevokeCredentialsModel : PageModel
    {
        [BindProperty]
        [Display(Name = "Issuance ID")]
        [Required(ErrorMessage = "Issuance ID is required.")]
        public string? IssuanceId { get; set; }

        [BindProperty]
        [Display(Name = "Revocation Reason")]
        [Required(ErrorMessage = "Revocation reason is required.")]
        public string? RevocationReason { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var revokeClient = new RevokeClient();

                var listdataResponse = await revokeClient.ListIssuanceRecordsAsync();
                string? issuanceRecordId = null;

                if (listdataResponse?.FlowData != null)
                {
                    foreach (var item in listdataResponse.FlowData)
                    {
                        // Deserialize each item to a dictionary for dynamic access
                        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                            item.ToString() ?? "{}"
                        );

                        if (dict != null &&
                            dict.TryGetValue("flowId", out var flowIdObj) &&
                            flowIdObj?.ToString() == IssuanceId &&
                            dict.TryGetValue("id", out var idObj))
                        {
                            issuanceRecordId = idObj?.ToString();
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(issuanceRecordId))
                {
                    ModelState.AddModelError(string.Empty, "Issuance Record ID not found for the provided Issuance ID.");
                    return Page();
                }

                var input = new RevokeCredentialInput
                {
                    ChangeReason = RevocationReason!,
                    IssuanceRecordId = issuanceRecordId
                };

                var response = await revokeClient.RevokeCredentialAsync(input);

                if (response != null && !string.IsNullOrWhiteSpace(response.Id))
                {
                    TempData["SuccessMessage"] =
                        $"Credential with Issuance ID '{IssuanceId}' has been successfully revoked. Status Purpose: {response.StatusListsDetails?[0]?.StatusListPurpose}";
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to revoke credential. Please check the Issuance ID and try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error revoking credential: {ex.Message}");
                return Page();
            }

            return RedirectToPage();
        }
    }
}
