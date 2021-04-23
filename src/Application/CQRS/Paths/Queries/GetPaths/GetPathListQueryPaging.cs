﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Shared.Dtos.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using DeveloperPath.Application.Common.Interfaces;
using DeveloperPath.Application.Paging;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace DeveloperPath.Application.CQRS.Paths.Queries.GetPaths
{
    /// <summary>
    /// Get paths paged
    /// </summary>
    public class GetPathListQueryPaging : IRequest<(PaginationData, IEnumerable<PathDto>)>
    {
        /// <summary>
        /// Page number
        /// </summary>
        public int PageNumber { get; init; }
        /// <summary>
        /// Items per page
        /// </summary>
        public int PageSize { get; init; }
    }

    internal class GetPathsPagingQueryHandler : IRequestHandler<GetPathListQueryPaging, (PaginationData, IEnumerable<PathDto>)>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetPathsPagingQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<(PaginationData, IEnumerable<PathDto>)> Handle(GetPathListQueryPaging request, CancellationToken cancellationToken)
        {
            IEnumerable<PathDto> pathCollection = null;

            if (request.PageNumber > 0 || request.PageSize > 0)
            {
                pathCollection = await _context.Paths.OrderBy(t => t.Title)
                 .ProjectTo<PathDto>(_mapper.ConfigurationProvider)
                 .Skip((request.PageNumber - 1) * request.PageSize)
                 .Take(request.PageSize)
                 .ToListAsync(cancellationToken);

                return (new PaginationData(request.PageNumber, request.PageSize), pathCollection);
            }

            pathCollection = await _context.Paths
             .OrderBy(t => t.Title)
             .ProjectTo<PathDto>(_mapper.ConfigurationProvider)
             .ToListAsync(cancellationToken);
            return (new PaginationData(request.PageNumber, request.PageSize), pathCollection);
        }
    }
}