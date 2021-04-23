﻿using DeveloperPath.Application.Common.Exceptions;
using DeveloperPath.Application.Common.Interfaces;
using DeveloperPath.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DeveloperPath.Application.CQRS.Themes.Commands.DeleteTheme
{
    /// <summary>
    /// Delete theme parameters
    /// </summary>
    public record DeleteThemeCommand : IRequest
    {
        /// <summary>
        /// Theme id
        /// </summary>
        public int Id { get; init; }
        /// <summary>
        /// Path id
        /// </summary>
        public int PathId { get; init; }
        /// <summary>
        /// Module id
        /// </summary>
        public int ModuleId { get; init; }
    }

    internal class DeleteThemeCommandHandler : IRequestHandler<DeleteThemeCommand>
    {
        private readonly IApplicationDbContext _context;

        public DeleteThemeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteThemeCommand request, CancellationToken cancellationToken)
        {
            //TODO: check if requested module is in requested path (???)
            var path = await _context.Paths.FindAsync(new object[] { request.PathId }, cancellationToken);
            if (path == null)
                throw new NotFoundException(nameof(Path), request.PathId);

            var entity = await _context.Themes
              .Where(t => t.Id == request.Id && t.ModuleId == request.ModuleId)
              .FirstOrDefaultAsync(cancellationToken);
            if (entity == null)
                throw new NotFoundException(nameof(Theme), request.Id);

            _context.Themes.Remove(entity);

            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}