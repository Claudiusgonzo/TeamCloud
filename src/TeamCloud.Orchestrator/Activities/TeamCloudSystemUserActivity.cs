/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using TeamCloud.Azure;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Orchestrator.Activities
{
    public class TeamCloudSystemUserActivity
    {
        private readonly IAzureSessionService azureSessionService;

        public TeamCloudSystemUserActivity(IAzureSessionService azureSessionService)
        {
            this.azureSessionService = azureSessionService ?? throw new System.ArgumentNullException(nameof(azureSessionService));
        }

        [FunctionName(nameof(TeamCloudSystemUserActivity))]
        public async Task<UserDocument> RunActivity(
            [ActivityTrigger] IDurableActivityContext functionContext)
        {
            if (functionContext is null)
                throw new System.ArgumentNullException(nameof(functionContext));

            var systemIdentity = await azureSessionService
                .GetIdentityAsync()
                .ConfigureAwait(false);

            return new UserDocument()
            {
                Id = systemIdentity.ObjectId.ToString(),
                Role = TeamCloudUserRole.Admin,
                UserType = UserType.System
            };
        }
    }
}
