using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnooNotes.Models
{
    public class TableResults<T>
    {
        public int CurrentPage { get; set; }
        public int ResultsPerPage { get; set; }
        public int TotalResults { get; set; }

        public T DataTable { get; set; }
    }
}
