using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Bobs_Parser.Models
{
    /// <summary>
    /// Individual items in the data grid
    /// </summary>
    public class GridStat
    {
        public TextBox Source { get; set; }
        public String Phrase { get; set; }
        public Int32  Count { get; set; }
        public Int32 ComparedCount { get; set; }
        public string OccuranceRate { get; set; }
    }
}
