﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using TeamCloud.Azure;
using TeamCloud.Http;

namespace TeamCloud.Orchestration
{
    public static class FunctionEnvironment
    {
        private static readonly ConcurrentDictionary<string, MethodInfo> FunctionMethodCache = new ConcurrentDictionary<string, MethodInfo>();

        public static MethodInfo GetFunctionMethod(string functionName) => FunctionMethodCache.GetOrAdd(functionName, (key) =>
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic)
                .SelectMany(asm => asm.GetExportedTypes().Where(type => type.IsClass))
                .SelectMany(type => type.GetMethods())
                .FirstOrDefault(method => method.GetCustomAttribute<FunctionNameAttribute>()?.Name.Equals(functionName, StringComparison.Ordinal) ?? false);

        }) ?? throw new ArgumentOutOfRangeException(nameof(functionName), $"Could not find function by name '{functionName}'");

        public static bool TryGetFunctionMethod(string functionName, out MethodInfo functionMethod)
        {
            try
            {
                functionMethod = GetFunctionMethod(functionName);
                return true;
            }
            catch
            {
                functionMethod = null;
                return false;
            }
        }

        public static bool FunctionExists(string functionName)
            => TryGetFunctionMethod(functionName, out var _);

        public static bool IsAzureEnvironment
            => !IsLocalEnvironment;

        public static bool IsLocalEnvironment
            => string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

        public static string HostUrl
        {
            get
            {
                var hostScheme = "http";
                var hostName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");

                if (!hostName.StartsWith("localhost", StringComparison.OrdinalIgnoreCase))
                    hostScheme += "s";

                return $"{hostScheme}://{hostName}";
            }
        }

        private static string GetResourceId(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler()
                .ReadJwtToken(token);

            if (jwtToken.Payload.TryGetValue("xms_mirid", out var value))
                return value.ToString();

            throw new NotSupportedException($"The acquired token does not contain any resource id information.");
        }

        private static async Task<JObject> GetKeyJsonAsync()
        {
            if (IsLocalEnvironment)
                return JObject.Parse("{}");

            var token = await AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var response = await "https://management.azure.com"
                .AppendPathSegment(GetResourceId(token))
                .AppendPathSegment("/host/default/listKeys")
                .SetQueryParam("api-version", "2018-11-01")
                .WithOAuthBearerToken(token)
                .PostAsync(null)
                .ConfigureAwait(false);

            return await response
                .ReadAsJsonAsync()
                .ConfigureAwait(false);
        }

        public static async Task<string> GetAdminKeyAsync()
        {
            var json = await GetKeyJsonAsync()
                .ConfigureAwait(false);

            return json
                .SelectToken("$.masterKey")?
                .ToString();
        }

        public static async Task<string> GetHostKeyAsync(string keyName = default)
        {
            var json = await GetKeyJsonAsync()
                .ConfigureAwait(false);

            return json
                .SelectToken($"$.functionKeys['{keyName ?? "default"}']")?
                .ToString();
        }

        public static async Task<IDictionary<string, string>> GetAppSettingsAsync()
        {
            if (IsLocalEnvironment)
                return new Dictionary<string, string>();

            var token = await AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var response = await "https://management.azure.com"
                .AppendPathSegment(GetResourceId(token))
                .AppendPathSegment("config/appsettings/list")
                .SetQueryParam("api-version", "2019-08-01")
                .WithOAuthBearerToken(token)
                .PostAsync(null)
                .ConfigureAwait(false);

            var json = await response
                .ReadAsJsonAsync()
                .ConfigureAwait(false);

            return json
                .SelectToken("properties")?
                .ToObject<IDictionary<string, string>>()
                ?? new Dictionary<string, string>();
        }

        public static async Task<IDictionary<string, string>> SetAppSettingsAsync(IDictionary<string, string> appConfig)
        {
            if (IsLocalEnvironment)
                return appConfig;

            var token = await AzureSessionService
                .AcquireTokenAsync()
                .ConfigureAwait(false);

            var response = await "https://management.azure.com"
                .AppendPathSegment(GetResourceId(token))
                .AppendPathSegment("config/appsettings")
                .SetQueryParam("api-version", "2019-08-01")
                .WithOAuthBearerToken(token)
                .PutJsonAsync(new { properties = appConfig })
                .ConfigureAwait(false);

            var json = await response
                .ReadAsJsonAsync()
                .ConfigureAwait(false);

            return json
                .SelectToken("properties")?
                .ToObject<IDictionary<string, string>>()
                ?? new Dictionary<string, string>();
        }
    }
}
