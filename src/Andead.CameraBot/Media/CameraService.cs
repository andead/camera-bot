﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Andead.CameraBot.Media
{
    internal class CameraService : ICameraService
    {
        private readonly IOptions<CameraBotOptions> _options;
        private readonly ILogger<CameraService> _logger;
        private readonly HttpClient _client;

        public CameraService(IOptions<CameraBotOptions> options, ILogger<CameraService> logger)
        {
            _options = options;
            _logger = logger;
            _client = new HttpClient();
        }

        public Task<IEnumerable<string>> GetAvailableCameraNames()
        {
            return Task.FromResult(_options.Value.Cameras.Values.Select(value => value.Name));
        }

        public Task<Snapshot> GetSnapshot(string cameraName)
        {
            CameraOptions camera = _options.Value.Cameras.Values
                .FirstOrDefault(c => string.Equals(cameraName, c.Name, StringComparison.OrdinalIgnoreCase));
            if (camera == null)
            {
                _logger.LogWarning("Camera with name {CameraName} was not found.", cameraName);
                return Task.FromResult<Snapshot>(null);
            }

            return GetSnapshotInternal(camera);
        }

        private async Task<Snapshot> GetSnapshotInternal(CameraOptions camera)
        {
            try
            {
                return new Snapshot
                {
                    CameraName = camera.Name,
                    CameraUrl = camera.Url,
                    Stream = await _client.GetStreamAsync(camera.SnapshotUrl)
                };
            }
            catch (TaskCanceledException)
            {
                // ignored
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error getting snapshot from {@Camera}.", camera);
            }

            return null;
        }
    }
}