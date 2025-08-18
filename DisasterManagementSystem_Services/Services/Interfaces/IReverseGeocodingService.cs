using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterManagementSystem_Services.Services.Interfaces
{
    public interface IReverseGeocodingService
    {
        Task<string?> GetAddressAsync(double latitude, double longitude);
    }
}
