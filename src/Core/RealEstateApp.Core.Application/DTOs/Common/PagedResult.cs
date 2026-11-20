using System;
using System.Collections.Generic;

namespace RealEstateApp.Core.Application.DTOs.Common
{
    /// <summary>
    /// Contenedor estándar paginado con límites acotados para evitar consultas sin tope.
    /// </summary>
    public class PagedResult<T>
    {
        public const int DefaultPage = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 50;

        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; } = DefaultPage;
        public int PageSize { get; init; } = DefaultPageSize;
        public int TotalCount { get; init; }
        public int TotalPages => TotalCount <= 0 || PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;

        public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
        {
            var normalizedPage = page.HasValue && page.Value >= 1 ? page.Value : DefaultPage;
            var normalizedSize = pageSize.HasValue && pageSize.Value >= 1
                ? Math.Min(pageSize.Value, MaxPageSize)
                : DefaultPageSize;

            return (normalizedPage, normalizedSize);
        }

        public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            var (normalizedPage, normalizedSize) = Normalize(page, pageSize);
            return new PagedResult<T>
            {
                Items = items ?? Array.Empty<T>(),
                Page = normalizedPage,
                PageSize = normalizedSize,
                TotalCount = totalCount < 0 ? 0 : totalCount
            };
        }
    }
}
