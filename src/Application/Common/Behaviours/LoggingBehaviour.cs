﻿using DeveloperPath.Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperPath.Application.Common.Behaviours
{
  /// <summary>
  /// Mediator pipilene behaviour for logging
  /// </summary>
  /// <typeparam name="TRequest"></typeparam>
  public class LoggingBehaviour<TRequest> : IRequestPreProcessor<TRequest>
  {
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly IIdentityService _identityService;

    /// <summary>
    /// Ctor for injecting dependencies
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="currentUserService"></param>
    /// <param name="identityService"></param>
    public LoggingBehaviour(ILogger<TRequest> logger, ICurrentUserService currentUserService, IIdentityService identityService)
    {
      _logger = logger;
      _currentUserService = currentUserService;
      _identityService = identityService;
    }

    /// <summary>
    /// Process method executes before calling the Handle method on handler
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
      var requestName = typeof(TRequest).Name;
      var userId = _currentUserService.UserId ?? string.Empty;
      string userName = string.Empty;

      if (!string.IsNullOrEmpty(userId))
      {
        userName = await _identityService.GetUserNameAsync(userId);
      }

      _logger.LogInformation("DeveloperPath Request: {Name} {@UserId} {@UserName} {@Request}",
          requestName, userId, userName, request);
    }
  }
}
