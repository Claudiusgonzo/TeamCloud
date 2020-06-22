/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Data.Core;
using TeamCloud.Serialization;

namespace TeamCloud.Model.Internal.Data
{
    [JsonObject(NamingStrategyType = typeof(TeamCloudNamingStrategy))]
    public sealed class ProjectType : ContainerDocument, IEquatable<ProjectType>, ITags, IProperties
    {
        [PartitionKey]
        public string Tenant { get; set; }

        public bool Default { get; set; }

        public string Region { get; set; }

        public IList<Guid> Subscriptions { get; set; } = new List<Guid>();

        public int SubscriptionCapacity { get; set; } = 10;

        public string ResourceGroupNamePrefix { get; set; }

        public IList<ProviderReference> Providers { get; set; } = new List<ProviderReference>();

        public IDictionary<string, string> Tags { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();


        public bool Equals(ProjectType other)
            => Id.Equals(other?.Id, StringComparison.Ordinal);

        public override bool Equals(object obj)
            => base.Equals(obj) || Equals(obj as ProjectType);

        public override int GetHashCode()
            => Id?.GetHashCode(StringComparison.Ordinal) ?? base.GetHashCode();
    }
}
