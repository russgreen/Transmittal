using System.ComponentModel.DataAnnotations;

namespace Transmittal.Library.Models;
public class TransmittalModel
{
    public int ID { get; set; }
    [Required]
    public DateTime TransDate { get; set; } = DateTime.Now;
    public List<TransmittalDistributionModel> Distribution { get; set; } = new List<TransmittalDistributionModel>();
    public List<TransmittalItemModel> Items { get; set; } = new List<TransmittalItemModel>();
}
