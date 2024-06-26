using PlanningEngineerApplication.Models;

namespace PlanningEngineerApplication.Models
{
    public class EditUserRolesViewModel
    {
        public string Email { get; set; }
        public List<RoleViewModel> AssignedRoles { get; set; }
    }
}
