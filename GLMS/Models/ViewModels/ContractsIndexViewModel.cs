using GLMS.Models;

namespace GLMS.Models.ViewModels
{
    public class ContractsIndexViewModel
    {
        public List<Contract> Contracts { get; set; } = new List<Contract>();
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ContractStatus? SelectedStatus { get; set; }
    }
}