# Affinidi APIs Reference

This document lists all Affinidi APIs used in this .NET reference application.

## Table of Contents
- [Authentication Flow](#authentication-flow)
- [IAM APIs](#1-iam-identity--access-management-apis)
- [CIS APIs](#2-cis-credential-issuance-service-apis)
- [AIS APIs](#3-ais-affinidi-iota-service--data-sharing-apis)
- [VER APIs](#4-ver-verifier-service-apis)
- [OAuth2/OIDC](#5-oauth2oidc-token-endpoint)
- [Summary](#summary)

---

## Authentication Flow

```
1. Sign JWT assertion with private key
   ↓
2. Exchange JWT for user access token (OAuth2 endpoint)
   ↓
3. Exchange user access token for project-scoped token (IAM API)
   ↓
4. Use project-scoped token for all other Affinidi APIs
```

---

## 1. IAM (Identity & Access Management) APIs

### Create Project-Scoped Token

**Endpoint**: `{API_GATEWAY_URL}/iam/v1/sts/create-project-scoped-token`

**Method**: `POST`

**Purpose**: Create project-scoped access tokens for authenticating to other Affinidi services

**Authentication**: Bearer token (user access token)

**Request Body**:
```json
{
  "projectId": "string"
}
```

**Response**:
```json
{
  "accessToken": "string"
}
```

**Implementation**: `util/ProjectScopedToken.cs` - `FetchProjectScopedTokenAsync()`

---

## 2. CIS (Credential Issuance Service) APIs

### a) Start Issuance

**Endpoint**: `{API_GATEWAY_URL}/cis/v1/{projectId}/issuance/start`

**Method**: `POST`

**Purpose**: Initiate credential issuance flow

**Authentication**: Bearer token (project-scoped token)

**Request Body**:
```json
{
  "claimMode": "NORMAL" | "TX_CODE" | "FIXED_HOLDER",
  "data": [
    {
      "credentialTypeId": "string",
      "credentialData": {},
      "metaData": {},
      "statusListDetails": []
    }
  ]
}
```

**Response**:
```json
{
  "credentialOfferUri": "string",
  "issuanceId": "string",
  "expiresIn": 0,
  "txCode": "string"
}
```

**Implementation**: `util/CredentialsClient.cs` - `IssuanceStart()`

---

### b) Get Issuance Status

**Endpoint**: `{API_GATEWAY_URL}/cis/v1/{projectId}/configurations/{configurationId}/issuances/{issuanceId}/credentials`

**Method**: `GET`

**Purpose**: Check the status of an issuance and retrieve issued credentials

**Authentication**: Bearer token (project-scoped token)

**Response**:
```json
{
  "credential": {},
  "credentials": []
}
```

**Implementation**: `util/CredentialsClient.cs` - `IssuanceStatus()`

---

### c) List Issuance Records

**Endpoint**: `{API_GATEWAY_URL}/cis/v1/{projectId}/configurations/{configurationId}/issuance/issuance-data-records`

**Method**: `GET`

**Purpose**: Retrieve all issuance records for revocation management

**Authentication**: Bearer token (project-scoped token)

**Response**:
```json
{
  "flowData": []
}
```

**Implementation**: `util/RevokeClient.cs` - `ListIssuanceRecordsAsync()`

---

### d) Revoke Credential (Change Status)

**Endpoint**: `{API_GATEWAY_URL}/cis/v1/{projectId}/configurations/{configurationId}/issuance/change-status`

**Method**: `POST`

**Purpose**: Revoke or change the status of issued credentials

**Authentication**: Bearer token (project-scoped token)

**Request Body**:
```json
{
  "changeReason": "string",
  "issuanceRecordId": "string"
}
```

**Response**:
```json
{
  "id": "string",
  "createdAt": "datetime",
  "modifiedAt": "datetime",
  "projectId": "string",
  "flowId": "string",
  "credentialTypeId": "string",
  "jsonLdContextUrl": "string",
  "jsonSchemaUrl": "string",
  "configurationId": "string",
  "walletId": "string",
  "statusListsDetails": [
    {
      "standard": "string",
      "statusListIndex": "string",
      "statusListId": "string",
      "isActive": true,
      "statusActivationReason": "string",
      "statusListPurpose": "string"
    }
  ]
}
```

**Implementation**: `util/RevokeClient.cs` - `RevokeCredentialAsync()`

---

## 3. AIS (Affinidi Iota Service / Data Sharing) APIs

### a) Initiate Data Sharing Request

**Endpoint**: `{API_GATEWAY_URL}/ais/v1/initiate-data-sharing-request`

**Method**: `POST`

**Purpose**: Start a data sharing flow to request credentials from users

**Authentication**: Bearer token (project-scoped token)

**Request Body**:
```json
{
  "queryId": "string",
  "correlationId": "string",
  "tokenMaxAge": 0,
  "nonce": "string",
  "redirectUri": "string",
  "configurationId": "string",
  "mode": "string"
}
```

**Response**:
```json
{
  "jwt": "string",
  "correlationId": "string",
  "transactionId": "string"
}
```

**Implementation**: `util/IotaClient.cs` - `Start()`

---

### b) Fetch Iota Response

**Endpoint**: `{API_GATEWAY_URL}/ais/v1/fetch-iota-response`

**Method**: `POST`

**Purpose**: Retrieve the verifiable presentation (VP) token after user shares data

**Authentication**: Bearer token (project-scoped token)

**Request Body**:
```json
{
  "correlationId": "string",
  "transactionId": "string",
  "responseCode": "string",
  "configurationId": "string"
}
```

**Response**:
```json
{
  "correlationId": "string",
  "presentationSubmission": "string",
  "vpToken": "string"
}
```

**Implementation**: `util/IotaClient.cs` - `Complete()`

---

## 4. VER (Verifier Service) APIs

### a) Verify Credentials

**Endpoint**: `{API_GATEWAY_URL}/ver/v1/verifier/verify-vcs`

**Method**: `POST`

**Purpose**: Verify one or more verifiable credentials

**Authentication**: Bearer token (project-scoped token)

**Request Body**:
```json
{
  "verifiableCredentials": []
}
```

**Response**:
```json
{
  "isValid": true,
  "errors": []
}
```

**Implementation**: `util/VerifierClient.cs` - `VerifyCredentialsAsync()`

---

### b) Verify Presentation

**Endpoint**: `{API_GATEWAY_URL}/ver/v1/verifier/verify-vp`

**Method**: `POST`

**Purpose**: Verify a complete verifiable presentation

**Authentication**: Bearer token (project-scoped token)

**Request Body**:
```json
{
  "verifiablePresentation": {}
}
```

**Response**:
```json
{
  "isValid": true,
  "errors": []
}
```

**Implementation**: `util/VerifierClient.cs` - `VerifyPresentationAsync()`

---

## 5. OAuth2/OIDC Token Endpoint

**Endpoint**: `{TOKEN_ENDPOINT}` (from environment variable)

**Method**: `POST`

**Purpose**: Obtain user access tokens using client credentials flow

**Grant Type**: `client_credentials`

**Authentication**: JWT assertion (signed with private key)

**Request Body** (form-urlencoded):
```
grant_type=client_credentials
scope=openid
client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer
client_assertion={signed_jwt}
client_id={tokenId}
```

**Response**:
```json
{
  "access_token": "string",
  "token_type": "Bearer",
  "expires_in": 0
}
```

**Implementation**: `util/ProjectScopedToken.cs` - `GetUserAccessTokenAsync()`

---

## Summary

| Service | Number of APIs | Purpose |
|---------|----------------|---------|
| **IAM** | 1 | Token management |
| **CIS** | 4 | Credential issuance & revocation |
| **AIS (Iota)** | 2 | Data sharing requests |
| **VER** | 2 | Credential verification |
| **OAuth2** | 1 | Initial authentication |
| **Total** | **10 APIs** | Complete credential lifecycle |

---

## Environment Variables Required

All APIs use the following environment variables:

- `API_GATEWAY_URL` - Base URL for all Affinidi APIs
- `TOKEN_ENDPOINT` - OAuth2 token endpoint URL
- `PROJECT_ID` - Your Affinidi project identifier
- `TOKEN_ID` - Personal Access Token ID
- `KEY_ID` - Key identifier (defaults to TOKEN_ID if not provided)
- `PRIVATE_KEY` - Private key for signing JWTs (PEM format)
- `PASSPHRASE` - Passphrase for encrypted private key (optional)
- `CONFIGURATION_ID` - Configuration ID for CIS operations

---

## Client Classes

| Client Class | File | Purpose |
|--------------|------|---------|
| `AuthProvider` | `util/AuthProvider.cs` | Manages authentication and token lifecycle |
| `ProjectScopedToken` | `util/ProjectScopedToken.cs` | Handles JWT signing and token exchange |
| `CredentialsClient` | `util/CredentialsClient.cs` | Credential issuance operations |
| `IotaClient` | `util/IotaClient.cs` | Data sharing and presentation requests |
| `VerifierClient` | `util/VerifierClient.cs` | Credential and presentation verification |
| `RevokeClient` | `util/RevokeClient.cs` | Credential revocation management |

---

## Notes

- All API requests (except OAuth2 token endpoint) require a project-scoped token in the Authorization header
- Project-scoped tokens are automatically managed and refreshed by the `AuthProvider` class
- JWT assertions are signed using RS256 algorithm
- Tokens have a 5-minute expiration and are automatically refreshed when needed
