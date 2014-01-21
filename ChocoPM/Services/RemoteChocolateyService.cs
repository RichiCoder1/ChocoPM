using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocoPM.Services
{
    class RemoteChocolateyService : IRemoteChocolateyService
    {
        FeedContext_x0060_1 _service;

        public IQueryable<V2FeedPackage> Packages
        {
            get
            {
                return _service.Packages.AsQueryable();
            }
        }

        public RemoteChocolateyService()
        {
            _service = new Services.FeedContext_x0060_1(new Uri("http://chocolatey.org/api/v2")); 
        }
    }
}
