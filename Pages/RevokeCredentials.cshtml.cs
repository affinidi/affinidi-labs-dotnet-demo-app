using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Affinidi_Login_Demo_App.Util;
using Newtonsoft.Json;
using AffinidiTdk.CredentialIssuanceClient.Model;
using Newtonsoft.Json.Linq;

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
                var projectId = Environment.GetEnvironmentVariable("PROJECT_ID") ?? string.Empty;
                var configurationId = Environment.GetEnvironmentVariable("CONFIGURATION_ID") ?? string.Empty;
                var credentialsClient = new CredentialsClient(projectId, configurationId);

                Console.WriteLine($"[Revoke] Fetching issuance records for projectId: {projectId}, configurationId: {configurationId}");

                var listdataResponseObj = await credentialsClient.ListIssuanceRecordsAsync(
                    projectId,
                    configurationId,
                    null,
                    null
                );

                // Find all flowIds matching the given IssuanceId
                var flowIdsToRevoke = new List<string>();

                // Use JObject to parse the response
                var listdataResponseJson = JsonConvert.SerializeObject(listdataResponseObj);
                var listdataResponseJObj = JObject.Parse(listdataResponseJson);

                var flowDataArray = listdataResponseJObj["flowData"] as JArray;

                if (flowDataArray != null)
                {
                    foreach (var item in flowDataArray)
                    {
                        var flowId = item["flowId"]?.ToString()?.Trim();
                        var id = item["id"]?.ToString();

                        Console.WriteLine($"[Revoke] flowId in record: '{flowId}', IssuanceId: '{IssuanceId?.Trim()}', id: '{id}'");

                        if (!string.IsNullOrEmpty(flowId) && flowId == IssuanceId?.Trim() && !string.IsNullOrEmpty(id))
                        {
                            flowIdsToRevoke.Add(id);
                            Console.WriteLine($"[Revoke] Matched! Added id: {id}");
                        }
                        else
                        {
                            Console.WriteLine($"[Revoke] No match for this record.");
                        }
                    }
                }

                Console.WriteLine($"[Revoke] Found {flowIdsToRevoke.Count} issuance record(s) for IssuanceId: {IssuanceId}");

                if (flowIdsToRevoke.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Issuance ID not found in records.");
                    return Page();
                }

                int successCount = 0;
                int failCount = 0;
                List<string> statusPurposes = new List<string>();

                foreach (var issuanceRecordId in flowIdsToRevoke)
                {
                    ChangeCredentialStatusInput.ChangeReasonEnum parsedReason;
                    if (!Enum.TryParse(RevocationReason?.Replace(" ", "").Replace("_", "").ToUpper(), out parsedReason))
                    {
                        ModelState.AddModelError(string.Empty, "Invalid revocation reason. Allowed: INVALID_CREDENTIAL or COMPROMISED_ISSUER");
                        return Page();
                    }

                    var input = new ChangeCredentialStatusInput
                    {
                        ChangeReason = parsedReason,
                        IssuanceRecordId = issuanceRecordId
                    };

                    Console.WriteLine($"[Revoke] Attempting revocation for IssuanceRecordId: {issuanceRecordId} with reason: {RevocationReason}");

                    var response = await credentialsClient.RevokeCredentialAsync(
                        projectId,
                        configurationId,
                        input
                    );

                    var responseJson = JsonConvert.SerializeObject(response);
                    var responseObj = JObject.Parse(responseJson);

                    string statusPurpose = "";
                    string statusListsDetailsStr = "";

                    if (responseObj["statusListsDetails"] != null)
                    {
                        var statusListsDetailsToken = responseObj["statusListsDetails"];
                        statusListsDetailsStr = statusListsDetailsToken != null ? statusListsDetailsToken.ToString(Formatting.Indented) : string.Empty;
                        var statusLists = statusListsDetailsToken as JArray;
                        if (statusLists != null && statusLists.Count > 0)
                        {
                            var firstStatus = statusLists[0];
                            statusPurpose = firstStatus["statusListPurpose"]?.ToString() ?? "";
                        }
                    }

                    if (responseObj["id"] != null)
                    {
                        Console.WriteLine($"[Revoke] Successfully revoked IssuanceRecordId: {issuanceRecordId}, Status Purpose: {statusPurpose}");
                        successCount++;
                        statusPurposes.Add(statusPurpose);
                        // Collect statusListsDetails for frontend
                        TempData["StatusListsDetails"] = statusListsDetailsStr;
                    }
                    else
                    {
                        Console.WriteLine($"[Revoke] Failed to revoke IssuanceRecordId: {issuanceRecordId}");
                        failCount++;
                    }
                }

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] =
                        $"Successfully revoked {successCount} credential(s) for Issuance ID '{IssuanceId}'. Status Purpose(s): {string.Join(", ", statusPurposes)}";
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to revoke credential(s). Please check the Issuance ID and try again.");
                    return Page();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[Revoke] Exception: {ex}");
                ModelState.AddModelError(string.Empty, $"Error revoking credential: {ex.Message}");
                return Page();
            }

            return RedirectToPage();
        }
    }
}
