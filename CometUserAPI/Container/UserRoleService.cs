using Azure;
using CometUserAPI.Entities;
using CometUserAPI.Helper;
using CometUserAPI.Model;
using CometUserAPI.Service;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace CometUserAPI.Container
{
    public class UserRoleService: IUserRoleService
    {
        private readonly CometUserDBContext _dbContext;
        public UserRoleService(CometUserDBContext context) { 
            this._dbContext = context;
        }

        public async Task<APIResponse> AssignRolePermission(List<TblRolepermission> _data)
        {
            APIResponse response = new APIResponse();
            int processCount = 0;
            try
            {
                using(var dbtransaction = await this._dbContext.Database.BeginTransactionAsync())
                if (_data.Count > 0)
                {
                    _data.ForEach(item =>
                    {
                        var userData = this._dbContext.TblRolepermissions.FirstOrDefault(item1 => item1.Userrole == item.Userrole && item1.Menucode == item.Menucode);
                        if(userData != null)
                        {
                            userData.Haveview = item.Haveview;
                            userData.Haveadd = item.Haveadd;
                            userData.Haveedit = item.Haveedit;
                            userData.Havedelete = item.Havedelete;
                            processCount++;
                        } else
                        {
                            this._dbContext.TblRolepermissions.Add(item);
                            processCount++;
                        }
                    });

                    if(_data.Count == processCount)
                    {
                        await this._dbContext.SaveChangesAsync();
                        await dbtransaction.CommitAsync();
                        response.Result = "Saved successfully";
                    }
                    else
                    {
                        await dbtransaction.RollbackAsync();
                    }
                }
                else
                {
                    response.Result = "fail";
                    response.Message = "please proceed with minimum 1 record";
                }
            }
            catch (Exception ex) {
                response = new APIResponse();
            }
            return response;

        }

        public async Task<List<TblMenu>> GetAllMenus()
        {
            return await this._dbContext.TblMenus.ToListAsync();
        }

        public async Task<List<AppMenu>> GetAllMenusByRole(string userRole)
        {
            List<AppMenu> appMenus = new List<AppMenu>();

            var accessData = (from menu in this._dbContext.TblRolepermissions.Where(o => o.Userrole == userRole && o.Haveview)
                              join m in this._dbContext.TblMenus on menu.Menucode equals m.Code into _jointable
                              from p in _jointable.DefaultIfEmpty()
                              select new {code=menu.Menucode,name=p.Name}).ToList();

            if (accessData.Any()) 
            {
                accessData.ForEach(item =>
                {
                    appMenus.Add(new AppMenu()
                    {
                        code=item.code,
                        name=item.name
                    });
                });
            }

            return appMenus;
        }

        public async Task<List<TblRole>> GetAllRoles()
        {
            return await this._dbContext.TblRoles.ToListAsync();
        }

        public async Task<MenuPermission> GetMenuPermissionByRole(string userRole, string menuCode)
        {
            MenuPermission menuPermission = new MenuPermission();
            var _data = await this._dbContext.TblRolepermissions.FirstOrDefaultAsync(o => o.Userrole == userRole && o.Haveview
            && o.Menucode == menuCode);

            if(_data != null)
            {
                menuPermission.Code = _data.Menucode;
                menuPermission.Haveview = _data.Haveview;
                menuPermission.Haveadd = _data.Haveadd;
                menuPermission.Haveedit = _data.Haveedit;
                menuPermission.Havedelete = _data.Havedelete;
            }

            return menuPermission;
        }
    }
}
