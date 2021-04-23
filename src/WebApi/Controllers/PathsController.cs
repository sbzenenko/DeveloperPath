﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Shared.Dtos.Models;
using DeveloperPath.Application.CQRS.Paths.Commands.CreatePath;
using DeveloperPath.Application.CQRS.Paths.Commands.DeletePath;
using DeveloperPath.Application.CQRS.Paths.Commands.UpdatePath;
using DeveloperPath.Application.CQRS.Paths.Queries.GetPaths;
using DeveloperPath.WebApi.Extensions;
using DeveloperPath.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DeveloperPath.WebApi.Controllers
{
    //[Authorize]
    [Route("api/paths")]
  public class PathsController : ApiController
  {
    public PathsController(IMediator mediator)
    {
      _mediator = mediator;
    }

    /// <summary>
    /// Get all available paths
    /// </summary>
    /// <returns>A collection of paths with summary information</returns>
    [HttpGet]
    [HttpHead]
    public async Task<ActionResult<IEnumerable<PathDto>>> Get([FromQuery] RequestParams requestParams = null)
    {
      //TODO: consider adding default page size and show 1st page instead of all
      return requestParams is not null && requestParams.UsePaging()
        ? await GetPage(requestParams)
        : await GetAll();
    }

    /// <summary>
    /// Get path information by its Id
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <returns>Information of the path with modules included</returns>
    [HttpGet("{pathId}", Name = "GetPath")]
    [HttpHead("{pathId}")]
    public async Task<ActionResult<PathDto>> Get(int pathId)
    {
      PathDto model = await Mediator.Send(new GetPathQuery { Id = pathId });

      return Ok(model);
    }

    ///// <summary>
    ///// Get detailed path information by its Id
    ///// </summary>
    ///// <param name="pathId">An id of the path</param>
    ///// <returns>Detailed information of the path with modules included</returns>
    //[Route("api/pathdetails")]
    //[HttpGet("{pathId}", Name = "GetPathDetails")]
    //[HttpHead("{pathId}")]
    //public async Task<ActionResult<PathViewModel>> GetDetails(int pathId)
    //{
    //  PathViewModel model = await Mediator.Send(new GetPathDetailsQuery { Id = pathId });

    //  return Ok(model);
    //}

    /// <summary>
    /// Create a path
    /// </summary>
    /// <param name="command">Path object</param>
    /// <returns>Created path</returns>
    [HttpPost]
    public async Task<ActionResult<PathDto>> Create([FromBody] CreatePathCommand command)
    {
      PathDto model = await Mediator.Send(command);

      return CreatedAtRoute("GetPath", new { pathId = model.Id }, model);
    }

    /// <summary>
    /// Update the path with given Id
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <param name="command">Updated path</param>
    /// <returns></returns>
    [HttpPut("{pathId}")]
    public async Task<ActionResult<PathDto>> Update(int pathId,
      [FromBody] UpdatePathCommand command)
    {
      if (pathId != command.Id)
        return BadRequest();

      return Ok(await Mediator.Send(command));
    }

    // TODO: add PATCH

    /// <summary>
    /// Delete the path with given Id
    /// </summary>
    /// <param name="pathId">An id of the path</param>
    /// <returns></returns>
    [HttpDelete("{pathId}")]
    public async Task<IActionResult> Delete(int pathId)
    {
      await Mediator.Send(new DeletePathCommand { Id = pathId });

      return NoContent();
    }

    private async Task<ActionResult<IEnumerable<PathDto>>> GetAll()
    {
      IEnumerable<PathDto> model = await Mediator.Send(new GetPathListQuery());
      return Ok(model);
    }

    private async Task<ActionResult<IEnumerable<PathDto>>> GetPage(RequestParams filter)
    {
      var (paginationData, result) = await Mediator.Send(
        new GetPathListQueryPaging()
        {
          PageNumber = filter.PageNumber,
          PageSize = filter.PageSize
        });

      Response?.Headers?.Add("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationData));
      return Ok(result);
    }
  }
}