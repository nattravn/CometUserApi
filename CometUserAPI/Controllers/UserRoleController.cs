using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CometUserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRoleController : ControllerBase
    {
        private readonly IUserRoleService _roleService;

        public UserRoleController(IUserRoleService roleService)
        {
            this._roleService = roleService;
        }

        [HttpPost("assignpermission")]
        public async Task<ActionResult> assignRolePermission(List<TblRolepermission> rolePermissions)
        {
            var data = await this._roleService.AssignRolePermission(rolePermissions);
            return Ok(data);
        }

        [HttpGet("GetAllRoles")]
        public async Task<ActionResult> GetAllRoles()
        {
            var data = await this._roleService.GetAllRoles();
            if(data == null)
            {
                return Ok(new List<TblRole>());
            }
            return Ok(data);
        }

        [HttpGet("GetAllMenus")]
        public async Task<ActionResult> GetAllMenus()
        {
            var data = await this._roleService.GetAllMenus();
            if (data == null)
            {
                return Ok(new List<TblMenu>());
            }
            return Ok(data);
        }

        [HttpGet("GetAllMenusByRole")]
        public async Task<ActionResult> GetAllMenusByRole(string userRole)
        {
            var data = await this._roleService.GetAllMenusByRole(userRole);
            if (data == null)
            {
                return Ok(new List<TblMenu>());
            }
            return Ok(data);
        }

        [HttpGet("GetMenuPermissionByRole")]
        public async Task<ActionResult> GetMenuPermissionByRole(string userRole, string menuCode)
        {
            var data = await this._roleService.GetMenuPermissionByRole(userRole, menuCode);
            if (data == null)
            {
                return Ok(new List<TblMenu>());
            }
            return Ok(data);
        }
    }
}
