using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DeveloperPath.Domain.Shared.ClientModels;
using DeveloperPath.Application.CQRS.Modules.Commands.CreateModule;
using DeveloperPath.Application.CQRS.Modules.Commands.DeleteModule;
using DeveloperPath.Application.CQRS.Modules.Commands.UpdateModule;
using DeveloperPath.Application.CQRS.Modules.Queries.GetModules;
using DeveloperPath.WebApi.Extensions;
using DeveloperPath.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace DeveloperPath.WebApi.Controllers
{
  [Route("api/paths/{pathId}/modules")]
  public class ModulesController : ApiController
  {
    public ModulesController(IMediator mediator)
    {
      _mediator = mediator;
    }

    /// <summary>
    /// Get all available modules
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <param name="requestParams">Request parameters</param>
    /// <param name="ct"></param>
    /// <returns>A collection of modules with summary information</returns>
    /// <response code="200">Returns a list of modules in the path</response>
    /// <response code="404">Path not found</response>
    [HttpGet]
    [HttpHead]
    public async Task<ActionResult<IEnumerable<Module>>> Get(int pathId, [FromQuery] RequestParams requestParams = null, CancellationToken ct = default)
    {
      //TODO: consider adding default page size and show 1st page instead of all
      return requestParams is not null && requestParams.UsePaging()
        ? await GetPage(pathId, requestParams, ct)
        : await GetAll(pathId, ct);
    }

    /// <summary>
    /// Get module information by its Id
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <param name="moduleId">An id of the module</param>
    /// <param name="ct"></param>
    /// <returns>Information about the module</returns>
    /// <response code="200">Returns requested module</response>
    /// <response code="404">Module not found</response>
    [HttpGet("{moduleId}", Name = "GetModule")]
    [HttpHead("{moduleId}")]
    public async Task<ActionResult<Module>> Get(int pathId, int moduleId, CancellationToken ct = default)
    {
      Module model = await Mediator.Send(new GetModuleQuery { PathId = pathId, Id = moduleId }, ct);

      return Ok(model);
    }

    ///// <summary>
    ///// Get module details information by its Id
    ///// </summary>
    ///// <param name="pathId">An id of the path</param>
    ///// <param name="moduleId">An id of the module</param>
    ///// <returns>Detailed information of the module with themes included</returns>
    //[Route("api/paths/{pathId}/moduledetails")]
    //[HttpGet("{moduleId}", Name = "GetModuleDetails")]
    //[HttpHead("{moduleId}")]
    //public async Task<ActionResult<ModuleViewModel>> GetDetails(int pathId, int moduleId)
    //{
    //  ModuleViewModel model = await Mediator.Send(new GetModuleDetailsQuery { Id = moduleId });

    //  return Ok(model);
    //}

    /// <summary>
    /// Create a module
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <param name="command">Module object</param>
    /// <returns>Created module</returns>
    /// <response code="201">Module created successfully</response>
    /// <response code="404">Path not found</response>
    /// <response code="406">Not acceptable entity provided</response>
    /// <response code="415">Unsupported media type provided</response>
    /// <response code="422">Unprocessible entity provided</response>
    [Authorize]
    [HttpPost]
    [Consumes("application/json")]
    public async Task<ActionResult<Module>> Create(int pathId,
      [FromBody] CreateModule command)
    {
      if (pathId != command.PathId)
        return BadRequest();

      Module model = await Mediator.Send(command);

      return CreatedAtRoute("GetModule", new { pathId = command.PathId, moduleId = model.Id }, model);
    }

    /// <summary>
    /// Update the module with given Id
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <param name="moduleId">An id of the module</param>
    /// <param name="command">Updated module object</param>
    /// <returns>Updated module</returns>
    /// <response code="200">Module updated successfully</response>
    /// <response code="406">Not acceptable entity provided</response>
    /// <response code="415">Unsupported media type provided</response>
    /// <response code="422">Unprocessible entity provided</response>
    [Authorize]
    [HttpPut("{moduleId}")]
    [Consumes("application/json")]
    public async Task<ActionResult<Module>> Update(int pathId, int moduleId,
      [FromBody] UpdateModule command)
    {
      if (moduleId != command.Id)
        return BadRequest();

      return Ok(await Mediator.Send(command));
    }

    // TODO: add PATCH

    /// <summary>
    /// Delete the module with given Id from path with given Id
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <param name="moduleId">An id of the module</param>
    /// <returns></returns>
    /// <response code="204">Module deleted successfully</response>
    [Authorize]
    [HttpDelete("{moduleId}")]
    public async Task<ActionResult> Delete(int pathId, int moduleId)
    {
      await Mediator.Send(new DeleteModule { PathId = pathId, Id = moduleId });

      return NoContent();
    }

    private async Task<ActionResult<IEnumerable<Module>>> GetAll(int pathId, CancellationToken ct = default)
    {
      IEnumerable<Module> model = await Mediator.Send(new GetModuleListQuery { PathId = pathId }, ct);
      return Ok(model);
    }

    private async Task<ActionResult<IEnumerable<Module>>> GetPage(int pathId, RequestParams filter, CancellationToken ct = default)
    {
      var (paginationData, result) = await Mediator.Send(
          new GetModuleListQueryPaging()
          {
            PathId = pathId,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
          }, ct);

      Response?.Headers?.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationData));
      return Ok(result);
    }
  }
}