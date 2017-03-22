using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Maps;

namespace Movia.Mobile.Models
{
    public class UserModel
    {
        public string Id { get; set; }
        public Position Position { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }
}
