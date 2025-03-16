using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Contracts.Responses
{
    public class PagedResponse<TResponse>
    {
        public required List<TResponse> Items { get; init; } = [];
        public required int Page { get; init; }
        public required int  PageSize { get; init; }
        public required int Total { get; init; }
        public bool HasNextPage => Page * PageSize < Total;
    }
}
