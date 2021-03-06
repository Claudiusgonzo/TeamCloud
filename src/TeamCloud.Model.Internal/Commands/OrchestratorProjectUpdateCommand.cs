﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Commands;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProjectUpdateCommand : OrchestratorCommand<ProjectDocument, OrchestratorProjectUpdateCommandResult, ProviderProjectUpdateCommand, Model.Data.Project>
    {
        public OrchestratorProjectUpdateCommand(Uri baseApi, UserDocument user, ProjectDocument payload) : base(baseApi, user, payload) { }
    }
}
