using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace ChocoPM.Models
{
    public struct Version : IComparable, IComparable<Version>
    {
        private static readonly Regex VersionRegex = new Regex(@"(?<Major>[0-9]*)\.?(?<Minor>[0-9]*)?\.?(?<Patch>[0-9]*)?\.?(?<Revision>[0-9]*)?(?<Addendum>-\w*)?");
        public Version(string version)
            : this()
        {
            VersionString = version;

            var match = VersionRegex.Match(version);
            Major = uint.Parse(match.Groups["Major"].Value);

            var minor = match.Groups["Minor"];
            if (minor != null && !string.IsNullOrWhiteSpace(minor.Value))
                Minor = uint.Parse(minor.Value);

            var patch = match.Groups["Patch"];
            if (patch != null && !string.IsNullOrWhiteSpace(patch.Value))
                Patch = uint.Parse(patch.Value);

            var revision = match.Groups["Revision"];
            if (revision != null && !string.IsNullOrWhiteSpace(revision.Value))
                Revision = uint.Parse(revision.Value);

            var addendum = match.Groups["Addendum"];
            if (addendum != null && !string.IsNullOrWhiteSpace(addendum.Value))
                Addendum = addendum.Value;
        }
        public string VersionString { get; private set; }
        public uint Major { get; private set; }
        public uint? Minor { get; private set; }
        public uint? Patch { get; private set; }
        public uint? Revision { get; private set; }
        public string Addendum { get; private set; }

        public int CompareTo(Version other)
        {
            if (Major > other.Major)
                return 1;
            if (Major < other.Major)
                return -1;

            if (Minor.HasValue && !other.Minor.HasValue)
                return 1;
            if (!Minor.HasValue && other.Minor.HasValue)
                return -1;

            if (Minor > other.Minor)
                return 1;
            if (Minor < other.Minor)
                return -1;

            if (Patch.HasValue && !other.Patch.HasValue)
                return 1;
            if (!Patch.HasValue && other.Patch.HasValue)
                return -1;

            if (Patch > other.Patch)
                return 1;
            if (Patch < other.Patch)
                return -1;

            if (Revision.HasValue && !other.Revision.HasValue)
                return 1;
            if (!Revision.HasValue && other.Revision.HasValue)
                return -1;

            if (Revision > other.Revision)
                return 1;
            if (Revision < other.Revision)
                return -1;

            // This is "reversed" because if we don't have an addendum, that means we're not prerelease.
            // Or at least that's how I understand it.
            if (Addendum == null && other.Addendum != null)
                return 1;

            if (Addendum != null && other.Addendum == null)
                return -1;

            return StringComparer.InvariantCulture.Compare(Addendum, other.Addendum);
        }

        public override string ToString()
        {
            return VersionString;
        }

        public static implicit operator Version(string versionString)
        {
            return new Version(versionString);
        }

        public int CompareTo(object obj)
        {
            if (obj is Version)
                return CompareTo((Version) obj);
            return Comparer.DefaultInvariant.Compare(VersionString, obj);
        }
    }
}
