using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Graphics.Display;
using Windows.UI.Input;
using Microsoft.Xna.Framework.Audio;

using static MangoTube8.YouTubeModal;

namespace MangoTube8
{
    public partial class VideoPage : PhoneApplicationPage
    {

        private DispatcherTimer hideControlsTimer;
        private DispatcherTimer syncTimer;
        private DispatcherTimer timer;
        private HttpClient _httpClient;
        private bool isFullscreen = false;
        private bool isUsingSeparateAudio = false;
        private DateTime _lastTapTime = DateTime.MinValue;
        private string continuationTokenNextComments = string.Empty;
        private const double DoubleTapThreshold = 500;
        private string localVideoId;
        private bool IsLoadedForPivotComments = false;
        private bool IsLoadedForPivotRealted = false;
        private bool IsLoadedForPivotDetails = false;

        public VideoPage()
        {
            InitializeComponent();

            SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;

            hideControlsTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(7.5)
            };

            hideControlsTimer.Tick += HideControlsTimer_Tick;

            VideoPlayer.CurrentStateChanged += VideoPlayer_CurrentStateChanged;

            ShowControls();

            this.OrientationChanged += VideoPage_OrientationChanged;

            syncTimer = new DispatcherTimer();
            syncTimer.Interval = TimeSpan.FromMilliseconds(250);
            syncTimer.Tick += SyncAudioWithVideo;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36");

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext != null)
            {
                Debug.WriteLine("NavigationContext is not null.");

                BubbleLoadingAnimationPlayer.Begin();
                LoadingPanelPlayer.Visibility = Visibility.Visible;

                if (NavigationContext.QueryString.ContainsKey("videoId"))
                {
                    string videoId = NavigationContext.QueryString["videoId"];
                    Debug.WriteLine("Received videoId: " + videoId);

                    if (string.IsNullOrEmpty(videoId))
                    {
                        Debug.WriteLine("videoId is empty or null.");
                    }
                    else
                    {
                        Debug.WriteLine("Valid videoId received: " + videoId);
                        localVideoId = videoId;
   
                        PopulateVideoStream("Mediuim");
                    }
                }
                else
                {
                    Debug.WriteLine("videoId parameter is missing in the query string.");
                }
            }
            else
            {
                Debug.WriteLine("NavigationContext is null.");
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            StartTimeText.Text = VideoPlayer.Position.ToString(@"h\:mm\:ss");
            ProgressSlider.Value = VideoPlayer.Position.TotalSeconds;
        }

        private void SyncAudioWithVideo(object sender, object e)
        {

            if (isUsingSeparateAudio)
            {

                double positionDifference = Math.Abs(VideoPlayer.Position.TotalSeconds - AudioPlayer.Position.TotalSeconds);

                if (positionDifference > 0.1)
                {
                    AudioPlayer.Position = VideoPlayer.Position;
                    System.Diagnostics.Debug.WriteLine($"Sync correction applied. Audio Player position adjusted to {AudioPlayer.Position} to match Video Player.");
                }

                if (AudioPlayer.CurrentState != MediaElementState.Playing)
                {
                    AudioPlayer.Play();
                    System.Diagnostics.Debug.WriteLine("Audio Player started playing.");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not using separate audio, skipping sync.");
            }
        }

        private void SyncAudioIfNeeded()
        {
            if (isUsingSeparateAudio)
            {
                AudioPlayer.Position = VideoPlayer.Position;
            }
        }

        private void LoadMedia(string videoSource, string audioSource = null)
        {
            Debug.WriteLine($"Loading media: Video Source = {videoSource}, Audio Source = {audioSource}");

            VideoPlayer.Source = new Uri(videoSource);
            Debug.WriteLine($"Video source set: {videoSource}");

            if (audioSource != null)
            {
                isUsingSeparateAudio = true;
                AudioPlayer.Source = new Uri(audioSource);
                Debug.WriteLine($"Audio source set: {audioSource}");
            }
            else
            {
                isUsingSeparateAudio = false;
                Debug.WriteLine("No audio source provided, using video audio.");
            }
        }


        private void VideoPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            Debug.WriteLine("Orientation Changed: " + e.Orientation.ToString());

            if (isFullscreen)
            {

                return;
            }

            if (e.Orientation == PageOrientation.Landscape ||
                e.Orientation == PageOrientation.LandscapeLeft ||
                e.Orientation == PageOrientation.LandscapeRight)
            {
                SetLandscapeMode();
            }
            else
            {
                SetPortraitMode();
            }
        }

        private void SetLandscapeMode()
        {
            Debug.WriteLine("Orientation: Landscape");

            VideoPivot.Visibility = Visibility.Collapsed;
            Header.Visibility = Visibility.Collapsed;

            VideoPlayer.Width = Application.Current.Host.Content.ActualHeight;
            VideoPlayer.Height = Application.Current.Host.Content.ActualWidth;

            VideoPlayer.MinHeight = 0;
            VideoPlayer.MinWidth = 0;
            VideoPlayer.MaxHeight = Double.PositiveInfinity;
            VideoPlayer.MaxWidth = Double.PositiveInfinity;

            VideoPlayer.Stretch = Stretch.Fill;

            BitmapImage fullScreenButMap = new BitmapImage();
            fullScreenButMap.UriSource = new Uri("Assets/FullScreenShrink.png", UriKind.Relative);
            FullScreenIcon.Source = fullScreenButMap;

            VideoPlayer.Visibility = Visibility.Visible;

            Debug.WriteLine("Video Player Set to Landscape Mode: Width = " + VideoPlayer.Width + ", Height = " + VideoPlayer.Height);
        }

        private void SetPortraitMode()
        {
            Debug.WriteLine("Orientation: Portrait");

            VideoPivot.Visibility = Visibility.Visible;
            Header.Visibility = Visibility.Visible;

            VideoPlayer.Width = Double.NaN;
            VideoPlayer.Height = Double.NaN;

            VideoPlayer.MinHeight = 150;
            VideoPlayer.MaxHeight = 350;

            VideoPlayer.Stretch = Stretch.UniformToFill;

            BitmapImage fullScreenButMap = new BitmapImage();
            fullScreenButMap.UriSource = new Uri("Assets/FullScreenExpend.png", UriKind.Relative);
            FullScreenIcon.Source = fullScreenButMap;

            VideoPlayer.Visibility = Visibility.Visible;

            Debug.WriteLine("Video Player Set to Portrait Mode: MinHeight = " + VideoPlayer.MinHeight + ", MaxHeight = " + VideoPlayer.MaxHeight);
        }

