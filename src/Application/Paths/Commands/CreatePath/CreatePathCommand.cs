﻿using AutoMapper;
using DeveloperPath.Application.Common.Interfaces;
using DeveloperPath.Application.Common.Models;
using DeveloperPath.Domain.Entities;
using MediatR;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperPath.Application.Paths.Commands.CreatePath
{
  /// <summary>
  /// Path to create
  /// </summary>
  public record CreatePathCommand : IRequest<PathDto>
  {
    /// <summary>
    /// Path title
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Title { get; init; }
    /// <summary>
    /// Path short summary
    /// </summary>
    [Required]
    [MaxLength(3000)]
    public string Description { get; init; }
    /// <summary>
    /// List of tags related to path
    /// </summary>
    public IList<string> Tags { get; init; }
  }

  internal class CreatePathCommandHandler : IRequestHandler<CreatePathCommand, PathDto>
  {
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreatePathCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<PathDto> Handle(CreatePathCommand request, CancellationToken cancellationToken)
    {
      var entity = new Path
      {
        Title = request.Title,
        Description = request.Description,
        Tags = request.Tags
      };

      _context.Paths.Add(entity);

      await _context.SaveChangesAsync(cancellationToken);

      return _mapper.Map<PathDto>(entity);
    }
  }
}