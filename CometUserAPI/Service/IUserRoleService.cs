using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Model;

namespace CometUserAPI.Service
{
    public interface IUserRoleService
    {
        Task<APIResponse> AssignRolePermission(List<MenuPermission> _data);
        Task<List<TblRole>> GetAllRoles();
        Task<List<TblMenu>> GetAllMenus();
        Task<List<AppMenu>> GetAllMenusByRole(string userRole);
        Task<MenuPermission> GetMenuPermissionByRole(string userRole, string menuCode);
    }
}
