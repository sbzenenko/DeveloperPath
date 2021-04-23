﻿using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeveloperPath.Application.Common.Exceptions;
using DeveloperPath.Application.Common.Interfaces;
using DeveloperPath.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeveloperPath.Application.CQRS.Modules.Queries.GetModules
{
    /// <summary>
    /// Get module details parameters
    /// </summary>
    public class GetModuleDetailsQuery : IRequest<ModuleViewModel>
    {
        /// <summary>
        /// Module Id
        /// </summary>
        public int Id { get; init; }
        /// <summary>
        /// Path Id
        /// </summary>
        public int PathId { get; init; }
    }

    internal class GetModuleDetailsHandler : IRequestHandler<GetModuleDetailsQuery, ModuleViewModel>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetModuleDetailsHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ModuleViewModel> Handle(GetModuleDetailsQuery request, CancellationToken cancellationToken)
        {
            var result = await _context.Modules
              .Include(m => m.Paths)
              .Include(m => m.Prerequisites)
              .Include(m => m.Themes)
              .Include(m => m.Sections)
              .Where(m => m.Id == request.Id)
              .FirstOrDefaultAsync(cancellationToken);

            if (result == null || result.Paths.Where(p => p.Id == request.PathId) == null)
                throw new NotFoundException(nameof(Module), request.Id);

            //TODO: is there another way to map single item?
            return _mapper.Map<ModuleViewModel>(result);
        }
    }
}