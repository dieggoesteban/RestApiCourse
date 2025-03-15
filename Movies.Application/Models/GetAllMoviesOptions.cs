using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Models
{
    public class GetAllMoviesOptions
    {
        public string? Title { get; init; }
        public int? YearOfRelease { get; init; }
        public Guid? UserId { get; init; }
    }
}
