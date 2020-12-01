using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SocialDistancing.API.DataContracts.Helpers
{
    public class TerminalPatch
    {

        [DataType(DataType.Text)]
        public string LeftNeighborSamNumber { get; set; }

        [DataType(DataType.Text)]
        public string RightNeighborSamNumber { get; set; }

        public bool Available { get; set; }
    }
}
