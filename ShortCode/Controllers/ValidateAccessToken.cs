using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ShortCode.Controllers;
public class ValidateAccessToken
{
    private readonly string jwksEndpoint;
    private readonly string userPoolId;

    public ValidateAccessToken()
    {
        jwksEndpoint = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_xIsyrVWFs/.well-known/jwks.json";
        userPoolId = "us-east-1_xIsyrVWFs";
    }


    public bool UserAuthentication(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, // Set to true if you want to validate the token issuer
            ValidateAudience = true, // Set to true if you want to validate the token audience
            ValidateLifetime = true, // Set to true if you want to validate the token expiration
            ValidIssuer = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_xIsyrVWFs", // Specify the expected issuer
            ValidAudience = "h5jieimv12nhk7om8d71mkpdo", // Specify the expected audience
            //IssuerSigningKey = new SymmetricSecurityKey("https://cognito-idp.us-east-1.amazonaws.com/us-east-1_xIsyrVWFs/.well-known/jwks.json") // Set the signing key used to validate the token signature
        };

        try
        {
            SecurityToken validatedToken;
            var principal = tokenHandler.ValidateToken(accessToken, validationParameters, out validatedToken);

            // If the token validation succeeds, you can access the token claims and extract relevant information
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return true; // Token is valid
        }
        catch (SecurityTokenException)
        {
            return false; // Token validation failed
        }
    }

    //public async Task<JwtSecurityToken> DecodeCognitoIdToken(string idToken)
    //{
    //    var tokenHandler = new JwtSecurityTokenHandler();

    //    string jwksEndpoint = "https://cognito-idp.{region}.amazonaws.com/{userPoolId}/.well-known/jwks.json";
    //    JsonWebKeySet jwks = await RetrieveSigningKeysAsync(jwksEndpoint);

    //    var keyId = "<key-id>"; // Replace with the appropriate key ID
    //    var signingKey = RetrieveSigningKey(jwks, keyId);

    //    var validationParameters = new TokenValidationParameters
    //    {
    //        ValidateIssuerSigningKey = true,
    //        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("<your signing key>")),
    //        IssuerSigningKey = signingKey,
    //        ValidateIssuer = true,
    //        ValidIssuer = "https://cognito-idp.us-east-1.amazonaws.com/us-east-1_xIsyrVWFs",
    //        ValidateAudience = true,
    //        ValidAudience = "h5jieimv12nhk7om8d71mkpdo",
    //        ValidateLifetime = true
    //    };

    //    // Validate the token
    //    SecurityToken validatedToken;
    //    var claimsPrincipal = tokenHandler.ValidateToken(idToken, validationParameters, out validatedToken);

    //    // Ensure the token is a JWT token
    //    if (!(validatedToken is JwtSecurityToken jwtToken))
    //    {
    //        throw new InvalidOperationException("Invalid ID token.");
    //    }

    //    return jwtToken;
    //}


    //public static async Task<JsonWebKeySet> RetrieveSigningKeysAsync(string jwksEndpoint)
    //{
    //    using (var httpClient = new HttpClient())
    //    {
    //        var response = await httpClient.GetAsync(jwksEndpoint);
    //        response.EnsureSuccessStatusCode();
    //        var jwksJson = await response.Content.ReadAsStringAsync();

    //        var signingKeys = jwks.Keys.Select(key => key.ToSecurityKey());
    //        return await Task.FromResult(signingKeys);
            
    //    }
    //}


    //private SecurityKey RetrieveSigningKey(JsonWebKeySet jwks, string keyId)
    //{
    //    foreach (var key in jwks.Keys)
    //    {
    //        if (key.KeyId == keyId)
    //        {
    //            if (key is X509SecurityKey x509Key)
    //            {
    //                return x509Key.PublicKey;
    //            }
    //            if (key is RsaSecurityKey rsaKey)
    //            {
    //                return rsaKey;
    //            }
    //            if (key is ECDsaSecurityKey ecdsaKey)
    //            {
    //                return ecdsaKey;
    //            }
    //            // Add support for other key types if needed
    //        }
    //    }

    //    throw new ArgumentException($"Signing key with ID '{keyId}' not found.");
    //}
}