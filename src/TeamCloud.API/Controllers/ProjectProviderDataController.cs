/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TeamCloud.API.Data.Results;
using TeamCloud.API.Services;
using TeamCloud.Data;
using TeamCloud.Model.Data;
using TeamCloud.Model.Data.Core;
using TeamCloud.Model.Internal.Data;
using TeamCloud.Model.Validation.Data;

namespace TeamCloud.API.Controllers
{
    [ApiController]
    [Route("api/projects/{projectId:guid}/providers/{providerId:providerId}/data")]
    [Produces("application/json")]
    public class ProjectProviderDataController : ControllerBase
    {
        readonly Orchestrator orchestrator;
        readonly IProjectsRepository projectsRepository;
        readonly IProvidersRepository providersRepository;
        readonly IProviderDataRepository providerDataRepository;

        public ProjectProviderDataController(Orchestrator orchestrator, IProjectsRepository projectsRepository, IProvidersRepository providersRepository, IProviderDataRepository providerDataRepository)
        {
            this.orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
            this.projectsRepository = projectsRepository ?? throw new ArgumentNullException(nameof(projectsRepository));
            this.providersRepository = providersRepository ?? throw new ArgumentNullException(nameof(providersRepository));
            this.providerDataRepository = providerDataRepository ?? throw new ArgumentNullException(nameof(providerDataRepository));
        }

        public string ProjectId
            => RouteData.Values.GetValueOrDefault(nameof(ProjectId), StringComparison.OrdinalIgnoreCase)?.ToString();

        public string ProviderId
            => RouteData.Values.GetValueOrDefault(nameof(ProviderId), StringComparison.OrdinalIgnoreCase)?.ToString();


        [HttpGet]
        [Authorize(Policy = "providerDataRead")]
        [SwaggerOperation(OperationId = "GetProjectProviderData", Summary = "Gets the ProviderData items for a Project.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns ProviderData", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided id was not found, or a Project with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromQuery] bool includeShared)
        {
            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the identifier '{ProjectId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found on the Project '{ProjectId}'")
                    .ActionResult();

            var data = await providerDataRepository
                .ListAsync(provider.Id, project.Id, includeShared)
                .ToListAsync()
                .ConfigureAwait(false);

            var returnData = data.Select(d => d.PopulateExternalModel()).ToList();

            return DataResult<List<ProviderData>>
                .Ok(returnData)
                .ActionResult();
        }