        private void VideoPlayer_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (IconAndStuff.Visibility == Visibility.Visible && IconAndStuffBackground.Visibility == Visibility.Visible)
            {
                HideControls();
            }
            else
            {
                ShowControls();
            }
        }

        private void ShowControls()
        {
            ShowControlsAnimation.Begin();
            IconAndStuff.Visibility = Visibility.Visible;
            IconAndStuffBackground.Visibility = Visibility.Visible;
            hideControlsTimer.Start();
        }

        private void HideControls()
        {
            HideControlsAnimation.Begin();
            IconAndStuff.Visibility = Visibility.Collapsed;
            IconAndStuffBackground.Visibility = Visibility.Collapsed;
            hideControlsTimer.Stop();
        }

        private void HideControlsTimer_Tick(object sender, EventArgs e)
        {
            HideControls();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {

            if (VideoPlayer.CurrentState == MediaElementState.Playing)
            {

                VideoPlayer.Pause();
                if (isUsingSeparateAudio) AudioPlayer.Pause();
                PlayPauseIcon.Source = new BitmapImage(new Uri("Assets/PlayIcon.png", UriKind.Relative));

            }
            else
            {

                VideoPlayer.Play();
                if (isUsingSeparateAudio) AudioPlayer.Play();
                PlayPauseIcon.Source = new BitmapImage(new Uri("Assets/PauseIcon.png", UriKind.Relative));

            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SearchTextBox.Visibility == Visibility.Collapsed)
            {
                ShowSearchBox.Begin();
            }
            else
            {
                HideSearchBox.Begin();
            }
        }

        private void HideSearchBox_Completed(object sender, EventArgs e)
        {
            YouTubeLogo.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Collapsed;
        }

        private void VideoPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            if (VideoPlayer.CurrentState == MediaElementState.Playing)
            {
                ProgressSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;

                CompositionTarget.Rendering += CompositionTarget_Rendering;
            }
            else
            {
                CompositionTarget.Rendering -= CompositionTarget_Rendering;
            }
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            if (VideoPlayer.NaturalDuration.HasTimeSpan)
            {
                ProgressSlider.Value = VideoPlayer.Position.TotalSeconds;
            }
        }

        private void ProgressSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Math.Abs(VideoPlayer.Position.TotalSeconds - ProgressSlider.Value) > 0.1)
            {
                VideoPlayer.Position = TimeSpan.FromSeconds(ProgressSlider.Value);
            }
        }

        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            ProgressSlider.Maximum = VideoPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            ProgressSlider.Value = VideoPlayer.Position.TotalSeconds;

            StartTimeText.Text = TimeSpan.Zero.ToString(@"h\:mm\:ss");
            EndTimeText.Text = VideoPlayer.NaturalDuration.TimeSpan.ToString(@"h\:mm\:ss");

            timer.Start();

            if (isUsingSeparateAudio)
            {
                AudioPlayer.Position = VideoPlayer.Position;
                AudioPlayer.Play();
                syncTimer.Start();
            }
            else
            {
                syncTimer.Stop();
            }

        }

        private void VideoPlayer_MediaPaused(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        private void VideoPlayer_MediaPlay(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_Rendering;
            StartTimeText.Text = TimeSpan.Zero.ToString(@"h\:mm\:ss");
            ProgressSlider.Value = 0;
        }

        private void AudioPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (isUsingSeparateAudio)
            {
                AudioPlayer.Stop();
                syncTimer.Stop();
            }
        }

        private void LeftArea_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            DateTime currentTime = DateTime.Now;

            if ((currentTime - _lastTapTime).TotalMilliseconds <= DoubleTapThreshold)
            {

                TimeSpan newVideoPosition = VideoPlayer.Position - TimeSpan.FromSeconds(5);
                VideoPlayer.Position = newVideoPosition;

                if (isUsingSeparateAudio && AudioPlayer != null)
                {
                    AudioPlayer.Position = newVideoPosition;
                }

                ShowSkipMessage("-5s", true);
            }

            _lastTapTime = currentTime;
        }

        private void RightArea_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

            DateTime currentTime = DateTime.Now;

            if ((currentTime - _lastTapTime).TotalMilliseconds <= DoubleTapThreshold)
            {

                TimeSpan newVideoPosition = VideoPlayer.Position + TimeSpan.FromSeconds(5);
                VideoPlayer.Position = newVideoPosition;

                if (isUsingSeparateAudio && AudioPlayer != null)
                {
                    AudioPlayer.Position = newVideoPosition;
                }

                ShowSkipMessage("+5s", false);
            }

            _lastTapTime = currentTime;
        }

        private void ShowSkipMessage(string message, bool isNegative)
        {

            SkipTimeTextMinus.Visibility = Visibility.Collapsed;
            SkipTimeTextPlus.Visibility = Visibility.Collapsed;

            if (isNegative)
            {
                SkipTimeTextMinus.Text = message;
                SkipTimeTextMinus.Visibility = Visibility.Visible;
            }
            else
            {
                SkipTimeTextPlus.Text = message;
                SkipTimeTextPlus.Visibility = Visibility.Visible;
            }

            var fadeOutAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(1))
            };

            var storyboard = new Storyboard();
            storyboard.Children.Add(fadeOutAnimation);

            Storyboard.SetTarget(fadeOutAnimation, isNegative ? SkipTimeTextMinus : SkipTimeTextPlus);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath("Opacity"));

            storyboard.Completed += (s, e) =>
            {
                if (isNegative)
                {
                    SkipTimeTextMinus.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SkipTimeTextPlus.Visibility = Visibility.Collapsed;
                }
            };

            storyboard.Begin();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string searchText = SearchTextBox.Text;
                Debug.WriteLine("Search Text: " + searchText);

                NavigationService.Navigate(new Uri("/SearchPage.xaml?query=" + Uri.EscapeDataString(searchText), UriKind.Relative));
            }
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (isFullscreen)
            {

                isFullscreen = false;

                SupportedOrientations = SupportedPageOrientation.PortraitOrLandscape;

                SetPortraitMode();

                if (Orientation == PageOrientation.Landscape || Orientation == PageOrientation.LandscapeLeft || Orientation == PageOrientation.LandscapeRight)
                {

                    SetPortraitMode();
                }
            }
            else
            {

                isFullscreen = true;

                SupportedOrientations = SupportedPageOrientation.Landscape;

                SetLandscapeMode();
            }
        }

        private Task<string> FetchVideoStreamsMain(string videoId)
        {
            return Task.Run(async () =>
            {
                try
                {

                    string videoUrl = $"https://www.youtube.com/youtubei/v1/player?key={Settings.InnerTubeApiKey}";

                    DateTime currentUtcDateTime = DateTime.UtcNow;
                    long signatureTimestamp = (long)(currentUtcDateTime - new DateTime(1970, 1, 1)).TotalSeconds;

                    var contextData = new
                    {
                        videoId = videoId,
                        context = new
                        {
                            client = new
                            {
                                hl = "en",
                                gl = "US",
                                clientName = "ANDROID_VR",
                                clientVersion = "1.57.29",
                                deviceMake = "Oculus",
                                deviceModel = "Quest 3",
                                androidSdkVersion = "32",
                                userAgent = "com.google.android.apps.youtube.vr.oculus/1.57.29 (Linux; U; Android 12L; eureka-user Build/SQ3A.220605.009.A1) gzip",
                                osName = "Android",
                                osVersion = "12L"
                            }
                        },
                        playbackContext = new
                        {
                            contentPlaybackContext = new
                            {
                                signatureTimestamp = signatureTimestamp
                            }
                        }
                    };

                    string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(contextData);
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage videoResponse = await _httpClient.PostAsync(videoUrl, content);
                    videoResponse.EnsureSuccessStatusCode();
                    string videoStreamJson = await videoResponse.Content.ReadAsStringAsync();

                    return videoStreamJson;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching video stream: {ex.Message}");
                    return null;
                }
            });
        }

        private Task<string> FetchVideoStreamsBackup(string videoId)
        {
            return Task.Run(async () =>
            {
                try
                {

                    string videoUrl = $"https://www.youtube.com/youtubei/v1/player?key={Settings.InnerTubeApiKey}";

                    DateTime currentUtcDateTime = DateTime.UtcNow;
                    long signatureTimestamp = (long)(currentUtcDateTime - new DateTime(1970, 1, 1)).TotalSeconds;

                    var contextData = new
                    {
                        videoId = videoId,
                        context = new
                        {
                            client = new
                            {
                                hl = "en",
                                gl = "US",
                                clientName = "IOS",
                                clientVersion = "19.29.1",
                                deviceMake = "Apple",
                                deviceModel = "iPhone",
                                osName = "iOS",
                                userAgent = "com.google.ios.youtube/19.29.1 (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X;)",
                                osVersion = "17.5.1.21F90"
                            }
                        },
                        playbackContext = new
                        {
                            contentPlaybackContext = new
                            {
                                signatureTimestamp = signatureTimestamp
                            }
                        }
                    };

                    string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(contextData);
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage videoResponse = await _httpClient.PostAsync(videoUrl, content);
                    videoResponse.EnsureSuccessStatusCode();
                    string videoStreamJson = await videoResponse.Content.ReadAsStringAsync();

                    return videoStreamJson;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching video stream: {ex.Message}");
                    return null;
                }
            });
        }

        private async void PopulateVideoStream(string videoQuality)
        {
            try
            {
                string videoStreamJson = await FetchVideoStreamsMain(localVideoId);
                bool isBackup = false;

                if (string.IsNullOrEmpty(videoStreamJson))
                {
                    Debug.WriteLine("Main video stream is unplayable or failed, trying backup...");
                    videoStreamJson = await FetchVideoStreamsBackup(localVideoId);
                    isBackup = true;
                }

                if (string.IsNullOrEmpty(videoStreamJson))
                {
                    Debug.WriteLine("Failed to retrieve video stream JSON.");
                    return;
                }

                Debug.WriteLine("Video Stream JSON: " + videoStreamJson);

                if (videoStreamJson != null)
                {
                    var selectedFormat = SelectFormatBasedOnQuality(videoStreamJson, videoQuality);
                    if (selectedFormat == null)
                    {
                        Debug.WriteLine($"No format found for {videoQuality}, defaulting to Medium quality.");
                        selectedFormat = SelectFormatBasedOnQuality(videoStreamJson, "Medium");
                    }

                    if (selectedFormat != null && selectedFormat.Itag == 18)
                    {
                        Debug.WriteLine($"Selected video format is medium quality (itag 18). Skipping audio selection.");
                        LoadMedia(selectedFormat.Url);
                    }
                    else
                    {
                        var selectedAudio = SelectFormatBasedOnQuality(videoStreamJson, "Audio");

                        if (selectedAudio != null)
                        {
                            LoadMedia(selectedFormat.Url, selectedAudio.Url);
                            Debug.WriteLine($"Selected audio: {selectedAudio.Itag} - {selectedAudio.QualityLabel} - {selectedAudio.MimeType}");
                        }
                        else
                        {
                            Debug.WriteLine("No suitable audio format found.");
                        }
                    }

                    if (selectedFormat != null)
                    {
                        Debug.WriteLine($"Selected video format: {selectedFormat.Itag} - {selectedFormat.QualityLabel} - {selectedFormat.MimeType}");
                    }
                    else
                    {
                        Debug.WriteLine("No suitable video format found.");
                    }
                }
                else
                {
                    Debug.WriteLine("Failed to deserialize video stream JSON.");
                }

                if (isBackup)
                {
                    Debug.WriteLine("This is a backup video stream.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PopulateVideoStream for video ID {localVideoId}: {ex.Message}");
            }
            finally
            {
                BubbleLoadingAnimationPlayer.Stop();
                LoadingPanelPlayer.Visibility = Visibility.Collapsed;
            }
        }

        private bool IsVideoUnplayable(string videoStreamJson)
        {
            try
            {
                var jsonObject = JsonConvert.DeserializeObject<JObject>(videoStreamJson);
                var playabilityStatus = jsonObject?["playabilityStatus"]?["status"]?.ToString();
                return playabilityStatus == "UNPLAYABLE";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking playability: {ex.Message}");
                return false;
            }
        }

        private VideoStreamFormat SelectFormatBasedOnQuality(string streamingDataJson, string videoQuality)
        {
            Debug.WriteLine($"SelectFormatBasedOnQuality called with videoQuality: {videoQuality}");

            JObject streamingData = JObject.Parse(streamingDataJson);
            var adaptiveFormats = streamingData.SelectToken("..adaptiveFormats");
            var formats = streamingData.SelectToken("..formats");


            if (adaptiveFormats != null && adaptiveFormats.HasValues)
            {
                Debug.WriteLine($"Available adaptive formats: {adaptiveFormats.Count()}");

                if (videoQuality == "Medium")
                {
                    Debug.WriteLine("Looking for Medium quality format...");

                    var mediumQualityFormat = formats.FirstOrDefault(f => f["itag"].ToString() == "18");

                    if (mediumQualityFormat != null)
                    {
                        Debug.WriteLine("Medium quality format found.");
                        Debug.WriteLine($"URL: {mediumQualityFormat["url"]}");
                        return mediumQualityFormat.ToObject<VideoStreamFormat>();
                    }
                    else
                    {
                        Debug.WriteLine("itag 18 not found, looking for itag 134 as backup...");
                        var backupFormat = adaptiveFormats.FirstOrDefault(f => f["itag"].ToString() == "134");
                        if (backupFormat != null)
                        {
                            Debug.WriteLine("Backup format found.");
                            Debug.WriteLine($"URL: {backupFormat["url"]}");
                        }
                        return backupFormat.ToObject<VideoStreamFormat>();
                    }
                }
                else if (videoQuality == "HD")
                {
                    Debug.WriteLine("Looking for HD quality format...");

                    var hdFormat = adaptiveFormats.FirstOrDefault(f => f["quality"].ToString() == "hd720" && f["mimeType"].ToString().Contains("avc1"));
                    if (hdFormat != null)
                    {
                        Debug.WriteLine("HD format found.");
                        Debug.WriteLine($"URL: {hdFormat["url"]}");
                        return hdFormat.ToObject<VideoStreamFormat>();
                    }
                    else
                    {
                        var qualityLabels = new[] { "480p", "360p", "240p", "144p" };
                        foreach (var label in qualityLabels)
                        {
                            Debug.WriteLine($"Looking for {label} format...");
                            var fallbackFormat = adaptiveFormats.FirstOrDefault(f => f["qualityLabel"].ToString() == label);
                            if (fallbackFormat != null)
                            {
                                Debug.WriteLine($"{label} format found.");
                                Debug.WriteLine($"URL: {fallbackFormat["url"]}");
                                return fallbackFormat.ToObject<VideoStreamFormat>();
                            }
                        }

                        Debug.WriteLine("No specific quality found, trying to match ranges...");

                        var possible720Format = adaptiveFormats.FirstOrDefault(f => (int)f["width"] >= 600 && (int)f["width"] <= 800);
                        if (possible720Format != null)
                        {
                            Debug.WriteLine("Found a 720p-like format.");
                            Debug.WriteLine($"URL: {possible720Format["url"]}");
                            return possible720Format.ToObject<VideoStreamFormat>();
                        }

                        var possible480Format = adaptiveFormats.FirstOrDefault(f => (int)f["width"] >= 400 && (int)f["width"] <= 599);
                        if (possible480Format != null)
                        {
                            Debug.WriteLine("Found a 480p-like format.");
                            Debug.WriteLine($"URL: {possible480Format["url"]}");
                            return possible480Format.ToObject<VideoStreamFormat>();
                        }

                        var possible360Format = adaptiveFormats.FirstOrDefault(f => (int)f["width"] >= 300 && (int)f["width"] <= 399);
                        if (possible360Format != null)
                        {
                            Debug.WriteLine("Found a 360p-like format.");
                            Debug.WriteLine($"URL: {possible360Format["url"]}");
                            return possible360Format.ToObject<VideoStreamFormat>();
                        }

                        var possible240Format = adaptiveFormats.FirstOrDefault(f => (int)f["width"] >= 200 && (int)f["width"] <= 299);
                        if (possible240Format != null)
                        {
                            Debug.WriteLine("Found a 240p-like format.");
                            Debug.WriteLine($"URL: {possible240Format["url"]}");
                            return possible240Format.ToObject<VideoStreamFormat>();
                        }

                        var possible144Format = adaptiveFormats.FirstOrDefault(f => (int)f["width"] >= 100 && (int)f["width"] <= 199);
                        if (possible144Format != null)
                        {
                            Debug.WriteLine("Found a 144p-like format.");
                            Debug.WriteLine($"URL: {possible144Format["url"]}");
                            return possible144Format.ToObject<VideoStreamFormat>();
                        }
                    }
                }

                if (videoQuality == "Audio")
                {
                    Debug.WriteLine("Looking for Audio format...");

                    var audioFormat = adaptiveFormats.FirstOrDefault(f => f["itag"].ToString() == "140");
                    if (audioFormat != null)
                    {
                        Debug.WriteLine("Audio format found.");
                        Debug.WriteLine($"URL: {audioFormat["url"]}");
                        return audioFormat.ToObject<VideoStreamFormat>();
                    }
                }
            }

            Debug.WriteLine("No specific format found, defaulting to Medium quality.");
            var defaultFormat = formats.FirstOrDefault(f => f["itag"].ToString() == "18") ?? adaptiveFormats.FirstOrDefault(f => f["itag"].ToString() == "134");
            if (defaultFormat != null)
            {
                Debug.WriteLine($"URL: {defaultFormat["url"]}");
            }
            return defaultFormat.ToObject<VideoStreamFormat>();
        }

        private async Task PopulateRelatedVideos(string videoId)
        {
            string response = await FetchRealtedVideos(videoId);

            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    var videos = new List<RealtedVideoDetail>();
                    var jsonResponse = JObject.Parse(response);

                    var videoItems = jsonResponse.SelectTokens("$..compactVideoRenderer").Where(x => x is JObject).ToList();

                    if (videoItems != null)
                    {
                        foreach (var item in videoItems)
                        {

                            var spotlightTitle = item.SelectToken("title.simpleText")?.ToString();
                            var spotlightVideoId = item.SelectToken("videoId")?.ToString();
                            var thumbnailUrl = item.SelectToken("thumbnail.thumbnails[1].url")?.ToString();
                            var views = item.SelectToken("viewCountText.simpleText")?.ToString();
                            var date = item.SelectToken("publishedTimeText.simpleText")?.ToString();
                            var author = item.SelectToken("longBylineText.runs[0].text")?.ToString();
                            var lengthText = item.SelectToken("lengthText.simpleText")?.ToString();
                            var authorPFP = item.SelectToken("channelThumbnail.thumbnails[0].url")?.ToString();

                            if (string.IsNullOrEmpty(lengthText))
                            {
                                var lengthLabel = item.SelectToken("lengthText.accessibility.accessibilityData.label")?.ToString();

                                if (!string.IsNullOrEmpty(lengthLabel))
                                {
                                    var timeMatch = System.Text.RegularExpressions.Regex.Match(lengthLabel, @"(\d+)\s+minutes?[, ]*(\d+)?\s*seconds?");
                                    if (timeMatch.Success)
                                    {
                                        int minutes = int.Parse(timeMatch.Groups[1].Value);
                                        int seconds = timeMatch.Groups[2].Success ? int.Parse(timeMatch.Groups[2].Value) : 0;

                                        lengthText = new TimeSpan(0, minutes, seconds).ToString(@"mm\:ss");
                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(spotlightTitle) && !string.IsNullOrEmpty(spotlightVideoId))
                            {
                                var video = new RealtedVideoDetail
                                {
                                    Title = spotlightTitle,
                                    Author = author,
                                    VideoId = spotlightVideoId,
                                    Thumbnail = thumbnailUrl,
                                    Views = views,
                                    Date = date,
                                    Length = lengthText
                                };

                                AddVideoCard(video, RealtedVideoItemControl);
                                videos.Add(video);
                            }
                        }

                        RealtedVideoItemControl.ItemsSource = videos;
                    }
                    else
                    {
                        Debug.WriteLine("No related videos found in the response.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing response: {ex.Message}");
                }
                finally
                {
                    IsLoadedForPivotRealted = true;
                    LoadMore.Visibility = Visibility.Visible;
                    BubbleLoadingAnimationRel.Stop();
                    LoadingPanelRel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void AddVideoCard(RealtedVideoDetail video, ItemsControl itemsControl)
        {
            var videoCard = CreateVideoCard(video);
            itemsControl.Items.Add(videoCard);
        }

        private void AddCommentCard(VideoComment comment, ItemsControl itemsControl)
        {
            var commentCard = CreateCommentCard(comment);
            itemsControl.Items.Add(commentCard);
        }

        private async Task<string> FetchRealtedVideos(string videoId)
        {
            try
            {
                string url = $"https://www.youtube.com/youtubei/v1/next?key={Settings.InnerTubeApiKey}";

                var requestBody = new
                {
                    context = new
                    {
                        client = new
                        {
                            hl = "en",
                            gl = "US",
                            clientName = "WEB",
                            clientVersion = "2.20211122.09.00",
                            userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36",
                            osName = "Windows",
                            osVersion = "10",
                            platform = "DESKTOP"
                        }
                    },
                    videoId = videoId
                };

                string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                Debug.WriteLine("Request URL (Recommended): " + url);
                Debug.WriteLine("Request Body (Recommended): " + jsonRequestBody);

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("API Response (Recommended): " + responseBody);

                return responseBody;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching recommended videos: " + ex.Message);
                return string.Empty;
            }
        }

        private async Task<string> FetchCommentsPostContinued(string continuationToken)
        {
            try
            {
                string url = $"https://www.youtube.com/youtubei/v1/next?key={Settings.InnerTubeApiKey}";

                var continuationContextData = new
                {
                    context = new
                    {
                        client = new
                        {
                            hl = "en",
                            gl = "US",
                            clientName = "WEB",
                            clientVersion = "2.20211122.09.00",
                            osName = "Windows",
                            osVersion = "10",
                            platform = "DESKTOP"
                        }
                    },
                    continuation = continuationToken
                };

                string continuationRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(continuationContextData);
                var continuationContent = new StringContent(continuationRequestBody, Encoding.UTF8, "application/json");

                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36");
                }

                HttpResponseMessage response = await _httpClient.PostAsync(url, continuationContent);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Continued Comments API Response: " + responseBody);

                string _continuationItemRendererToken = ExtractContinuationToken(responseBody);

                if (!string.IsNullOrEmpty(_continuationItemRendererToken))
                {
                    Debug.WriteLine("Found continuation token: " + _continuationItemRendererToken);
                    continuationTokenNextComments = _continuationItemRendererToken;
                }
                else
                {
                    Debug.WriteLine("No continuation token found.");
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in FetchCommentsPostContinued: {ex.Message}");
                return string.Empty;
            }
        }

        private async void LoadMoreButton_Click(object sender, RoutedEventArgs e)
        {
            await PopulateComments(localVideoId);
        }

        private async Task PopulateVideoDetails(string videoId)
        {
            try
            {
                VideoAuthorDetails result = await FetchVideoAndAuthorDetails(videoId);

                if (string.IsNullOrEmpty(result.VideoDetailsJson))
                {
                    Debug.WriteLine("No video details found for video ID: " + videoId);
                    return;
                }

                if (string.IsNullOrEmpty(result.AuthorDetailsJson))
                {
                    Debug.WriteLine("No author details found for video ID: " + videoId);
                    return;
                }

                var videoJsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(result.VideoDetailsJson);
                var authorJsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(result.AuthorDetailsJson);


                Debug.WriteLine("Full  videoJsonResponse JSON: " + videoJsonResponse?.ToString());

                var videoDetailsSection = videoJsonResponse.SelectToken("videoDetails");
                Debug.WriteLine("Full videoDetails JSON section: " + videoDetailsSection?.ToString());

                var videoOwnerRendererSection = authorJsonResponse.SelectToken("..videoOwnerRenderer");
                Debug.WriteLine("Full videoOwnerRenderer JSON section: " + videoOwnerRendererSection?.ToString());

                var videoUploadDateSec = authorJsonResponse.SelectToken("..dateText");
                Debug.WriteLine("Full videoUploadDateSec JSON section: " + videoUploadDateSec?.ToString());

                string FormatedViewCountStart = videoJsonResponse.SelectToken("videoDetails.viewCount")?.ToString();

                string FormatedViewCountEnd;
                int parsedViewCount;

                if (int.TryParse(FormatedViewCountStart, out parsedViewCount))
                {

                    FormatedViewCountEnd = Utils.AddCommasManually(parsedViewCount);
                    Console.WriteLine(FormatedViewCountEnd);
                }
                else
                {

                    FormatedViewCountEnd = FormatedViewCountStart;
                    Console.WriteLine(FormatedViewCountEnd);
                }


                var videoDetailsTab = new VideoDetailsTab
                {
                    Title = videoJsonResponse.SelectToken("videoDetails.title")?.ToString(),              
                    Description = videoJsonResponse.SelectToken("videoDetails.shortDescription")?.ToString(),
                    ViewCount = FormatedViewCountEnd + " views",
                    UploadDate = authorJsonResponse
                        .SelectTokens("..dateText.simpleText")
                        .FirstOrDefault()?.ToString(),
                    Subcribers = authorJsonResponse
                        .SelectTokens("..subscriberCountText.simpleText")
                        .FirstOrDefault()?.ToString()
                };

                var authorDetails = new AuthorDetails
                {    
                    AvatarUrl = authorJsonResponse.SelectToken("..videoOwnerRenderer.thumbnail.thumbnails[1].url")?.ToString(),
                    Name = authorJsonResponse.SelectToken("..videoOwnerRenderer.title.runs[0].text")?.ToString()
                };

                VideoTitle.Text = authorDetails.Name;

                ProfilePicture.Source = new BitmapImage(new Uri(authorDetails.AvatarUrl, UriKind.Absolute));
                VideoUploadDate.Text = "Published on " + videoDetailsTab.UploadDate;

                SetVideoDescriptionWithLinks(videoDetailsTab.Description);

                VideoViews.Text = videoDetailsTab.ViewCount;
              
                SubscriptionStatus.Text = videoDetailsTab.Subcribers;

                Username.Text = authorDetails.Name;

                SetVideoDescriptionWithLinks(videoDetailsTab.Description);
                Debug.WriteLine($"Video Title: {videoDetailsTab.Title}");
                Debug.WriteLine($"View Count: {videoDetailsTab.ViewCount}");
                Debug.WriteLine($"Description: {videoDetailsTab.Description}");
                Debug.WriteLine($"Upload Date: {videoDetailsTab.UploadDate}");

                Debug.WriteLine($"Subbed {videoDetailsTab.Subcribers}");

                Debug.WriteLine($"Name {authorDetails.Name}"); 

                Debug.WriteLine($"Author Avatar URL: {authorDetails.AvatarUrl}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PopulateVideoDetails for video ID {videoId}: {ex.Message}");
            }
            finally
            {
                IsLoadedForPivotDetails = true;
                StuffPanel.Visibility = Visibility.Visible;
                BubbleLoadingAnimationDel.Stop();
                LoadingPanelDel.Visibility = Visibility.Collapsed;

            }
        }

        private void SetVideoDescriptionWithLinks(string description)
        {
            VideoDescription.Blocks.Clear();

            var paragraph = new Paragraph();

            string pattern = @"(http://[^\s]+|https://[^\s]+)";
            var matches = Regex.Matches(description, pattern);

            int lastIndex = 0;
            foreach (Match match in matches)
            {

                if (match.Index > lastIndex)
                {
                    paragraph.Inlines.Add(new Run { Text = description.Substring(lastIndex, match.Index - lastIndex) });
                }

                string url = match.Value;

                if (url.StartsWith("http://") || url.StartsWith("https://"))
                {
                    var hyperlink = new Hyperlink
                    {
                        NavigateUri = new Uri(url, UriKind.Absolute),
                        Foreground = new SolidColorBrush(Color.FromArgb(255, 130, 180, 220))
                    };

                    hyperlink.Inlines.Add(new Run { Text = url });

                    hyperlink.Click += async (sender, args) =>
                    {
                        try
                        {
                            if (Uri.IsWellFormedUriString(hyperlink.NavigateUri.ToString(), UriKind.Absolute))
                            {

                                await Windows.System.Launcher.LaunchUriAsync(hyperlink.NavigateUri);
                            }
                            else
                            {
                                Debug.WriteLine("Invalid URI: " + hyperlink.NavigateUri.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Error launching URI: " + ex.Message);
                        }
                    };

                    paragraph.Inlines.Add(hyperlink);
                }
                else
                {

                    paragraph.Inlines.Add(new Run { Text = url });
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < description.Length)
            {
                paragraph.Inlines.Add(new Run { Text = description.Substring(lastIndex) });
            }

            VideoDescription.Blocks.Add(paragraph);
        }

        private Task<VideoAuthorDetails> FetchVideoAndAuthorDetails(string videoId)
        {
            return Task.Run(async () =>
            {
                try
                {
                    string videoUrl = $"https://www.youtube.com/youtubei/v1/player?key={Settings.InnerTubeApiKey}";

                    DateTime currentUtcDateTime = DateTime.UtcNow;
                    long signatureTimestamp = (long)(currentUtcDateTime - new DateTime(1970, 1, 1)).TotalSeconds;

                    var contextData = new
                    {
                        videoId = videoId,
                        context = new
                        {
                            client = new
                            {
                                hl = "en",
                                gl = "US",
                                clientName = "IOS",
                                clientVersion = "19.29.1",
                                deviceMake = "Apple",
                                deviceModel = "iPhone",
                                osName = "iOS",
                                userAgent = "com.google.ios.youtube/19.29.1 (iPhone16,2; U; CPU iOS 17_5_1 like Mac OS X;)",
                                osVersion = "17.5.1.21F90"
                            }
                        },
                        playbackContext = new
                        {
                            contentPlaybackContext = new
                            {
                                signatureTimestamp = signatureTimestamp
                            }
                        }
                    };

                    string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(contextData);
                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage videoResponse = await _httpClient.PostAsync(videoUrl, content);
                    videoResponse.EnsureSuccessStatusCode();
                    string videoDetailsJson = await videoResponse.Content.ReadAsStringAsync();

                    string authorUrl = $"https://www.youtube.com/youtubei/v1/next?key={Settings.InnerTubeApiKey}";
                    var authorContextData = new
                    {
                        videoId = videoId,
                        context = new
                        {
                            client = new
                            {
                                hl = "en",
                                gl = "US",
                                clientName = "WEB",
                                clientVersion = "2.20211122.09.00",
                                osName = "Windows",
                                osVersion = "10",
                                platform = "DESKTOP"
                            }
                        }
                    };

                    string authorRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(authorContextData);
                    var authorContent = new StringContent(authorRequestBody, Encoding.UTF8, "application/json");

                    HttpResponseMessage authorResponse = await _httpClient.PostAsync(authorUrl, authorContent);
                    authorResponse.EnsureSuccessStatusCode();
                    string authorDetailsJson = await authorResponse.Content.ReadAsStringAsync();

                    return new VideoAuthorDetails
                    {
                        VideoDetailsJson = videoDetailsJson,
                        AuthorDetailsJson = authorDetailsJson
                    };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error fetching video and author details: {ex.Message}");
                    return new VideoAuthorDetails { VideoDetailsJson = string.Empty, AuthorDetailsJson = string.Empty };
                }
            });
        }

        private async Task PopulateComments(string videoId)
        {
            try
            {
                string commentsJson = string.IsNullOrEmpty(continuationTokenNextComments)
                                      ? await FetchNewestComments(videoId)
                                      : await FetchCommentsPostContinued(continuationTokenNextComments);

                if (string.IsNullOrEmpty(commentsJson))
                {
                    Debug.WriteLine($"No comments found for video ID: {videoId}");
                    return;
                }

                var commentEntities = GetCommentEntities(commentsJson);

                if (commentEntities.Any())
                {
                    var videoComments = new List<VideoComment>();

                    foreach (var entity in commentEntities)
                    {
                        var commentId = entity.SelectToken("properties.commentId")?.ToString();
                        var content = entity.SelectToken("properties.content.content")?.ToString();
                        var date = entity.SelectToken("properties.publishedTime")?.ToString();
                        var author = entity.SelectToken("properties.authorButtonA11y")?.ToString();
                        var authorChannelId = entity.SelectToken("author.channelId")?.ToString();
                        var avatar = entity.SelectToken("author.avatarThumbnailUrl")?.ToString();

                        var videoComment = new VideoComment
                        {
                            CommentId = commentId,
                            Content = content,
                            Date = date,
                            Author = author,
                            AuthorChannelId = authorChannelId,
                            Avatar = avatar
                        };

                        videoComments.Add(videoComment);
                        AddCommentCard(videoComment, CommentsItemControl);
                    }
                }
                else
                {
                    Debug.WriteLine("No comment entities found.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in PopulateComments for video ID {videoId}: {ex.Message}");
            }
            finally
            {
                IsLoadedForPivotComments = true;
                BubbleLoadingAnimationCom.Stop();
                LoadingPanelCom.Visibility = Visibility.Collapsed;
                LoadMore.Visibility = Visibility.Visible;
            }
        }

        private async Task<string> FetchNewestComments(string videoId)
        {
            try
            {
                string url = $"https://www.youtube.com/youtubei/v1/next?key={Settings.InnerTubeApiKey}";

                var initialContextData = new
                {
                    videoId = videoId,
                    context = new
                    {
                        client = new
                        {
                            hl = "en",
                            gl = "US",
                            clientName = "WEB",
                            clientVersion = "2.20211122.09.00",
                            osName = "Windows",
                            osVersion = "10",
                            platform = "DESKTOP"
                        }
                    }
                };

                string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(initialContextData);
                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                if (_httpClient == null)
                {
                    _httpClient = new HttpClient();
                    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.82 Safari/537.36");
                }

                HttpResponseMessage response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(responseBody);
                var newestToken = jsonResponse
                    ["engagementPanels"]?.FirstOrDefault()
                    ["engagementPanelSectionListRenderer"]?["header"]?["engagementPanelTitleHeaderRenderer"]
                    ["menu"]?["sortFilterSubMenuRenderer"]?["subMenuItems"]
                    ?.FirstOrDefault(item => item["title"]?.ToString() == "Top comments")
                    ?["serviceEndpoint"]?["continuationCommand"]?["token"]?.ToString();

                if (string.IsNullOrEmpty(newestToken))
                {
                    Debug.WriteLine("No continuation token found for newest comments.");
                    return string.Empty;
                }

                var continuationContextData = new
                {
                    context = initialContextData.context,
                    continuation = newestToken
                };

                string continuationRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(continuationContextData);
                var continuationContent = new StringContent(continuationRequestBody, Encoding.UTF8, "application/json");

                response = await _httpClient.PostAsync(url, continuationContent);
                response.EnsureSuccessStatusCode();

                string continuationResponseBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("Newest Comments API Response: " + continuationResponseBody);

                string _continuationTokenNextComments = ExtractContinuationToken(continuationResponseBody);

                if (!string.IsNullOrEmpty(_continuationTokenNextComments))
                {
                    Debug.WriteLine("Found continuation token: " + _continuationTokenNextComments);
                    continuationTokenNextComments = _continuationTokenNextComments;
                }
                else
                {
                    Debug.WriteLine("No continuation token found.");
                }

                return continuationResponseBody;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error fetching newest comments: " + ex.Message);
                return string.Empty;
            }
        }

        private string ExtractContinuationToken(string jsonResponse)
        {
            try
            {
                var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(jsonResponse);

                var tokens = jsonObject.SelectTokens("$..token")
                                       .Select(t => t.ToString())
                                       .ToList();

                if (tokens.Any())
                {
                    string longestToken = tokens.OrderByDescending(t => t.Length).First();
                    Debug.WriteLine("Found longest continuation token: " + longestToken);
                    return longestToken;
                }

                Debug.WriteLine("No continuation token found.");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in ExtractContinuationToken: {ex.Message}");
                return string.Empty;
            }
        }

        private List<JToken> FindAllOccurrences(JToken container, string targetKey)
        {
            var matches = new List<JToken>();

            if (container.Type == JTokenType.Object)
            {
                foreach (var property in container.Children<JProperty>())
                {
                    if (property.Name == targetKey)
                    {
                        matches.Add(property.Value);
                    }
                    else
                    {
                        matches.AddRange(FindAllOccurrences(property.Value, targetKey));
                    }
                }
            }
            else if (container.Type == JTokenType.Array)
            {
                foreach (var item in container.Children())
                {
                    matches.AddRange(FindAllOccurrences(item, targetKey));
                }
            }

            return matches;
        }

        private List<JToken> GetCommentEntities(string json)
        {
            try
            {
                var jsonResponse = JObject.Parse(json);

                return FindAllOccurrences(jsonResponse, "commentEntityPayload");
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine($"JSON parsing error: {ex.Message}");
                return new List<JToken>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in GetCommentEntities: {ex.Message}");
                return new List<JToken>();
            }
        }

        private Border CreateCommentCard(VideoComment comment)
        {
            var grid = new Grid
            {
                Margin = new Thickness(15, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            var bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new Uri(comment.Avatar, UriKind.Absolute);

            var thumbnailImage = new Image
            {
                Source = bitmapImage,
                Width = 47.5,
                Height = 47.5,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(10, 0, 10, 0)
            };

            Grid.SetColumn(thumbnailImage, 0);
            Grid.SetRow(thumbnailImage, 0);
            grid.Children.Add(thumbnailImage);

            var infoPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 0)
            };

            var nameAndDateGrid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            nameAndDateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            nameAndDateGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });

            var nameTextBlock = new TextBlock
            {
                Text = comment.Author,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 91, 141, 195)),
                Margin = new Thickness(0, 0, 5, 0)
            };
            Grid.SetColumn(nameTextBlock, 0);
            nameAndDateGrid.Children.Add(nameTextBlock);

            var dateTextBlock = new TextBlock
            {
                Text = $"| {comment.Date}",
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 14
            };
            Grid.SetColumn(dateTextBlock, 1);
            nameAndDateGrid.Children.Add(dateTextBlock);

            infoPanel.Children.Add(nameAndDateGrid);

            Grid.SetColumn(infoPanel, 1);
            Grid.SetRow(infoPanel, 0);
            grid.Children.Add(infoPanel);

            var commentTextBlock = new TextBlock
            {
                Text = comment.Content,
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 16,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, -25, 0, 0)
            };

            Grid.SetColumn(commentTextBlock, 1);
            Grid.SetRow(commentTextBlock, 1);
            grid.Children.Add(commentTextBlock);

            return new Border
            {
                Child = grid,
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Colors.Transparent)
            };
        }

        private void Border_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            try
            {
                Border border = (Border)sender;

                if (border.Tag == null)
                {
                    Debug.WriteLine("Tag is null.");
                    return;
                }

                string videoId = border.Tag.ToString();

                if (string.IsNullOrEmpty(videoId))
                {
                    Debug.WriteLine("videoId is null or empty.");
                    return;
                }

                Debug.WriteLine("Navigating with videoId: " + videoId);

                Settings.AddSeedVideoId(videoId);

                Uri navigationUri = new Uri("/VideoPage.xaml?videoId=" + videoId, UriKind.Relative);
                Debug.WriteLine("Navigating to: " + navigationUri);

                NavigationService.Navigate(navigationUri);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception caught in Border_Tap: {ex.Message}");
            }
        }

        private Border CreateVideoCard(RealtedVideoDetail video)
        {
            var grid = new Grid
            {
                Margin = new Thickness(15, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

            var thumbnailImage = new Image
            {
                Source = new BitmapImage(new Uri(video.Thumbnail, UriKind.Absolute)),
                Width = 95,
                Height = 65,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(10, 0, 5, 0)
            };
            Grid.SetColumn(thumbnailImage, 0);
            grid.Children.Add(thumbnailImage);

            var lengthBorder = new Border
            {
                Background = new SolidColorBrush(Colors.Black) { Opacity = 0.7 },
                Padding = new Thickness(1),
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 5, 0),
                MaxWidth = 100
            };

            var lengthTextBlock = new TextBlock
            {
                Text = video.Length,
                FontSize = 11.5,
                Foreground = new SolidColorBrush(Colors.White),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            lengthBorder.Child = lengthTextBlock;
            Grid.SetColumn(lengthBorder, 0);
            Grid.SetRow(lengthBorder, 0);
            grid.Children.Add(lengthBorder);

            var infoPanel = new Grid
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0)
            };
            Grid.SetColumn(infoPanel, 1);
            grid.Children.Add(infoPanel);

            var titleTextBlock = new TextBlock
            {
                Text = video.Title,
                FontSize = 16,
                FontWeight = FontWeights.Normal,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51)),
                Margin = new Thickness(0, -3.5, 0, 5),
                MaxWidth = 250,
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 45,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            infoPanel.Children.Add(titleTextBlock);

            var viewsAndAuthorPanel = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(0, 5, 0, 5)
            };

            var viewsAndAuthorTextBlock = new TextBlock
            {
                Text = $"{video.Views} Views - By {video.Author}",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102)),
                TextWrapping = TextWrapping.NoWrap,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            viewsAndAuthorPanel.Children.Add(viewsAndAuthorTextBlock);
            infoPanel.Children.Add(viewsAndAuthorPanel);


            var videoCardBorder = new Border
            {
                Child = grid,
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                Background = new SolidColorBrush(Colors.Transparent),
                Tag = video.VideoId
            };

            videoCardBorder.Tap += Border_Tap;

            return videoCardBorder;
        }

        private void MainPivot_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private async void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var selectedPivotItem = VideoPivot.SelectedItem as PivotItem;

            if (selectedPivotItem != null)
            {

                switch (selectedPivotItem.Tag?.ToString())
                {
                    case "details":


                        if (!string.IsNullOrEmpty(localVideoId) && IsLoadedForPivotDetails == false)
                        {

                            LoadingPanelDel.Visibility = Visibility.Visible;
                            BubbleLoadingAnimationDel.Begin();
                            StuffPanel.Visibility = Visibility.Collapsed;
                            await PopulateVideoDetails(localVideoId);
                        }

                        Debug.WriteLine("Selected Pivot Item: details");

                        break;

                    case "comments":

                        if (!string.IsNullOrEmpty(localVideoId) && IsLoadedForPivotComments == false)
                        {
                            LoadMore.Visibility = Visibility.Collapsed;
                            LoadingPanelCom.Visibility = Visibility.Visible;
                            BubbleLoadingAnimationCom.Begin();

                            await PopulateComments(localVideoId);
                        } else
                        {
                            LoadMore.Visibility = Visibility.Visible;
                        }

                        Debug.WriteLine("Selected Pivot Item: comments");

                        break;

                    case "related videos":

                        LoadingPanelRel.Visibility = Visibility.Visible;
                        BubbleLoadingAnimationRel.Begin();

                        if (!string.IsNullOrEmpty(localVideoId) && IsLoadedForPivotRealted == false)
                        {                   
                            await PopulateRelatedVideos(localVideoId);
                        }

                        Debug.WriteLine("Selected Pivot Item: related videos");
                        break;

                    default:
                        Debug.WriteLine("Unknown Pivot Item");
                        break;
                }
            }
        }

    }
}