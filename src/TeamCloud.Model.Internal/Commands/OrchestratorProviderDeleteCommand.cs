﻿/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using TeamCloud.Model.Internal.Data;

namespace TeamCloud.Model.Internal.Commands
{
    public class OrchestratorProviderDeleteCommand : OrchestratorCommand<ProviderDocument, OrchestratorProviderDeleteCommandResult>
    {
        public OrchestratorProviderDeleteCommand(Uri baseApi, UserDocument user, ProviderDocument payload) : base(baseApi, user, payload) { }
    }
}
