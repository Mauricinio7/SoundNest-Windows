using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

namespace SoundNest_Windows_Client.Utilities
{
    public static class JwtHelper
    {
        public static Dictionary<string, object>? DecodeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var handler = new JwtSecurityTokenHandler();

            if (handler.CanReadToken(token))
            {
                var jwtToken = handler.ReadJwtToken(token);
                var claims = new Dictionary<string, object>();
                foreach (var claim in jwtToken.Claims)
                {
                    claims[claim.Type] = claim.Value;
                }

                return claims;
            }

            return null;
        }

        public static string? GetUsernameFromToken(string token)
        {
            var claims = DecodeToken(token);
            return claims?.ContainsKey("username") ?? false ? claims["username"]?.ToString() : null;
        }

        public static string? GetEmailFromToken(string token)
        {
            var claims = DecodeToken(token);
            return claims?.ContainsKey("email") ?? false ? claims["email"]?.ToString() : null;
        }

        public static int? GetUserIdFromToken(string token)
        {
            var claims = DecodeToken(token);
            if (claims?.ContainsKey("id") == true && int.TryParse(claims["id"].ToString(), out var id))
                return id;
            return null;
        }

        public static int? GetRoleFromToken(string token)
        {
            var claims = DecodeToken(token);
            if (claims?.ContainsKey("role") == true && int.TryParse(claims["role"].ToString(), out var role))
                return role;
            return null;
        }

        public static long? GetExpFromToken(string token)
        {
            var claims = DecodeToken(token);
            if (claims?.ContainsKey("exp") == true && long.TryParse(claims["exp"].ToString(), out var exp))
                return exp;
            return null;
        }

        public static long? GetIatFromToken(string token)
        {
            var claims = DecodeToken(token);
            if (claims?.ContainsKey("iat") == true && long.TryParse(claims["iat"].ToString(), out var iat))
                return iat;
            return null;
        }

        public static DateTime? GetExpAsDateTime(string token)
        {
            var exp = GetExpFromToken(token);
            return exp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(exp.Value).DateTime : null;
        }

        public static DateTime? GetIatAsDateTime(string token)
        {
            var iat = GetIatFromToken(token);
            return iat.HasValue ? DateTimeOffset.FromUnixTimeSeconds(iat.Value).DateTime : null;
        }
    }
}