        [HttpGet("{providerDataId:guid}")]
        [Authorize(Policy = "providerDataRead")]
        [SwaggerOperation(OperationId = "GetProjectProviderDataById", Summary = "Gets a ProviderData for a Project by ID.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Returns ProviderData", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProviderData with the provided id was not found, or a Project with the provided identifier was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Get([FromRoute] string providerDataId)
        {
            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the identifier '{ProjectId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found on the Project '{ProjectId}'")
                    .ActionResult();

            var providerData = await providerDataRepository
                .GetAsync(providerDataId)
                .ConfigureAwait(false);

            if (providerData is null)
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found")
                    .ActionResult();

            var returnData = providerData.PopulateExternalModel();

            return DataResult<ProviderData>
                .Ok(returnData)
                .ActionResult();
        }


        [HttpPost]
        [Authorize(Policy = "providerDataWrite")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "CreateProjectProviderData", Summary = "Creates a new ProviderData")]
        [SwaggerResponse(StatusCodes.Status201Created, "The ProviderData was created.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Provider with the provided provider ID was not found, or a Project with the id specified was not found.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "A Project User already exists with the email address provided in the request body.", typeof(ErrorResult))]
        public async Task<IActionResult> Post([FromBody] ProviderData providerData)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var validation = new ProviderDataValidator().Validate(providerData);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the identifier '{ProjectId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found on the Project '{ProjectId}'")
                    .ActionResult();

            var newProviderData = new ProviderDataDocument
            {
                ProviderId = provider.Id,
                Scope = ProviderDataScope.Project,
                ProjectId = project.Id,
            };

            newProviderData.PopulateFromExternalModel(providerData);

            var addResult = await orchestrator
                .AddAsync(newProviderData)
                .ConfigureAwait(false);

            var baseUrl = HttpContext.GetApplicationBaseUrl();
            var location = new Uri(baseUrl, $"api/projects/{project.Id}/providers/{provider.Id}/data/{addResult.Id}").ToString();

            var returnAddResult = addResult.PopulateExternalModel();

            return DataResult<ProviderData>
                .Created(returnAddResult, location)
                .ActionResult();
        }


        [HttpPut]
        [Authorize(Policy = "providerDataWrite")]
        [Consumes("application/json")]
        [SwaggerOperation(OperationId = "UpdateProjectProviderData", Summary = "Updates an existing ProviderData.")]
        [SwaggerResponse(StatusCodes.Status200OK, "The ProviderData was updated.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A Project with the provided projectId was not found, or a User with the ID provided in the request body was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Put([FromBody] ProviderData providerData)
        {
            if (providerData is null)
                throw new ArgumentNullException(nameof(providerData));

            var validation = new ProviderDataValidator().Validate(providerData);

            if (!validation.IsValid)
                return ErrorResult
                    .BadRequest(validation)
                    .ActionResult();

            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the identifier '{ProjectId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            if (!project.Type.Providers.Any(p => p.Id.Equals(provider.Id, StringComparison.OrdinalIgnoreCase)))
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found on the Project '{ProjectId}'")
                    .ActionResult();

            var oldProviderData = await providerDataRepository
                .GetAsync(providerData.Id)
                .ConfigureAwait(false);

            if (oldProviderData is null)
                return ErrorResult
                    .NotFound($"The Provider Data '{providerData.Id}' could not be found..")
                    .ActionResult();

            var newProviderData = new ProviderDataDocument
            {
                ProviderId = provider.Id,
                Scope = ProviderDataScope.Project,
                ProjectId = project.Id,
            };

            newProviderData.PopulateFromExternalModel(providerData);

            var updateResult = await orchestrator
                .UpdateAsync(newProviderData)
                .ConfigureAwait(false);

            var returnUpdateResult = updateResult.PopulateExternalModel();

            return DataResult<ProviderData>
                .Ok(returnUpdateResult)
                .ActionResult();
        }


        [HttpDelete("{providerDataId:guid}")]
        [Authorize(Policy = "providerDataWrite")]
        [SwaggerOperation(OperationId = "DeleteProjectProviderData", Summary = "Deletes a ProviderData.")]
        [SwaggerResponse(StatusCodes.Status204NoContent, "The ProviderData was deleted.", typeof(DataResult<ProviderData>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "A validation error occured.", typeof(ErrorResult))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "A ProviderData with the providerDataId provided was not found.", typeof(ErrorResult))]
        public async Task<IActionResult> Delete([FromRoute] string providerDataId)
        {
            var provider = await providersRepository
                .GetAsync(ProviderId)
                .ConfigureAwait(false);

            if (provider is null)
                return ErrorResult
                    .NotFound($"A Provider with the ID '{ProviderId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            var existingProviderData = await providerDataRepository
                .GetAsync(providerDataId)
                .ConfigureAwait(false);

            if (existingProviderData is null)
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found")
                    .ActionResult();

            if (existingProviderData.Scope == ProviderDataScope.System)
                return ErrorResult
                    .BadRequest("The specified Provider Data item is not scoped to a project use the system api to delete.", ResultErrorCode.ValidationError)
                    .ActionResult();

            var project = await projectsRepository
                .GetAsync(ProjectId)
                .ConfigureAwait(false);

            if (project is null)
                return ErrorResult
                    .NotFound($"A Project with the identifier '{ProjectId}' could not be found in this TeamCloud Instance")
                    .ActionResult();

            if (!existingProviderData.ProjectId.Equals(project.Id, StringComparison.OrdinalIgnoreCase))
                return ErrorResult
                    .NotFound($"A Provider Data item with the ID '{providerDataId}' could not be found for project '{ProjectId}'")
                    .ActionResult();

            _ = await orchestrator
                .DeleteAsync(existingProviderData)
                .ConfigureAwait(false);

            return DataResult<ProviderData>
                .NoContent()
                .ActionResult();
        }
    }
}
