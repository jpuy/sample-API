using System.ComponentModel.DataAnnotations;

namespace SocialDistancing.API.DataContracts
{
    public class Terminal
    {
        [DataType(DataType.Text)]
        public string Id { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string SamNumber { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string LocationID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string LeftNeighborSamNumber { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string RightNeighborSamNumber { get; set; }

        [Required]
        public bool InUse { get; set; }
    }
}
