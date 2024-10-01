using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace YoutubeConverter.Services
{
    internal class FFmpegService
    {
        private static string _ffmpegPath = Path.Combine(AppContext.BaseDirectory, "FFmpeg", "bin", "ffmpeg.exe");

        /// <summary>
        /// Creates a process that uses ffmpeg.exe to merge mp4 and mp3 files
        /// into one file.
        /// </summary>
        public static async Task MergeVideoAndAudioAsync(string videoPath, string audioPath, string outputPath)
        {
            var ffmpegArgs = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -strict experimental \"{outputPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            await process.WaitForExitAsync();
        }

        /// <summary>
        /// Creates a process that uses ffmpeg.exe to download a stream
        /// from mpd link to a file.
        /// </summary>
        public static async Task DownloadVideoAsync(string mpdLink, string outputPath, IProgress<string> progress = null)
        {
            var ffmpegArgs = $"-i \"{mpdLink}\" -c copy \"{outputPath}\"";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _ffmpegPath,
                    Arguments = ffmpegArgs,
                    RedirectStandardOutput = false,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            var errorTask = ReadStreamAsync(process.StandardError, progress);

            await process.WaitForExitAsync();
        }

        /// <summary>
        /// Reads error output from the ffmpeg process that we then use
        /// to pass progress info about the download to the UI.
        /// </summary>
        private static async Task ReadStreamAsync(StreamReader streamReader, IProgress<string> progress)
        {
            string line;
            double totalDuration = 0;
            double downloadedDuration = 0;
            DateTime startTime = DateTime.Now;

            while ((line = await streamReader.ReadLineAsync()) != null)
            {
                if (line.Contains("Duration"))
                {
                    var durationStr = line.Substring(line.IndexOf("Duration: ") + 10, 11);
                    if (TimeSpan.TryParseExact(durationStr, "hh\\:mm\\:ss\\.ff", null, out var duration))
                    {
                        totalDuration = duration.TotalSeconds;
                    }
                }

                if (line.Contains("time="))
                {
                    var timeStr = line.Substring(line.IndexOf("time=") + 5, 11);
                    if (TimeSpan.TryParseExact(timeStr, "hh\\:mm\\:ss\\.ff", null, out var currentTime))
                    {
                        downloadedDuration = currentTime.TotalSeconds;

                        if (totalDuration > 0)
                        {
                            double percentComplete = downloadedDuration / totalDuration;
                            var elapsedTime = DateTime.Now - startTime;
                            double elapsedTimeSeconds = elapsedTime.TotalSeconds;

                            double downloadSpeed = downloadedDuration / elapsedTimeSeconds;
                            double remainingDuration = totalDuration - downloadedDuration;
                            double estimatedRemainingTime = remainingDuration / downloadSpeed;

                            var remainingTime = TimeSpan.FromSeconds(estimatedRemainingTime).ToString(@"hh\:mm\:ss");
                            var statusMessage = $"Pobieranie wideo ({percentComplete:P}) - Pozostało: {remainingTime}";

                            progress?.Report(statusMessage);
                        }
                    }
                }
            }
        }
    }
}