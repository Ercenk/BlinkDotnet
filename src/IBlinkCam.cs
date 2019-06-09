using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BlinkDotnet.DTO;

namespace BlinkDotnet
{
    public interface IBlinkCam
    {
        Task<IEnumerable<RecordedVideo>> GetAllVideosListAsync(CancellationToken cancelToken = default);
        Task<IEnumerable<CameraEvent>> GetEventsAsync(CancellationToken cancelToken = default);
        Task<IEnumerable<NetworkDetail>> GetNetworksAsync(CancellationToken cancelToken = default);
        Task<LoginResponse> LoginAsync(CancellationToken cancelToken = default);
        Task<MemoryStream> GetVideoAsMemoryStreamAsync(string videoAddress, CancellationToken cancelToken = default);
    }
}