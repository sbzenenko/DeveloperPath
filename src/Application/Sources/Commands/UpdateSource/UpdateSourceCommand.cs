﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeveloperPath.Application.Common.Exceptions;
using DeveloperPath.Application.Common.Interfaces;
using DeveloperPath.Application.Common.Models;
using DeveloperPath.Domain.Entities;
using DeveloperPath.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeveloperPath.Application.Sources.Commands.UpdateSource
{
  /// <summary>
  /// Source to update
  /// </summary>
  public record UpdateSourceCommand : IRequest<SourceDto>
  {
    /// <summary>
    /// Source Id
    /// </summary>
    [Required]
    public int Id { get; init; }
    /// <summary>
    /// Path Id
    /// </summary>
    [Required]
    public int PathId { get; init; }
    /// <summary>
    /// Module Id
    /// </summary>
    [Required]
    public int ModuleId { get; init; }
    /// <summary>
    /// Theme id that the source is for
    /// </summary>
    [Required]
    public int ThemeId { get; init; }
    /// <summary>
    /// Source title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; init; }
    /// <summary>
    /// Source short summary
    /// </summary>
    [Required]
    [MaxLength(10000)]
    public string Description { get; init; }
    /// <summary>
    /// Source Url
    /// </summary>
    [Required]
    [MaxLength(500)]
    [RegularExpression(@"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,4}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)")]
    public string Url { get; init; }
    /// <summary>
    /// Position of source in theme (0-based).
    /// </summary>
    public int Order { get; init; }
    /// <summary>
    /// Type of source (None | Book | Blog | Course | Documentation | QandA | Video)
    /// </summary>
    public SourceType Type { get; init; }
    /// <summary>
    /// Whether the resource Free | Requires registration | Paid only
    /// </summary>
    public AvailabilityLevel Availability { get; init; }
    /// <summary>
    /// Whether inforation is Not applicable (default) | Up-to-date | Somewhat up-to-date | Outdated
    /// </summary>
    public RelevanceLevel Relevance { get; init; }
    /// <summary>
    /// List of tags related to theme
    /// </summary>
    public IList<string> Tags { get; init; }
  }

  internal class UpdateSourceCommandHandler : IRequestHandler<UpdateSourceCommand, SourceDto>
  {
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateSourceCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
      _context = context;
      _mapper = mapper;
    }

    public async Task<SourceDto> Handle(UpdateSourceCommand request, CancellationToken cancellationToken)
    {
      //TODO: check if requested module is in requested path (???)
      var path = await _context.Paths.FindAsync(new object[] { request.PathId }, cancellationToken);
      if (path == null)
        throw new NotFoundException(nameof(Path), request.PathId);

      var theme = await _context.Themes
        .Where(t => t.Id == request.ThemeId && t.ModuleId == request.ModuleId)
        .FirstOrDefaultAsync(cancellationToken);
      if (theme == null)
        throw new NotFoundException(nameof(Theme), request.ThemeId);

      var entity = await _context.Sources.FindAsync(new object[] { request.Id }, cancellationToken);
      if (entity == null)
        throw new NotFoundException(nameof(Source), request.Id);

      // TODO: is there a way to use init-only fields?
      entity.ThemeId = request.ThemeId;
      entity.Title = request.Title;
      entity.Description = request.Description;
      entity.Url = request.Url;
      entity.Order = request.Order;
      entity.Type = request.Type;
      entity.Availability = request.Availability;
      entity.Relevance = request.Relevance;

      await _context.SaveChangesAsync(cancellationToken);

      return _mapper.Map<SourceDto>(entity);
    }
  }
}