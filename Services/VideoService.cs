using System;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode;

namespace YoutubeConverter.Services
{
    internal class VideoService
    {
        private readonly YoutubeClient _youtubeClient;

        public VideoService()
        {
            _youtubeClient = new YoutubeClient();
        }

        public async Task<StreamManifest> GetStreamManifestAsync(string link)
        {
            return await _youtubeClient.Videos.Streams.GetManifestAsync(link);
        }

        public IStreamInfo GetHighestQualityVideoStream(StreamManifest manifest)
        {
            return manifest.GetVideoOnlyStreams()
                           .Where(s => s.Container == Container.Mp4)
                           .GetWithHighestVideoQuality();
        }

        public IStreamInfo GetHighestBitrateAudioStream(StreamManifest manifest)
        {
            return manifest.GetAudioOnlyStreams().GetWithHighestBitrate();
        }

        public async Task DownloadStreamAsync(IStreamInfo streamInfo, string outputPath, IProgress<double> progress = null)
        {
            await _youtubeClient.Videos.Streams.DownloadAsync(streamInfo, outputPath, progress);
        }
    }
}