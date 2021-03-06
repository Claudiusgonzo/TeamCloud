/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Internal.Data.Core;
using TeamCloud.Model.Data.Core;
using TeamCloud.Serialization;
using TeamCloud.Model.Data;

namespace TeamCloud.Model.Internal.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class TeamCloudInstanceDocument : ContainerDocument, ITeamCloudInstance, IPopulate<Model.Data.TeamCloudInstance>
    {
        [PartitionKey]
        public override string Id { get; set; }

        public string Version { get; set; }

        public AzureResourceGroup ResourceGroup { get; set; }

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();
    }
}
