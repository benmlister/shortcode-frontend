using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

public class TokenValidator
{
    private readonly string jwksEndpoint;
    private readonly string userPoolId;
    private readonly string region;
    private readonly string clientId;

    public TokenValidator()
    {
        region = "us-east-1";
        userPoolId = "us-east-1_xIsyrVWFs";
        jwksEndpoint = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/jwks.json";
        clientId = "h5jieimv12nhk7om8d71mkpdo";
    }

    public async Task<ClaimsPrincipal> ValidateAndDecodeIdTokenAsync(string idToken)
    {
        // Retrieve the JSON Web Key Set (JWKS) from the JWKS endpoint
        JsonWebKeySet jwks = await RetrieveJwksAsync();

        // Extract the Key ID (kid) from the ID token
        string keyId = ExtractKeyIdFromIdToken(idToken);

        // Retrieve the signing key from the JWKS based on the Key ID
        SecurityKey signingKey = RetrieveSigningKey(jwks, keyId);

        // Validate and decode the ID token using the signing key
        ClaimsPrincipal claimsPrincipal = ValidateAndDecodeIdToken(idToken, signingKey);

        return claimsPrincipal;
    }

    private async Task<JsonWebKeySet> RetrieveJwksAsync()
    {
        using var httpClient = new HttpClient();
        string jwksJson = await httpClient.GetStringAsync(jwksEndpoint);
        return new JsonWebKeySet(jwksJson);
    }

    private string ExtractKeyIdFromIdToken(string idToken)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken token = tokenHandler.ReadJwtToken(idToken);
        return token.Header.Kid;
    }

    private SecurityKey RetrieveSigningKey(JsonWebKeySet jwks, string keyId)
    {
        var signingKey = jwks.Keys.FirstOrDefault(k => k.Kid == keyId);
        return signingKey;
    }

    private ClaimsPrincipal ValidateAndDecodeIdToken(string idToken, SecurityKey signingKey)
    {
        TokenValidationParameters validationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudiences = new List<string> { clientId },
            ValidateIssuer = true,
            ValidIssuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}",
            IssuerSigningKey = signingKey,
            ValidateIssuerSigningKey = true,
            RequireExpirationTime = true,
            ValidateLifetime = true
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            SecurityToken validatedToken;
            ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(idToken, validationParameters, out validatedToken);

            // Perform custom audience validation
            if (validatedToken is JwtSecurityToken jwtSecurityToken)
            {
                // Get the audience claim
                var audienceClaim = jwtSecurityToken.Claims.FirstOrDefault(c => c.Type == "aud");

                // Check if the expected clientId is present in the audience claim
                if (audienceClaim?.Value == clientId)
                {
                    return claimsPrincipal;
                }
            }

            throw new Exception("ID token audience validation failed.");
        }
        catch (Exception ex)
        {
            // Token validation failed
            throw new Exception("ID token validation failed.", ex);
        }
    }


}
