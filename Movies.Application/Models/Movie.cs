using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Movies.Application.Models
{
    public class Movie
    {
        public required Guid Id { get; init; }
        public required string Title { get; init; }
        public string Slug => GetSlug();
        public required int YearOfRelease { get; init; }
        public required List<string> Genres { get; init; } = [];

        public string GetSlug()
        {
            var sluggedTitle = Regex.Replace(Title, "[^a-zA-Z0-9 _-]", string.Empty)
                .ToLower()
                .Replace(" ", "-");

            return $"{sluggedTitle}-{YearOfRelease}";
        }
    }
}
