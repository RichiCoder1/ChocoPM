﻿using System.Linq;

namespace ChocoPM.Services
{
    public interface IRemoteChocolateyService
    {
        IQueryable<V2FeedPackage> Packages { get; }
    }
}
