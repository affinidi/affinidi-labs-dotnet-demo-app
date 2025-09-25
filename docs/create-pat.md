# 🚀 Setup Personal Access Token (PAT)

<div align="center">
  <img src="./images/Affinidi%20Stacked_FC_RGB.jpg" alt="Affinidi PAT" width="180"/>
</div>



> [!IMPORTANT]
> This guide is for learning, experimentation, and prototyping only.
> **Do not use this configuration as-is in production environments.**
> Please review, test, and secure your implementation before deploying to production.



## 📖 Table of Contents
- [Overview](#overview)
- [Install Affinidi CLI](#install-affinidi-cli)
- [Create Personal Access Token (PAT)](#create-personal-access-token-pat)
- [Update Environment Variables](#update-environment-variables)
- [Further Reading](#further-reading)
- [Disclaimer](#disclaimer)



## 🧭 Overview

A **Personal Access Token (PAT)** acts as a machine user for Affinidi services.
It is required for authentication and automation within your application and can access multiple projects once granted by the user.

Learn more: [Personal Access Token Documentation](https://docs.affinidi.com/dev-tools/affinidi-cli/manage-token/#how-does-pat-authentication-works)



## 🛠️ Install Affinidi CLI

1. **Install Affinidi CLI using NPM:**
   ```sh
   npm install -g @affinidi/cli
   ```

2. **Verify installation:**
   ```sh
   affinidi --version
   ```
   > Note: Affinidi CLI requires Node.js version 18 or above.



## 🔑 Create Personal Access Token (PAT)

1. **Log in to Affinidi CLI:**
   ```sh
   affinidi start
   ```

2. **Create a token:**
   ```sh
   affinidi token create-token
   ```
   Follow the prompts:
   ```
   ? Enter the value for name BSCPAT
   ? Generate a new keypair for the token? yes
   ? Enter a passphrase to encrypt the private key. Leave it empty for no encryption ******
   ? Add token to active project and grant permissions? yes
   ? Enter the allowed resources, separated by spaces. Use * to allow access to all project resources *
   ? Enter the allowed actions, separated by spaces. Use * to allow all actions *
   ```

3. **Sample response:**
   ```json
   {
     "id": "**********",
     "ari": "ari:iam:::token/**********",
     "ownerAri": "ari:iam:::user/**********",
     "name": "workshopPAT",
     "scopes": [
       "openid",
       "offline_access"
     ],
     "authenticationMethod": {
       "type": "PRIVATE_KEY",
       "signingAlgorithm": "RS256",
       "publicKeyInfo": {
         "jwks": {
           "keys": [
             {
               "use": "sig",
               "kty": "RSA",
               "kid": "**********",
               "alg": "RS256",
               "n": "**********",
               "e": "AQAB"
             }
           ]
         }
       }
     }
   }
   ```

   Save your `projectId`, `tokenId`, `privateKey`, and `passphrase` securely.
   > ⚠️ **Warning:** You will not be able to view the private key and passphrase again.

   For more details, run:
   ```sh
   affinidi token create-token --help
   ```



## ⚙️ Update Environment Variables

After creating your PAT, update your `.env` file with the following variables:

```env
PROJECT_ID=""
KEY_ID=""         # Optional. Required if a different key_id is used; otherwise, TOKEN_ID=KEY_ID
TOKEN_ID=""
PASSPHRASE=""     # Optional. Required if private key is encrypted
PRIVATE_KEY=""
```

**Instructions:**
- Fill in the values using the details from your PAT creation response.
- Ensure your `.env` file is kept secure and never committed to source control.

For Token Generation, refer to [AuthProvider.cs](/util/AuthProvider.cs) and [ProjectScopedToken.cs](/util/ProjectScopedToken.cs)

## 📚 Further Reading

- [Affinidi CLI Token Management](https://docs.affinidi.com/dev-tools/affinidi-cli/manage-token)
- [Affinidi Documentation](https://docs.affinidi.com/docs/)



## _Disclaimer_

_This documentation is provided for informational purposes only and is not a legal document. For legal terms, conditions, and limitations, please refer to the official Affinidi documentation and your service agreement._
