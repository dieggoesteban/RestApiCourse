using FluentValidation;
using Movies.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Validators
{
    public class GetAllMoviesOptionsValidator : AbstractValidator<GetAllMoviesOptions>
    {
        private static readonly string[] ValidSortFields =
        {
            "title",
            "yearofrelease"
        };

        public GetAllMoviesOptionsValidator()
        {
            RuleFor(x => x.YearOfRelease).LessThanOrEqualTo(DateTime.UtcNow.Year);
            RuleFor(x => x.SortField).Must(x => x is null || ValidSortFields.Contains(x, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Valid sort fields are: {string.Join(", ", ValidSortFields)}");
            RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 15)
                .WithMessage("Page size must be between 1 and 15.");
        }
    }
}
