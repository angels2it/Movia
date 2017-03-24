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
        public UserPositionModel Position { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
    }

    public class UserPositionModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime UpdatedAt { get; set; }

        public UserPositionModel(double latitude, double longitude, DateTime updatedAt)
        {
            Latitude = latitude;
            Longitude = longitude;
            UpdatedAt = updatedAt;
        }
    }
}
