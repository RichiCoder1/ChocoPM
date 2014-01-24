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
            _service.IgnoreResourceNotFoundException = true;
            _service.IgnoreMissingProperties = true;
        }


        public V2FeedPackage GetLatest(string id)
        {
            return _service.Packages.Where(package => package.Id == id && package.IsLatestVersion).FirstOrDefault();
        }
    }
}
