﻿using Moq;
using SteamPrefill.Handlers;
using SteamPrefill.Handlers.Steam;
using SteamPrefill.Models;
using SteamPrefill.Models.Enums;
using Xunit;
using OperatingSystem = SteamPrefill.Models.Enums.OperatingSystem;

namespace SteamPrefill.Test
{
    public class DepotHandlerTests
    {
        private readonly Mock<Steam3Session> _steam3Mock;
        private readonly DepotHandler _depotHandler;

        public DepotHandlerTests()
        {
            _steam3Mock = new Mock<Steam3Session>(null);
            // User will always have access to every depot
            _steam3Mock.Setup(m => m.AccountHasDepotAccess(It.IsAny<uint>())).Returns(true);
            _depotHandler = new DepotHandler(_steam3Mock.Object, null);
        }

        [Fact]
        public void UserDoesNotHaveDepotAccess_DepotIsFiltered()
        {
            // User will have no access to any depots
            _steam3Mock.Setup(m => m.AccountHasDepotAccess(It.IsAny<uint>())).Returns(false);
            
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555 }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments(), depotList);
            // Since the user has no access to any depots, we should expect this result to be empty
            Assert.Empty(filteredDepots);
        }

        [Fact]
        public void DepotHasNoMetadata_DepotIsIncluded()
        {
            var depotList = new List<DepotInfo>
            {
                // Depot is being setup without metadata
                new DepotInfo { DepotId = 123, ManifestId = 5555 }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments(), depotList);
            // Since the depot has no metadata, it should always be included
            Assert.Equal(1, filteredDepots.Count);
        }

        [Fact]
        public void OperatingSystemDoesntMatch_DepotIsNotIncluded()
        {
            // Depot is for macos only
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555, SupportedOperatingSystems = new List<OperatingSystem> { OperatingSystem.MacOS } }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments { OperatingSystem = OperatingSystem.Windows }, depotList);
            // We are only interested in windows depots, so we should expect the depot to be filtered
            Assert.Equal(0, filteredDepots.Count);
        }

        [Fact]
        public void OperatingSystemMatches_DepotIsIncluded()
        {
            // Depot is for windows only
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555, SupportedOperatingSystems = new List<OperatingSystem> { OperatingSystem.Windows } }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments { OperatingSystem = OperatingSystem.Windows }, depotList);
            // Since we want windows depots, the depot should be included
            Assert.Equal(1, filteredDepots.Count);
        }

        [Fact]
        public void ArchitectureDoesntMatch_DepotIsNotIncluded()
        {
            // Depot is for 64 bit only
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555, Architecture = Architecture.x64 }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments { Architecture = Architecture.x86 }, depotList);
            // We are only interested in 32 bit depots, so we should expect the depot to be filtered
            Assert.Equal(0, filteredDepots.Count);
        }

        [Fact]
        public void ArchitectureMatches_DepotIsIncluded()
        {
            // Depot is for 64 bit only
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555, Architecture = Architecture.x64 }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments { Architecture = Architecture.x64 }, depotList);
            // Since we want 64 bit depots, then we should expect the depot to be included
            Assert.Equal(1, filteredDepots.Count);
        }

        [Fact]
        public void LanguageDoesntMatch_DepotIsNotIncluded()
        {
            // Depot is for spanish
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555, Languages = new List<Language> { Language.Spanish } }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments { Language = Language.English }, depotList);
            // We are only interested in english depots, so we should expect the depot to be filtered
            Assert.Equal(0, filteredDepots.Count);
        }

        [Fact]
        public void LanguageMatches_DepotIsIncluded()
        {
            // Depot is for english
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555, Languages = new List<Language> { Language.English } }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments { Language = Language.English }, depotList);
            // Since we want english depots, then we should expect the depot to be included
            Assert.Equal(1, filteredDepots.Count);
        }

        [Fact]
        public void LowViolenceDepots_AreFiltered()
        {
            var depotList = new List<DepotInfo>
            {
                new DepotInfo { DepotId = 123, ManifestId = 5555, LowViolence = true }
            };

            var filteredDepots = _depotHandler.FilterDepotsToDownload(new DownloadArguments(), depotList);
            // Low violence depots should be expected to be filtered.
            Assert.Equal(0, filteredDepots.Count);
        }
    }
}
