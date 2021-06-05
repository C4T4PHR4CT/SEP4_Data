﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using SEP4_Data.Model.Exception;

namespace SEP4_Data.Data
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IPersistenceService _persistence;
        private readonly IConfigService _config;
        private readonly ILogService _log;

        public JwtMiddleware(RequestDelegate next, IPersistenceService persistence, IConfigService config, ILogService log)
        {
            _next = next;
            _persistence = persistence;
            _config = config;
            _log = log;
        }

        public async Task Invoke(HttpContext context)
        {
            var temp = context.Request.Headers["Authorization"];
            foreach (string token in temp)
                if (AttachBasic(context, token))
                    break;
            foreach (string token in temp)
                if (AttachBearer(context, token))
                    break;
            await _next(context);
        }

        private bool AttachBearer(HttpContext context, string token)
        {
            try {
                var temp = token.Split(" ");
                if (temp[0] == "Basic")
                    return false;
                if (temp[0] != "Bearer" || temp.Length != 2)
                    throw new JwtException("malformed bearer authorization header");
                token = temp[1];
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = _config.JwtKey;
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(30)
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken) validatedToken;
                int userKey = int.Parse(jwtToken.Claims.First(x => x.Type == "key").Value);

                context.Items["User"] = _persistence.GetUserByKey(userKey);
                return true;
            } catch (Exception e) {
                _log.Log(e.ToString());
                return false;
            }
        }
        
        private bool AttachBasic(HttpContext context, string token)
        {
            try {
                var temp = token.Split(" ");
                if (temp[0] == "Bearer")
                    return false;
                if (temp[0] != "Basic" || temp.Length != 2)
                    throw new JwtException("malformed basic authorization header");
                temp = Encoding.UTF8.GetString(Convert.FromBase64String(temp[1])).Split(":");
                if (temp.Length != 2)
                    throw new JwtException("malformed basic authorization header");
                temp[1] = Convert.ToBase64String(KeyDerivation.Pbkdf2(temp[1], _config.Salt, KeyDerivationPrf.HMACSHA1, 1000, 256 / 8));
                if (_persistence.CheckUserPassword(temp[0], temp[1]))
                    context.Items["User"] = _persistence.GetUserByName(temp[0]);
                else
                    throw new JwtException("invalid basic authorization credentials");
                return true;
            } catch (Exception e) {
                _log.Log(e.ToString());
                return false;
            }
        }
    }
}