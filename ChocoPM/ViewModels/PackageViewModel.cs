﻿using AutoMapper;
using System;

namespace ChocoPM.ViewModels
{
    public class PackageViewModel : ViewModelBase
    {
        static PackageViewModel()
        {
            Mapper.CreateMap<Services.V2FeedPackage, PackageViewModel>();
        }

        public PackageViewModel(Services.V2FeedPackage feedPackage)
        {
            Mapper.Map(feedPackage, this);
        }

        #region Properties
        private string _authors;
        public string Authors
        {
            get { return _authors; }
            set { SetPropertyValue(ref _authors, value); }
        }

        public bool CanUpdate
        {
            get { return IsInstalled && !IsLatestVersion; }
        }

        private string _copyright;
        public string Copyright
        {
            get { return _copyright; }
            set { SetPropertyValue(ref _copyright, value); }
        }
        
        private DateTime _created;
        public DateTime Created
        {
            get { return _created; }
            set { SetPropertyValue(ref _created, value); }
        }

        private string _dependencies;
        public string Dependencies
        {
            get { return _dependencies; }
            set { SetPropertyValue(ref _dependencies, value); }
        }

        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetPropertyValue(ref _description, value); }
        }

        private int _downloadCount;
        public int DownloadCount
        {
            get { return _downloadCount; }
            set { SetPropertyValue(ref _downloadCount, value); }
        }

        private string _galleryDetailsUrl;
        public string GalleryDetailsUrl
        {
            get { return _galleryDetailsUrl; }
            set { SetPropertyValue(ref _galleryDetailsUrl, value); }
        }

        private string _iconUrl;
        public string IconUrl
        {
            get { return _iconUrl; }
            set { SetPropertyValue(ref _iconUrl, value); }
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetPropertyValue(ref _id, value); }
        }

        private bool _isAbsoluteLatestVersion;
        public bool IsAbsoluteLatestVersion
        {
            get { return _isAbsoluteLatestVersion; }
            set { SetPropertyValue(ref _isAbsoluteLatestVersion, value); }
        }

        public bool IsInstalled
        {

            // Todo: Check local Package Service
            get { return false; }
        }

        private bool _isLatestVersion;
        public bool IsLatestVersion
        {
            get { return _isLatestVersion; }
            set { SetPropertyValue(ref _isLatestVersion, value); }
        }
        
        private bool _isPrerelease;
        public bool IsPrerelease
        {
            get { return _isPrerelease; }
            set { SetPropertyValue(ref _isPrerelease, value); }
        }

        private string _language;
        public string Language
        {
            get { return _language; }
            set { SetPropertyValue(ref _language, value); }
        }

        private DateTime _lastUpdated;
        public DateTime LastUpdated
        {
            get { return _lastUpdated; }
            set { SetPropertyValue(ref _lastUpdated, value); }
        }

        private string _licenseUrl;
        public string LicenseUrl
        {
            get { return _licenseUrl; }
            set { SetPropertyValue(ref _licenseUrl, value); }
        }

        private string _packageHash;
        public string PackageHash
        {
            get { return _packageHash; }
            set { SetPropertyValue(ref _packageHash, value); }
        }
        
        private string _packageHashAlgorithm;
        public string PackageHashAlgorithm
        {
            get { return _packageHashAlgorithm; }
            set { SetPropertyValue(ref _packageHashAlgorithm, value); }
        }

        private long _packageSize;
        public long PackageSize
        {
            get { return _packageSize; }
            set { SetPropertyValue(ref _packageSize, value); }
        }
        
        private string _projectUrl;
        public string ProjectUrl
        {
            get { return _projectUrl; }
            set { SetPropertyValue(ref _projectUrl, value); }
        }

        private DateTime _published;
        public DateTime Published
        {
            get { return _published; }
            set { SetPropertyValue(ref _published, value); }
        }

        private string _releaseNotes;
        public string ReleaseNotes
        {
            get { return _releaseNotes; }
            set { SetPropertyValue(ref _releaseNotes, value); }
        }

        private string _reportAbuseUrl;
        public string ReportAbuseUrl
        {
            get { return _reportAbuseUrl; }
            set { SetPropertyValue(ref _reportAbuseUrl, value); }
        }

        private string _requireLicenseAcceptance;
        public string RequireLicenseAcceptance
        {
            get { return _requireLicenseAcceptance; }
            set { SetPropertyValue(ref _requireLicenseAcceptance, value); }
        }

        private string _summary;
        public string Summary
        {
            get { return _summary; }
            set { SetPropertyValue(ref _summary, value); }
        }

        private string _tags;
        public string Tags
        {
            get { return _tags; }
            set { SetPropertyValue(ref _tags, value); }
        }
		
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetPropertyValue(ref _title, value); }
        }

        private string _version;
        public string Version
        {
            get { return _version; }
            set { SetPropertyValue(ref _version, value); }
        }

        private int _versionDownloadCount;
        public int VersionDownloadCount
        {
            get { return _versionDownloadCount; }
            set { SetPropertyValue(ref _versionDownloadCount, value); }
        }
		
        #endregion

        #region Package Methods
        public void Install()
        {
        }

        public void Remove()
        {
        }

        public void Update()
        {
        }
        #endregion
    }
}
