using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChocoPM.Services
{
    public interface IRemoteChocolateyService
    {
        IQueryable<V2FeedPackage> Packages { get; }
    }
}
