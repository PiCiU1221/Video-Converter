using AngleSharp.Dom;
using Microsoft.UI.Dispatching;
using System;
using System.IO;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace YoutubeConverter.Services
{
    internal class ConverterService
    {
        private readonly DialogService _dialogService;
        private readonly VideoService _videoService;

        public ConverterService(DialogService dialogService)
        {
            _dialogService = dialogService;
            _videoService = new VideoService();
        }

        /// <summary>
        /// Downloads audio from a Youtube video to a MP3 file from a Youtube link.
        /// </summary>
        public async Task LinkToMp3(string link)
        {
            _dialogService.ShowLoadingDialogAsync();
            _dialogService.UpdateStatus("(1/3) Pobieranie strumieni audio...");

            StreamManifest streamManifest = null;

            try
            {
                streamManifest = await _videoService.GetStreamManifestAsync(link);
            }
            catch (Exception)
            {
                _dialogService.CloseLoadingDialog();
                _dialogService.ShowMessage("Link wideo z Youtube niepoprawny", "Błąd");

                return;
            }

            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var stream = _videoService.GetHighestBitrateAudioStream(streamManifest);

            var title = await GetAudioTitle(link);

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string outputPath = Path.Combine(desktopPath, $"{title}.mp3");

            _dialogService.UpdateStatus("(3/3) Pobieranie audio...");

            long totalSize = streamInfo.Size.Bytes;
            DateTime lastUpdateTime = DateTime.Now.AddMilliseconds(-500);
            TimeSpan updateInterval = TimeSpan.FromMilliseconds(500);

            IProgress<double> progressAudio = new Progress<double>(percent =>
            {
                if (DateTime.Now - lastUpdateTime >= updateInterval)
                {
                    double downloadedMB = (percent * totalSize) / (1024.0 * 1024.0);
                    double totalMB = totalSize / (1024.0 * 1024.0);

                    var statusMessage = $"(3/3) Pobieranie audio ({downloadedMB:F2} MB / {totalMB:F2} MB, {percent:P})";
                    _ = DispatcherQueue.GetForCurrentThread().TryEnqueue(() => _dialogService.UpdateStatus(statusMessage));

                    lastUpdateTime = DateTime.Now;
                }
            });
            await _videoService.DownloadStreamAsync(streamInfo, outputPath, progressAudio);

            var message = $"Pomyślnie utworzono nowy plik MP3 na pulpicie o nazwie: ";
            _dialogService.CloseLoadingDialog();
            _dialogService.ShowSuccessfulMessage(message, title);
        }

        /// <summary>
        /// Retrieves Youtube video name from a link.
        /// </summary>
        private async Task<string> GetAudioTitle(string link)
        {
            try
            {
                _dialogService.UpdateStatus("(2/3) Pobieranie nazwy wideo...");
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(link);
                return video.Title;
            }
            catch (Exception)
            {
                return "Audio " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            }
        }

        /// <summary>
        /// Downloads a Youtube video to a MP4 file from a Youtube link.
        /// </summary>
        public async Task LinkToMp4(string link)
        {
            _dialogService.ShowLoadingDialogAsync();
            _dialogService.UpdateStatus("(1/6) Pobieranie strumieni wideo...");

            StreamManifest streamManifest = null;

            try
            {
                streamManifest = await _videoService.GetStreamManifestAsync(link);
            }
            catch (Exception)
            {
                _dialogService.CloseLoadingDialog();
                _dialogService.ShowMessage("Link wideo z Youtube niepoprawny", "Błąd");

                return;
            }

            var videoStreamInfo = _videoService.GetHighestQualityVideoStream(streamManifest);
            var audioStreamInfo = _videoService.GetHighestBitrateAudioStream(streamManifest);

            string title = await GetVideoTitle(link);
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string videoPath = Path.Combine(desktopPath, $"{title}_video.mp4");
            string audioPath = Path.Combine(desktopPath, $"{title}_audio.mp3");
            string outputPath = Path.Combine(desktopPath, $"{title}.mp4");

            await DownloadVideoAndAudioAsync(videoStreamInfo, audioStreamInfo, videoPath, audioPath);

            _dialogService.UpdateStatus("(5/6) Łączenie wideo i audio...");
            await FFmpegService.MergeVideoAndAudioAsync(videoPath, audioPath, outputPath);

            _dialogService.UpdateStatus("(6/6) Czyszczenie tymczasowych plików...");
            CleanUpTemporaryFiles(videoPath, audioPath);

            var message = $"Pomyślnie utworzono nowy plik MP4 na pulpicie o nazwie: ";
            _dialogService.CloseLoadingDialog();
            _dialogService.ShowSuccessfulMessage(message, title);
        }

        /// <summary>
        /// Retrieves Youtube video name from a link.
        /// </summary>
        private async Task<string> GetVideoTitle(string link)
        {
            try
            {
                _dialogService.UpdateStatus("(2/6) Pobieranie nazwy wideo...");
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(link);
                return video.Title;
            }
            catch (Exception)
            {
                return "Film " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
            }
        }

        /// <summary>
        /// Directs the download of a video and audio separately of a single Youtube video.
        /// </summary>
        private async Task DownloadVideoAndAudioAsync(IStreamInfo videoStreamInfo, IStreamInfo audioStreamInfo, string videoPath, string audioPath)
        {
            _dialogService.UpdateStatus("(3/6) Pobieranie wideo...");
            long totalSizeVideo = videoStreamInfo.Size.Bytes;
            DateTime lastUpdateTime = DateTime.Now.AddMilliseconds(-500);
            TimeSpan updateInterval = TimeSpan.FromMilliseconds(500);

            IProgress<double> progressVideo = new Progress<double>(percent =>
            {
                if (DateTime.Now - lastUpdateTime >= updateInterval)
                {
                    double downloadedMB = (percent * totalSizeVideo) / (1024.0 * 1024.0);
                    double totalMB = totalSizeVideo / (1024.0 * 1024.0);

                    var statusMessage = $"(3/6) Pobieranie wideo ({downloadedMB:F2} MB / {totalMB:F2} MB, {percent:P})";
                    _ = DispatcherQueue.GetForCurrentThread().TryEnqueue(() => _dialogService.UpdateStatus(statusMessage));

                    lastUpdateTime = DateTime.Now;
                }
            });

            await _videoService.DownloadStreamAsync(videoStreamInfo, videoPath, progressVideo);

            long totalSizeAudio = audioStreamInfo.Size.Bytes;
            lastUpdateTime = DateTime.Now.AddMilliseconds(-500);

            bool isDownloadingComplete = false;

            IProgress<double> progressAudio = new Progress<double>(percent =>
            {
                if (isDownloadingComplete)
                {
                    return;
                }

                if (DateTime.Now - lastUpdateTime >= updateInterval)
                {
                    double downloadedMB = (percent * totalSizeAudio) / (1024.0 * 1024.0);
                    double totalMB = totalSizeAudio / (1024.0 * 1024.0);

                    var statusMessage = $"(4/6) Pobieranie audio ({downloadedMB:F2} MB / {totalMB:F2} MB, {percent:P})";
                    _ = DispatcherQueue.GetForCurrentThread().TryEnqueue(() => _dialogService.UpdateStatus(statusMessage));

                    lastUpdateTime = DateTime.Now;
                }
            });

            _dialogService.UpdateStatus("(4/6) Pobieranie audio...");
            await _videoService.DownloadStreamAsync(audioStreamInfo, audioPath, progressAudio);
            isDownloadingComplete = true;
        }

        /// <summary>
        /// Deletes temporary video and audio files that we downloaded.
        /// </summary>
        private void CleanUpTemporaryFiles(string videoPath, string audioPath)
        {
            if (File.Exists(videoPath)) File.Delete(videoPath);
            if (File.Exists(audioPath)) File.Delete(audioPath);
        }

        /// <summary>
        /// Downloads a TVP episode to a MP4 file from a link.
        /// </summary>
        public async Task TVPLinkToMp4(string link)
        {
            if (!link.StartsWith("https://vod.tvp.pl"))
            {
                _dialogService.ShowMessage("Link wideo z vod TVP niepoprawny", "Błąd");
                return;
            }

            var mpdLink = await HtmlService.GetMpdLinkFromUserUrl(link);

            if (mpdLink == null)
            {
                _dialogService.ShowMessage("Link wideo z vod TVP niepoprawny", "Błąd");
                return;
            }

            var title = "Film " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string outputPath = Path.Combine(desktopPath, $"{title}.mp4");

            _dialogService.ShowLoadingDialogAsync();

            IProgress<string> progress = new Progress<string>(statusMessage =>
            {
                _ = DispatcherQueue.GetForCurrentThread().TryEnqueue(() => _dialogService.UpdateStatus(statusMessage));
            });

            _dialogService.UpdateStatus("Pobieranie wideo...");
            await FFmpegService.DownloadVideoAsync(mpdLink, outputPath, progress);

            _dialogService.CloseLoadingDialog();
            var message = $"Pomyślnie utworzono nowy plik MP4 na pulpicie o nazwie: ";
            _dialogService.ShowSuccessfulMessage(message, title);
        }
    }
}