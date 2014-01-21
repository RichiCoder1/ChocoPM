using System;
using System.Linq;

namespace ChocoPM.Services
{
    class RemoteChocolateyService : IRemoteChocolateyService
    {
        readonly FeedContext_x0060_1 _service;

        public IQueryable<V2FeedPackage> Packages
        {
            get
            {
                return _service.Packages.AsQueryable();
            }
        }

        public RemoteChocolateyService()
        {
            // Todo: All this URI to be swapped with other NuGet compatible APIs
            _service = new FeedContext_x0060_1(new Uri("http://chocolatey.org/api/v2")); 
        }
    }
}
