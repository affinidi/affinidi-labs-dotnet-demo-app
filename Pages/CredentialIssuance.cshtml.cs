using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Affinidi_Login_Demo_App.Util;

namespace Affinidi_Login_Demo_App.Pages
{

    public class MetaData
    {
        public string expirationDate { get; set; }
    }

    [IgnoreAntiforgeryToken]
    public class CredentialIssuanceModel : PageModel
    {
        [BindProperty]
        public string? IssuanceId { get; set; }

        public void OnGet()
        {
            // Console.WriteLine("[CredentialIssuance] Page loaded");
        }

        private async Task<IActionResult> IssueCredential(string credentialTypeId, object credentialData, bool isRevocable, bool isExpiry)
        {
            // Console.WriteLine($"[CredentialIssuance] IssueCredential called for type: {credentialTypeId}, Revocable: {isRevocable}, Expiry: {isExpiry}");

            var dataToIssue = new CredentialData
            {
                credentialTypeId = credentialTypeId,
                credentialData = credentialData
            };

            // Conditionally add metadata for expiry. The property is only added if isExpiry is true.
            if (isExpiry)
            {
                // Console.WriteLine($"[CredentialIssuance] Adding expiry metadata");
                dataToIssue.metaData = new MetaData
                {
                    expirationDate = "2027-09-01T00:00:00.000Z"
                };
            }

            // Conditionally add status list details for revocability. The property is only added if isRevocable is true.
            if (isRevocable)
            {
                // Console.WriteLine($"[CredentialIssuance] Adding revocable status list details");
                dynamic revocablePayload = new
                {
                    purpose = "REVOCABLE",
                    standard = "RevocationList2020"
                };
                dataToIssue.statusListDetails = new List<dynamic> { revocablePayload };
            }

            var issuanceInput = new StartIssuanceInput
            {
                claimMode = ClaimModeEnum.TX_CODE,
                data = new List<CredentialData> { dataToIssue }
            };

            var credentialsClient = new CredentialsClient();
            // Console.WriteLine($"[CredentialIssuance] Calling IssuanceStart API");
            // Console.WriteLine($"[CredentialIssuance] Issuance Input: {JsonConvert.SerializeObject(issuanceInput, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })}");

            var issuanceResponse = await credentialsClient.IssuanceStart(issuanceInput);
            // Console.WriteLine($"[CredentialIssuance] Issuance Response: {JsonConvert.SerializeObject(issuanceResponse)}");

            var credentialOfferUri = issuanceResponse?.CredentialOfferUri ?? "";
            var vaultUrl = Environment.GetEnvironmentVariable("PUBLIC_VAULT_URL") ?? "https://vault.affinidi.com";
            var claimUrl = $"{vaultUrl}/claim?credential_offer_uri={Uri.EscapeDataString(credentialOfferUri)}";

            // Console.WriteLine($"[CredentialIssuance] Credential Offer URI: {credentialOfferUri}");
            // Console.WriteLine($"[CredentialIssuance] Claim URL: {claimUrl}");
            // Console.WriteLine($"[CredentialIssuance] Issuance ID: {issuanceResponse?.IssuanceId}");
            // Console.WriteLine($"[CredentialIssuance] TX Code: {issuanceResponse?.TxCode}");
            // Console.WriteLine($"[CredentialIssuance] Expires In: {issuanceResponse?.ExpiresIn} seconds");

            TempData["IssuanceMessage"] = $"{credentialTypeId} Credential issued. Check logs for details.";
            TempData["CredentialOfferUri"] = credentialOfferUri;
            TempData["IssuanceId"] = issuanceResponse?.IssuanceId;
            TempData["ExpiresIn"] = issuanceResponse?.ExpiresIn.ToString() ?? "0";
            TempData["TxCode"] = issuanceResponse?.TxCode;
            TempData["ClaimUrl"] = claimUrl;

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostIssuePersonalInfo([FromForm] bool revocablePersonalInfo, [FromForm] bool expiryPersonalInfo)
        {
            // Console.WriteLine($"[CredentialIssuance] OnPostIssuePersonalInfo triggered - Revocable: {revocablePersonalInfo}, Expiry: {expiryPersonalInfo}");

            // Use a dynamic object to build the credential data
            dynamic credentialData = new
            {
                name = new
                {
                    givenName = "Grajesh",
                    familyName = "Chandra",
                    nickname = "Grajesh Testing"
                },
                birthdate = "01-01-1990",
                birthCountry = "India",
                citizenship = "Indian",
                phoneNumber = "7666009585",
                nationalIdentification = new
                {
                    idNumber1 = "pan",
                    idType1 = "askjd13212432d"
                },
                email = "grajesh.c@affinidi.com",
                gender = "male",
                maritalStatus = "married",
                verificationStatus = "Completed",
                verificationEvidence = new
                {
                    evidenceName1 = "letter",
                    evidenceURL1 = "http://localhost"
                },
                verificationRemarks = "Done"
            };

            return await IssueCredential(Environment.GetEnvironmentVariable("PERSONAL_INFORMATION_CREDENTIAL_TYPE_ID") ?? "AvvanzPersonalInformationVerification", credentialData, revocablePersonalInfo, expiryPersonalInfo);
        }

        public async Task<IActionResult> OnPostIssueEducation([FromForm] bool revocableEducation, [FromForm] bool expiryEducation)
        {
            // Console.WriteLine($"[CredentialIssuance] OnPostIssueEducation triggered - Revocable: {revocableEducation}, Expiry: {expiryEducation}");

            dynamic credentialData = new
            {
                candidateDetails = new
                {
                    name = "Grajesh Chandra",
                    phoneNumber = "7666009585",
                    email = "grajesh.c@affinidi.com",
                    gender = "male"
                },
                institutionDetails = new
                {
                    institutionName = "Affinidi",
                    institutionAddress = new
                    {
                        addressLine1 = "Varthur, Gunjur",
                        addressLine2 = "B305, Candeur Landmark, Tower Eiffel",
                        postalCode = "560087",
                        addressRegion = "Karnataka",
                        addressCountry = "India"
                    },
                    institutionContact1 = "+91 1234567890",
                    institutionContact2 = "+91 1234567890",
                    institutionEmail = "test@affinidi.com",
                    institutionWebsiteURL = "affinidi.com"
                },
                educationDetails = new
                {
                    qualification = "Graduation",
                    course = "MBA",
                    graduationDate = "12-08-2013",
                    dateAttendedFrom = "12-08-2011",
                    dateAttendedTo = "12-07-2013",
                    educationRegistrationID = "admins1223454356"
                },
                verificationStatus = "Verified",
                verificationEvidence = new
                {
                    evidenceName1 = "Degree",
                    evidenceURL1 = "http://localhost"
                },
                verificationRemarks = "completed"
            };

            return await IssueCredential(Environment.GetEnvironmentVariable("EDUCATION_CREDENTIAL_TYPE_ID") ?? "education_credential_type_id", credentialData, revocableEducation, expiryEducation);
        }

        public async Task<IActionResult> OnPostIssueEmployment([FromForm] bool revocableEmployment, [FromForm] bool expiryEmployment)
        {
            // Console.WriteLine($"[CredentialIssuance] OnPostIssueEmployment triggered - Revocable: {revocableEmployment}, Expiry: {expiryEmployment}");

            dynamic credentialData = new
            {
                candidateDetails = new
                {
                    name = "Grajesh Chandra",
                    phoneNumber = "7666009585",
                    email = "grajesh.c@affinidi.com",
                    gender = "male"
                },
                employerDetails = new
                {
                    companyName = "Affinidi",
                    companyAddress = new
                    {
                        addressLine1 = "Varthur, Gunjur",
                        addressLine2 = "B305, Candeur Landmark, Tower Eiffel",
                        postalCode = "560087",
                        addressRegion = "Karnataka",
                        addressCountry = "India"
                    },
                    hRDetails = new
                    {
                        hRfirstName = "Testing",
                        hRLastName = "HR",
                        hREmail = "hr@affinidi.com",
                        hRDesignation = "Lead HR",
                        hRContactNumber1 = "+911234567789",
                        whenToContact = "9:00-6:00 PM"
                    }
                },
                employmentDetails = new
                {
                    designation = "Testing",
                    employmentStatus = "Fulltime",
                    annualisedSalary = "10000",
                    currency = "INR",
                    tenure = new
                    {
                        fromDate = "05-2022",
                        toDate = "06-2050"
                    },
                    reasonForLeaving = "Resignation",
                    eligibleForRehire = "Yes"
                },
                verificationStatus = "Completed",
                verificationEvidence = new
                {
                    evidenceName1 = "letter",
                    evidenceURL1 = "http://localhost"
                },
                verificationRemarks = "Done"
            };

            return await IssueCredential(Environment.GetEnvironmentVariable("EMPLOYMENT_CREDENTIAL_TYPE_ID") ?? "employment_credential_type_id", credentialData, revocableEmployment, expiryEmployment);
        }

        public async Task<IActionResult> OnPostIssueResidence([FromForm] bool revocableResidence, [FromForm] bool expiryResidence)
        {
            // Console.WriteLine($"[CredentialIssuance] OnPostIssueResidence triggered - Revocable: {revocableResidence}, Expiry: {expiryResidence}");

            dynamic credentialData = new
            {
                address = new
                {
                    addressLine1 = "Varthur, Gunjur",
                    addressLine2 = "B305, Candeur Landmark, Tower Eiffel",
                    postalCode = "560087",
                    addressRegion = "Karnataka",
                    addressCountry = "India"
                },
                ownerDetails = new
                {
                    ownerName = "TestOwner",
                    ownerContactDetails1 = "+912325435634"
                },
                neighbourDetails = new
                {
                    neighbourName = "Test Neighbour",
                    neighbourContactDetails1 = "+912325435634"
                },
                stayDetails = new
                {
                    fromDate = "01-01-2000",
                    toDate = "01-01-2020"
                },
                verificationStatus = "Completed",
                verificationEvidence = new
                {
                    evidenceName1 = "Letter",
                    evidenceURL1 = "http://localhost"
                },
                verificationRemarks = "done"
            };

            return await IssueCredential(Environment.GetEnvironmentVariable("ADDRESS_CREDENTIAL_TYPE_ID") ?? "address_credential_type_id", credentialData, revocableResidence, expiryResidence);
        }

        public async Task<IActionResult> OnPostIssueBatch([FromForm] bool revocableBatch, [FromForm] bool expiryBatch)
        {
            // Console.WriteLine($"[CredentialIssuance] OnPostIssueBatch triggered - Revocable: {revocableBatch}, Expiry: {expiryBatch}");

            var data = new List<CredentialData>();

            // Create and conditionally add each credential to the list

            // Personal Information Credential
            var personalInfoCredential = new CredentialData
            {
                credentialTypeId = Environment.GetEnvironmentVariable("PERSONAL_INFORMATION_CREDENTIAL_TYPE_ID") ?? "personal_information_credential_type_id",
                credentialData = new
                {
                    name = new
                    {
                        givenName = "Grajesh",
                        familyName = "Chandra",
                        nickname = "Grajesh Testing"
                    },
                    birthdate = "01-01-1990",
                    birthCountry = "India",
                    citizenship = "Indian",
                    phoneNumber = "7666009585",
                    nationalIdentification = new
                    {
                        idNumber1 = "pan",
                        idType1 = "askjd13212432d"
                    },
                    email = "grajesh.c@affinidi.com",
                    gender = "male",
                    maritalStatus = "married",
                    verificationStatus = "Completed",
                    verificationEvidence = new
                    {
                        evidenceName1 = "letter",
                        evidenceURL1 = "http://localhost"
                    },
                    verificationRemarks = "Done"
                }
            };
            if (expiryBatch)
            {
                personalInfoCredential.metaData = new MetaData { expirationDate = "2027-09-01T00:00:00.000Z" };
            }
            if (revocableBatch)
            {
                personalInfoCredential.statusListDetails = new List<dynamic> { new { purpose = "REVOCABLE", standard = "RevocationList2020" } };
            }
            data.Add(personalInfoCredential);

            // Address Credential
            var addressCredential = new CredentialData
            {
                credentialTypeId = Environment.GetEnvironmentVariable("ADDRESS_CREDENTIAL_TYPE_ID") ?? "address_credential_type_id",
                credentialData = new
                {
                    address = new
                    {
                        addressLine1 = "Varthur, Gunjur",
                        addressLine2 = "B305, Candeur Landmark, Tower Eiffel",
                        postalCode = "560087",
                        addressRegion = "Karnataka",
                        addressCountry = "India"
                    },
                    ownerDetails = new
                    {
                        ownerName = "TestOwner",
                        ownerContactDetails1 = "+912325435634"
                    },
                    neighbourDetails = new
                    {
                        neighbourName = "Test Neighbour",
                        neighbourContactDetails1 = "+912325435634"
                    },
                    stayDetails = new
                    {
                        fromDate = "01-01-2000",
                        toDate = "01-01-2020"
                    },
                    verificationStatus = "Completed",
                    verificationEvidence = new
                    {
                        evidenceName1 = "Letter",
                        evidenceURL1 = "http://localhost"
                    },
                    verificationRemarks = "done"
                }
            };
            if (expiryBatch)
            {
                addressCredential.metaData = new MetaData { expirationDate = "2027-09-01T00:00:00.000Z" };
            }
            if (revocableBatch)
            {
                addressCredential.statusListDetails = new List<dynamic> { new { purpose = "REVOCABLE", standard = "RevocationList2020" } };
            }
            data.Add(addressCredential);

            // Education Credential
            var educationCredential = new CredentialData
            {
                credentialTypeId = Environment.GetEnvironmentVariable("EDUCATION_CREDENTIAL_TYPE_ID") ?? "education_credential_type_id",
                credentialData = new
                {
                    candidateDetails = new
                    {
                        name = "Grajesh Chandra",
                        phoneNumber = "7666009585",
                        email = "grajesh.c@affinidi.com",
                        gender = "male"
                    },
                    institutionDetails = new
                    {
                        institutionName = "Affinidi",
                        institutionAddress = new
                        {
                            addressLine1 = "Varthur, Gunjur",
                            addressLine2 = "B305, Candeur Landmark, Tower Eiffel",
                            postalCode = "560087",
                            addressRegion = "Karnataka",
                            addressCountry = "India"
                        },
                        institutionContact1 = "+91 1234567890",
                        institutionContact2 = "+91 1234567890",
                        institutionEmail = "test@affinidi.com",
                        institutionWebsiteURL = "affinidi.com"
                    },
                    educationDetails = new
                    {
                        qualification = "Graduation",
                        course = "MBA",
                        graduationDate = "12-08-2013",
                        dateAttendedFrom = "12-08-2011",
                        dateAttendedTo = "12-07-2013",
                        educationRegistrationID = "admins1223454356"
                    },
                    verificationStatus = "Verified",
                    verificationEvidence = new
                    {
                        evidenceName1 = "Degree",
                        evidenceURL1 = "http://localhost"
                    },
                    verificationRemarks = "completed"
                }
            };
            if (expiryBatch)
            {
                educationCredential.metaData = new MetaData { expirationDate = "2027-09-01T00:00:00.000Z" };
            }
            if (revocableBatch)
            {
                educationCredential.statusListDetails = new List<dynamic> { new { purpose = "REVOCABLE", standard = "RevocationList2020" } };
            }
            data.Add(educationCredential);

            // Employment Credential
            var employmentCredential = new CredentialData
            {
                credentialTypeId = Environment.GetEnvironmentVariable("EMPLOYMENT_CREDENTIAL_TYPE_ID") ?? "employment_credential_type_id",
                credentialData = new
                {
                    candidateDetails = new
                    {
                        name = "Grajesh Chandra",
                        phoneNumber = "7666009585",
                        email = "grajesh.c@affinidi.com",
                        gender = "male"
                    },
                    employerDetails = new
                    {
                        companyName = "Affinidi",
                        companyAddress = new
                        {
                            addressLine1 = "Varthur, Gunjur",
                            addressLine2 = "B305, Candeur Landmark, Tower Eiffel",
                            postalCode = "560087",
                            addressRegion = "Karnataka",
                            addressCountry = "India"
                        },
                        hRDetails = new
                        {
                            hRfirstName = "Testing",
                            hRLastName = "HR",
                            hREmail = "hr@affinidi.com",
                            hRDesignation = "Lead HR",
                            hRContactNumber1 = "+911234567789",
                            whenToContact = "9:00-6:00 PM"
                        }
                    },
                    employmentDetails = new
                    {
                        designation = "Testing",
                        employmentStatus = "Fulltime",
                        annualisedSalary = "10000",
                        currency = "INR",
                        tenure = new
                        {
                            fromDate = "05-2022",
                            toDate = "06-2050"
                        },
                        reasonForLeaving = "Resignation",
                        eligibleForRehire = "Yes"
                    },
                    verificationStatus = "Completed",
                    verificationEvidence = new
                    {
                        evidenceName1 = "letter",
                        evidenceURL1 = "http://localhost"
                    },
                    verificationRemarks = "Done"
                }
            };
            if (expiryBatch)
            {
                employmentCredential.metaData = new MetaData { expirationDate = "2027-09-01T00:00:00.000Z" };
            }
            if (revocableBatch)
            {
                employmentCredential.statusListDetails = new List<dynamic> { new { purpose = "REVOCABLE", standard = "RevocationList2020" } };
            }
            data.Add(employmentCredential);

            var issuanceInput = new StartIssuanceInput
            {
                claimMode = ClaimModeEnum.TX_CODE,
                data = data
            };

            var credentialsClient = new CredentialsClient();
            // Console.WriteLine($"[CredentialIssuance] Calling batch IssuanceStart API with {data.Count} credentials");
            // Console.WriteLine($"[CredentialIssuance] Batch Issuance Input: {JsonConvert.SerializeObject(issuanceInput, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })}");

            var issuanceResponse = await credentialsClient.IssuanceStart(issuanceInput);
            // Console.WriteLine($"[CredentialIssuance] Batch Issuance Response: {JsonConvert.SerializeObject(issuanceResponse)}");

            var credentialOfferUri = issuanceResponse?.CredentialOfferUri ?? "";
            var vaultUrl = Environment.GetEnvironmentVariable("PUBLIC_VAULT_URL") ?? "https://vault.affinidi.com";
            var claimUrl = $"{vaultUrl}/claim?credential_offer_uri={Uri.EscapeDataString(credentialOfferUri)}";

            // Console.WriteLine($"[CredentialIssuance] Batch Claim URL: {claimUrl}");

            TempData["IssuanceMessage"] = "Batch Credential issuance process completed. Check logs for details.";
            TempData["ClaimUrl"] = claimUrl;
            TempData["CredentialOfferUri"] = credentialOfferUri;
            TempData["IssuanceId"] = issuanceResponse?.IssuanceId;
            TempData["ExpiresIn"] = issuanceResponse?.ExpiresIn.ToString() ?? "0";
            TempData["TxCode"] = issuanceResponse?.TxCode;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostIssueCustom([FromForm] bool revocableCustom, [FromForm] bool expiryCustom)
        {
            // Console.WriteLine($"[CredentialIssuance] OnPostIssueCustom triggered - Revocable: {revocableCustom}, Expiry: {expiryCustom}");

            // Similar approach can be used here for dynamic data
            TempData["IssuanceMessage"] = "Custom Credential issuance process initiated. Check your backend logs for details.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckCredentialStatus()
        {
            // Console.WriteLine($"[CredentialIssuance] OnPostCheckCredentialStatus triggered");

            // Use the bound property from the form
            var issuanceId = IssuanceId ?? string.Empty;

            // Console.WriteLine($"[CheckCredentialStatus] Retrieved IssuanceId from form: {issuanceId}");

            if (string.IsNullOrEmpty(issuanceId))
            {
                // Console.WriteLine("[CheckCredentialStatus] Missing IssuanceId.");
                TempData["StatusMessage"] = "Cannot check status: IssuanceId is missing.";
                return RedirectToPage();
            }

            try
            {
                var credentialsClient = new CredentialsClient();
                // Console.WriteLine("[CheckCredentialStatus] Calling IssuanceStatus...");
                var statusResponse = await credentialsClient.IssuanceStatus(issuanceId);

                // Console.WriteLine($"[CheckCredentialStatus] Response: {JsonConvert.SerializeObject(statusResponse)}");

                TempData["StatusMessage"] = $"Credential status retrieved successfully. Check logs for details.";
                TempData["StatusResponse"] = JsonConvert.SerializeObject(statusResponse, Formatting.Indented);
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"[CheckCredentialStatus] Exception: {ex.Message}");
                // Console.WriteLine($"[CheckCredentialStatus] Stack trace: {ex.StackTrace}");
                TempData["StatusMessage"] = $"Error checking credential status: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
