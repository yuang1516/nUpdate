﻿// Author: Dominic Beger (Trade/ProgTrade)

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace nUpdate
{
    public class UpdateVersion : IUpdateVersion
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateVersion" />-class.
        /// </summary>
        public UpdateVersion()
            : this(0, 0, 0, 0, DevelopmentalStage.Release, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateVersion" />-class.
        /// </summary>
        /// <param name="version">The update version.</param>
        public UpdateVersion(string version)
        {
            Match match = Regex.Match(version,
                @"^(?<Version>((?<VersionNumber>\d+)\.){0,3}(?<VersionNumber>\d+))((-| )?(?<DevStage>(?<Type>[ab]|rc)(\.?(?<DevBuild>\d+))?))?$");
            if (!match.Success || !match.Groups["Version"].Success)
                throw new ArgumentException("The specified version is not valid.");

            Major = int.Parse(match.Groups["VersionNumber"].Captures[0].Value);
            if (match.Groups["VersionNumber"].Captures.Count > 1)
                Minor = int.Parse(match.Groups["VersionNumber"].Captures[1].Value);
            else
                Build = 0;
            Build = match.Groups["VersionNumber"].Captures.Count > 2
                ? int.Parse(match.Groups["VersionNumber"].Captures[2].Value)
                : 0;
            Revision = match.Groups["VersionNumber"].Captures.Count > 3
                ? int.Parse(match.Groups["VersionNumber"].Captures[3].Value)
                : 0;

            if (!match.Groups["DevStage"].Success)
                return;
            var devStage = match.Groups["Type"].Value;
            DevelopmentalStage = devStage == "a"
                ? DevelopmentalStage.Alpha
                : devStage == "b" ? DevelopmentalStage.Beta : DevelopmentalStage.ReleaseCandidate;

            DevelopmentBuild = match.Groups["DevBuild"].Success ? int.Parse(match.Groups["DevBuild"].Value) : 0;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateVersion" />-class.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="build">The build version.</param>
        /// <param name="revision">The revision version.</param>
        public UpdateVersion(int major, int minor, int build, int revision)
            : this(major, minor, build, revision, DevelopmentalStage.Release, 0)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="UpdateVersion" />-class.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="build">The build version.</param>
        /// <param name="revision">The revision version.</param>
        /// <param name="developmentalStage">The developmental stage.</param>
        /// <param name="developmentBuild">The pre-release version.</param>
        public UpdateVersion(int major, int minor, int build, int revision, DevelopmentalStage developmentalStage,
            int developmentBuild)
        {
            if (major < 0)
                throw new ArgumentOutOfRangeException(nameof(major), "Index must be 0 or higher.");

            if (minor < 0)
                throw new ArgumentOutOfRangeException(nameof(minor), "Index must be 0 or higher.");

            if (build < 0)
                throw new ArgumentOutOfRangeException(nameof(build), "Index must be 0 or higher.");

            if (revision < 0)
                throw new ArgumentOutOfRangeException(nameof(revision), "Index must be 0 or higher.");

            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
            DevelopmentalStage = developmentalStage;
            DevelopmentBuild = developmentBuild;
        }

        /// <summary>
        ///     Gets or sets the major of this <see cref="UpdateVersion"/>.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        ///     Gets or sets the minor of this <see cref="UpdateVersion"/>.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        ///     Gets or sets the build of this <see cref="UpdateVersion"/>.
        /// </summary>
        public int Build { get; set; }

        /// <summary>
        ///     Gets or sets the revision of this <see cref="UpdateVersion"/>.
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        ///     Gets or sets the developmental stage of this <see cref="UpdateVersion"/>.
        /// </summary>
        public DevelopmentalStage DevelopmentalStage { get; set; }

        /// <summary>
        ///     Gets or sets the development build of this <see cref="UpdateVersion"/>.
        /// </summary>
        public int DevelopmentBuild { get; set; }

        /// <summary>
        ///     Gets the full description text of this <see cref="UpdateVersion" />.
        /// </summary>
        public virtual string Description => DevelopmentalStage != DevelopmentalStage.Release
            ? DevelopmentBuild != 0
                ? $"{BasicVersion} {DevelopmentalStage} {DevelopmentBuild.ToString(CultureInfo.InvariantCulture)}"
                : $"{BasicVersion} {DevelopmentalStage}"
            : BasicVersion;

        /// <summary>
        ///     Gets the current <see cref="UpdateVersion" /> without the developmental stage and development build.
        /// </summary>
        public string BasicVersion => $"{Major}.{Minor}.{Build}.{Revision}";

        public int CompareTo(IUpdateVersion version)
        {
            if (IsNewerThan(version))
                return -1;
            return IsEqualTo(version) ? 0 : 1;
        }

        // Overwritten Instance Methods

        /// <summary>
        ///     Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            if (DevelopmentalStage == DevelopmentalStage.Release)
                return BasicVersion;

            var fieldInfo = GetType().GetField(DevelopmentalStage.ToString());
            var descriptionAttributes =
                (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            //if (descriptionAttributes.Length > 0)
            var shortcut = descriptionAttributes[0].Description;
            return $"{Major}.{Minor}.{Build}.{Revision}{shortcut}{(DevelopmentBuild != 0 ? DevelopmentBuild.ToString(CultureInfo.InvariantCulture) : string.Empty)}";
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            var accumulator = 0;

            // ReSharper disable once NonReadonlyMemberInGetHashCode
            accumulator |= (Major & 0x0000000F) << 28;
            accumulator |= (Minor & 0x000000FF) << 20;
            accumulator |= (Build & 0x000000FF) << 12;
            accumulator |= (Revision & 0x00000FFF);

            return accumulator;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj.GetType() == typeof (UpdateVersion) && ToString() == obj.ToString();
        }

        public bool IsEqualTo(IUpdateVersion version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));
            // TODO: Check if it can be improved
            return ToString() == version.ToString();
        }
        
        public bool IsNewerThan(IUpdateVersion version)
        {
            if (Major > version.Major)
                return true;
            if (Major < version.Major)
                return false;

            if (Minor > version.Minor)
                return true;
            if (Minor < version.Minor)
                return false;

            if (Build > version.Build)
                return true;
            if (Build < version.Build)
                return false;

            if (Revision > version.Revision)
                return true;
            if (Revision < version.Revision)
                return false;

            if (DevelopmentalStage < version.DevelopmentalStage)
                return true;
            if (DevelopmentalStage > version.DevelopmentalStage)
                return false;

            return DevelopmentBuild > version.DevelopmentBuild;
        }
        
        public bool IsOlderThan(IUpdateVersion version)
        {
            if (Major < version.Major)
                return true;
            if (Major > version.Major)
                return false;

            if (Minor < version.Minor)
                return true;
            if (Minor > version.Minor)
                return false;

            if (Build < version.Build)
                return true;
            if (Build > version.Build)
                return false;

            if (Revision < version.Revision)
                return true;
            if (Revision > version.Revision)
                return false;

            if (DevelopmentalStage > version.DevelopmentalStage)
                return true;
            if (DevelopmentalStage < version.DevelopmentalStage)
                return false;

            return DevelopmentBuild < version.DevelopmentBuild;
        }

        public bool IsOlderOrEqualTo(IUpdateVersion version)
        {
            return IsOlderThan(version) || IsEqualTo(version);
        }

        public bool IsNewerOrEqualTo(IUpdateVersion version)
        {
            return IsNewerThan(version) || IsEqualTo(version);
        }
        
        /// <summary>
        ///     Returns the highest <see cref="UpdateVersion"/> in the given collection.
        /// </summary>
        /// <param name="updateVersions">The collection of <see cref="UpdateVersion"/>s to check.</param>
        /// <returns>Returns the highest <see cref="UpdateVersion"/> found.</returns>
        public static IUpdateVersion GetHighestUpdateVersion(IEnumerable<IUpdateVersion> updateVersions)
        {
            IUpdateVersion newestVersion = new UpdateVersion();
            // ReSharper disable once AccessToModifiedClosure
            foreach (var currentVersion in updateVersions.Where(i => i.IsNewerThan(newestVersion)))
                newestVersion = currentVersion;

            return newestVersion;
        }

        /// <summary>
        ///     Returns the lowest <see cref="UpdateVersion"/> in the given collection.
        /// </summary>
        /// <param name="updateVersions">The collection of <see cref="UpdateVersion"/>s to check.</param>
        /// <returns>Returns the lowest <see cref="UpdateVersion"/> found.</returns>
        public static IUpdateVersion GetLowestUpdateVersion(IEnumerable<IUpdateVersion> updateVersions)
        {
            var versionList = updateVersions.ToList();
            var lowestVersion = GetHighestUpdateVersion(versionList);
            // ReSharper disable once AccessToModifiedClosure
            foreach (var currentVersion in versionList.Where(i => i.IsOlderThan(lowestVersion)))
                lowestVersion = currentVersion;

            return lowestVersion;
        }

        // TODO: Kill this
        /// <summary>
        ///     Returns a <see cref="IUpdateVersion" /> from the given description.
        /// </summary>
        /// <param name="description">The description containing the version information.</param>
        /// <returns>The built a <see cref="IUpdateVersion" /> from the given description.</returns>
        /// <exception cref="System.ArgumentException">description</exception>
        public static IUpdateVersion FromDescription(string description)
        {
            var versionSections = description.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            if (versionSections.Length > 3)
                throw new ArgumentException(nameof(description));

            var versionParts = versionSections[0].Split('.');
            int major = int.Parse(versionParts[0]);
            int minor = int.Parse(versionParts[1]);
            int build = int.Parse(versionParts[2]);
            int revision = int.Parse(versionParts[3]);

            if (versionSections.Length == 1)
                return new UpdateVersion(major, minor, build, revision);

            var developmentalStage = DevelopmentalStage.Release;
            switch (versionSections[1])
            {
                case "Alpha":
                    developmentalStage = DevelopmentalStage.Alpha;
                    break;
                case "Beta":
                    developmentalStage = DevelopmentalStage.Beta;
                    break;
                case "ReleaseCandidate":
                    developmentalStage = DevelopmentalStage.ReleaseCandidate;
                    break;
            }

            if (versionSections.Length == 2)
                return new UpdateVersion(major, minor, build, revision, developmentalStage, 0);

            int developmentBuild = int.Parse(versionSections[2]);
            return new UpdateVersion(major, minor, build, revision, developmentalStage, developmentBuild);
        }
        
        /// <summary>
        ///     Determines whether the specified version string is a valid representation of a <see cref="UpdateVersion"/>.
        /// </summary>
        /// <param name="versionString">The version string to check.</param>
        public static bool IsValid(string versionString)
        {
            var regex =
                new Regex(
                    @"^(?<Version>((?<VersionNumber>\d+)\.){0,3}(?<VersionNumber>\d+))((-| )?(?<DevStage>(?<Type>[ab]|rc)(\.?(?<DevBuild>\d+))?))?$",
                    RegexOptions.IgnoreCase);
            return regex.IsMatch(versionString);
        }
    }
}