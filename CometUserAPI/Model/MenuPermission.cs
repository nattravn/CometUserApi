using System.ComponentModel.DataAnnotations.Schema;

namespace CometUserAPI.Model
{
    public class MenuPermission
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public bool Haveview { get; set; }
        public bool Haveadd { get; set; }
        public bool Haveedit { get; set; }
        public bool Havedelete { get; set; }
    }
}
