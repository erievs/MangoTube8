        using System.Windows.Media;
        using Microsoft.Phone.Controls;
        using System.Windows.Controls;
        using System;
        using System.Text;
        using System.Windows;
        using System.Net.Http;
        using static MangoTube8.YouTubeModal;
        using System.Collections.Generic;
        using Newtonsoft.Json.Linq;
        using System.Linq;
        using Microsoft.Phone.Shell;
        using System.Windows.Input;
        using System.Diagnostics;
        using System.Threading.Tasks;
        using System.Collections.ObjectModel;
        using System.Windows.Media.Imaging;
        using Newtonsoft.Json;

        namespace MangoTube8
        {
            public partial class MainPage : PhoneApplicationPage
            {
                public MainPage()
                {
                    InitializeComponent();
                    Settings.LoadSeedVideoIds();

                    CreateAppBarBecauseWeCannotDoWhatWeWantEasilyInXamlOrIMaybeStupid();
                }

                public void CreateAppBarBecauseWeCannotDoWhatWeWantEasilyInXamlOrIMaybeStupid()
                {

                    ApplicationBar = new ApplicationBar
                    {
                        Mode = ApplicationBarMode.Minimized,
                        IsVisible = true,
                        IsMenuEnabled = true
                    };

                    ApplicationBarMenuItem settingsMenuItem = new ApplicationBarMenuItem("Settings");
                    settingsMenuItem.Click += Settings_Click;
                    ApplicationBar.MenuItems.Add(settingsMenuItem);
                }

                private void Settings_Click(object sender, EventArgs e)
                {
                    NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
                }

                private ObservableCollection<VideoDetails> TrendingVideos { get; set; } = new ObservableCollection<VideoDetails>();
                private ObservableCollection<VideoDetails> SpotlightVideos { get; set; } = new ObservableCollection<VideoDetails>();
                private ObservableCollection<VideoDetails> RecommendedVideos { get; set; } = new ObservableCollection<VideoDetails>();


                private List<VideoDetails> currentTrendingVideos = new List<VideoDetails>();
                private List<VideoDetails> currentSpotlightVideos = new List<VideoDetails>();
                private List<VideoDetails> currentRecommendedVideos = new List<VideoDetails>();


                private int displayedVideosCount = 0;

                private int displayedVideosCountTrending = 0;

                private int displayedVideosCountSpotlight = 0;

                private int displayedVideosCountRecommended = 0;

                private bool isAllVideosLoaded = false;

                private bool IsTrendingLoaded = false;
                private bool IsSpotlightLoaded = false;
                private bool IsRecIsRecommendedLoaded = false;

                private YouTubeFeed ItemControlWeAreOne;

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

                private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
                {
                    if (e.Key == Key.Enter)
                    {
                        string searchText = SearchTextBox.Text;

                        Debug.WriteLine("Search Text: " + searchText);

                        NavigationService.Navigate(new Uri("/SearchPage.xaml?query=" + Uri.EscapeDataString(searchText), UriKind.Relative));
                    }
                }

                private void HideSearchBox_Completed(object sender, EventArgs e)
                {

                    YouTubeLogo.Visibility = Visibility.Visible;
                    SearchTextBox.Visibility = Visibility.Collapsed;
                }

                private async void FetchVideos(YouTubeFeed feed)
                {
                    try
                    {
                        using (HttpClient httpClient = new HttpClient())
                        {
                            string url = $"https://www.youtube.com/youtubei/v1/browse?key={Settings.InnerTubeApiKey}";
                            string browseId;
                            string additionalParams = string.Empty;
                            string recommendedJson = null;

                            TrendingVideos.Clear();
                            SpotlightVideos.Clear();
                            RecommendedVideos.Clear();

                            currentRecommendedVideos.Clear();
                            currentSpotlightVideos.Clear();
                            currentTrendingVideos.Clear();

                            ItemControlWeAreOne = feed;

                            switch (feed)
                            {
                                case YouTubeFeed.Trending:
                                    LoadingPanelTrending.Visibility = Visibility.Visible;
                                    LoadMore.Visibility = Visibility.Collapsed;
                                    BubbleLoadingAnimationRec.Begin();
                                    browseId = "FEtrending";
                                    break;
                                case YouTubeFeed.Sports:
                                    browseId = "UCEgdi0XIXXZ-qJOFPf4JSKw";
                                    break;
                                case YouTubeFeed.Gaming:
                                    browseId = "UCOpNcN46UbXVtpKMrmU4Abg";
                                    break;
                                case YouTubeFeed.News:
                                    browseId = "UCYfdidRxbB8Qhf0Nx7ioOYw";
                                    break;
                                case YouTubeFeed.Podcasts:
                                    browseId = "FEpodcasts_destination";
                                    additionalParams = "qgcCCAE%3D";
                                    break;
                                case YouTubeFeed.Popular:
                                    browseId = "UCF0pVplsI8R5kcAqgtoRqoA";
                                    break;
                                case YouTubeFeed.Education:
                                    browseId = "UCtFRv9O2AHqOZjjynzrv-xg";
                                    break;
                                case YouTubeFeed.Spotlight:
                                    LoadingPanelSpot.Visibility = Visibility.Visible;
                                    LoadMoreSpot.Visibility = Visibility.Collapsed;
                                    BubbleLoadingAnimationRec.Begin();
                                    browseId = "UCBR8-60-B28hp2BmDPdntcQ";
                                    break;
                                case YouTubeFeed.Recommended:
                                    LoadingPanelRecommended.Visibility = Visibility.Visible;
                                    LoadMoreRec.Visibility = Visibility.Collapsed;
                                    BubbleLoadingAnimationRec.Begin();
                                    recommendedJson = await FetchRecommendedVideos(httpClient, Settings.GetRandomSeedVideoId());
                                    browseId = "null";
                                    break;
                                default:
                                    browseId = "FEtrending";
                                    break;
                            }

                            var clientContext = (feed == YouTubeFeed.Spotlight)
                                ? new
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
                                : new
                                {
                                    hl = "en",
                                    gl = "US",
                                    clientName = "IOS",
                                    clientVersion = "19.32.8",
                                    userAgent = "com.google.ios.youtube/19.32.8 (iPhone14,5; U; CPU iOS 17_6 like Mac OS X;)",
                                    osName = "iOS",
                                    osVersion = "17.6.1.21G93",
                                    platform = "MOBILE"
                                };

                            var requestBody = new
                            {
                                browseId = browseId,
                                context = new
                                {
                                    client = clientContext
                                },
                                additionalParams = additionalParams.Length > 0 ? additionalParams : null
                            };

                            List<JToken> allVideoItems = new List<JToken>();

                            if (feed != YouTubeFeed.Recommended)
                            {

                                string jsonRequestBody = Newtonsoft.Json.JsonConvert.SerializeObject(requestBody);
                                jsonRequestBody = jsonRequestBody.Replace("additionalParams", "params");
                                var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

                                Debug.WriteLine("Request URL: " + url);
                                Debug.WriteLine("Request Body: " + jsonRequestBody);

                                HttpResponseMessage response = await httpClient.PostAsync(url, content);
                                response.EnsureSuccessStatusCode();

                                string responseBody = await response.Content.ReadAsStringAsync();
                                Debug.WriteLine("API Response: " + responseBody);

                                JObject jsonResponse = JObject.Parse(responseBody);

                                allVideoItems = feed == YouTubeFeed.Spotlight
                                    ? jsonResponse.SelectTokens("$..gridVideoRenderer").Where(x => x is JObject).ToList()
                                    : feed == YouTubeFeed.Trending
                                        ? jsonResponse.SelectTokens("$..videoWithContextRenderer").Where(x => x is JObject).ToList()
                                            : jsonResponse.SelectTokens("$..videoData").Where(x => x is JObject).ToList();

                            }

                            else
                            {


                                if (!string.IsNullOrEmpty(recommendedJson) && recommendedJson != "null")
                                {
                                    Debug.WriteLine("Using Recommended JSON.");

                                    JObject jsonResponse = JObject.Parse(recommendedJson);
                                    allVideoItems = jsonResponse.SelectTokens("$..compactVideoRenderer").Where(x => x is JObject).ToList();
                                }
                                else
                                {
                                    Debug.WriteLine("Recommended JSON is 'null' or empty, skipping data processing.");
                                }
                            }

                            var videos = new List<VideoDetails>();

                            foreach (var item in allVideoItems)
                            {
                                if (feed == YouTubeFeed.Spotlight)
                                {
                                    var spotlightTitle = item.SelectToken("title.simpleText")?.ToString();
                                    var spotlightVideoId = item.SelectToken("videoId")?.ToString();
                                    var thumbnailUrl = item.SelectToken("thumbnail.thumbnails.[0].url")?.ToString();
                                    var viewsAndDate = item.SelectToken("metadata.metadataDetails")?.ToString();
                                    var author = item.SelectToken("shortBylineText.runs[0].text")?.ToString();

                                    var viewsText = item.SelectToken("shortViewCountText.simpleText")?.ToString();

                                    var authorPFP = item.SelectToken("channelThumbnail.channelThumbnailWithLinkRenderer.thumbnail.thumbnails[0].url")?.ToString();

                                    int viewCount = Utils.ParseViewCount(viewsText);

                                    string views = Utils.FormatViewCountWithCommas(viewCount);

                                    var date = item.SelectToken("publishedTimeText.simpleText")?.ToString();

                                    var titleLabel = item.SelectToken("title.accessibility.accessibilityData.label")?.ToString();
                                    string length = null;

                                    if (!string.IsNullOrEmpty(titleLabel))
                                    {

                                        var timeMatch = System.Text.RegularExpressions.Regex.Match(titleLabel, @"(\d+)\s+seconds?$");

                                        if (timeMatch.Success)
                                        {
                                            int seconds = int.Parse(timeMatch.Groups[1].Value);

                                            string formattedTime = TimeSpan.FromSeconds(seconds).ToString(@"m\:ss");

                                            length = formattedTime;

                                        }
                                    }
                                    if (!string.IsNullOrEmpty(spotlightTitle) && !string.IsNullOrEmpty(spotlightVideoId) && !string.IsNullOrEmpty(length))
                                    {
                                        var video = new VideoDetails
                                        {
                                            Title = spotlightTitle,
                                            Author = author,
                                            VideoId = spotlightVideoId,
                                            Thumbnail = thumbnailUrl,
                                            Views = views + " views",
                                            Date = date,
                                            Length = length,
                                            AuthorProfilePictureUrl = thumbnailUrl
                                        };

                                        videos.Add(video);
                                    }
                                }
                                else if (feed == YouTubeFeed.Recommended)
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
                                        var video = new VideoDetails
                                        {
                                            Title = spotlightTitle,
                                            Author = author,
                                            VideoId = spotlightVideoId,
                                            Thumbnail = thumbnailUrl,
                                            Views = views,
                                            Date = date,
                                            Length = lengthText,
                                            AuthorProfilePictureUrl = authorPFP
                                        };

                                        videos.Add(video);
                                    }
                                }
                                else if (feed == YouTubeFeed.Trending)
                                {
                                    var spotlightTitle = item.SelectToken("headline.runs[0].text")?.ToString();
                                    var spotlightVideoId = item.SelectToken("videoId")?.ToString();
                                    var thumbnailUrl = item.SelectToken("thumbnail.thumbnails.[1].url")?.ToString();
                                    var viewsAndDate = item.SelectToken("metadata.metadataDetails")?.ToString();
                                    var author = item.SelectToken("shortBylineText.runs[0].text")?.ToString();
                        
                                    var length = item.SelectToken("lengthText.runs[0].text")?.ToString();

                                    var viewsText = item.SelectToken("shortViewCountText.runs[0].text")?.ToString();

                                    int viewCount = Utils.ParseViewCount(viewsText);

                                    string views = Utils.FormatViewCountWithCommas(viewCount);

                                    var date = item.SelectToken("publishedTimeText.runs[0].text")?.ToString();

                                    var authorPFP = item.SelectToken("channelThumbnail.channelThumbnailWithLinkRenderer.thumbnail.thumbnails[0].url")?.ToString();

                                    {
                                        var video = new VideoDetails
                                        {
                                            Title = spotlightTitle,
                                            Author = author,
                                            VideoId = spotlightVideoId,
                                            Thumbnail = thumbnailUrl,
                                            Views = views + " views",
                                            Date = date,
                                            Length = length,
                                            AuthorProfilePictureUrl = authorPFP
                                        };

                                        videos.Add(video);
                                    }
                                }
                                else
                                {
                                    var title = item.SelectToken("metadata.title")?.ToString();
                                    var videoId = item.SelectToken("videoId")?.ToString();
                                    var thumbnailUrl = item.SelectToken("thumbnail.image.sources[1].url")?.ToString();
                                    var viewsAndDate = item.SelectToken("metadata.metadataDetails")?.ToString();
                                    var author = item.SelectToken("metadata.byline")?.ToString();
                                    var length = item.SelectToken("thumbnail.timestampText")?.ToString();


                                    string views = null;
                                    string date = null;
                                    if (!string.IsNullOrEmpty(viewsAndDate))
                                    {
                                        var parts = viewsAndDate.Split('·');
                                        if (parts.Length > 0)
                                            views = parts[0]?.Trim();
                                        if (parts.Length > 1)
                                            date = parts[1]?.Trim();
                                    }


                                    int parsedViews = Utils.ParseViewCount(views);
                                    string formattedViews = Utils.FormatViewCountWithCommas(parsedViews);

                                    if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(videoId))
                                    {
                                        var video = new VideoDetails
                                        {
                                            Title = title,
                                            Author = author,
                                            VideoId = videoId,
                                            Thumbnail = thumbnailUrl,
                                            Views = formattedViews + " views",
                                            Date = date,
                                            Length = length
                                        };

                                        videos.Add(video);
                                    }
                                }
                            }

                            string videoDetailsJson = Newtonsoft.Json.JsonConvert.SerializeObject(videos, Newtonsoft.Json.Formatting.Indented);
                            Debug.WriteLine("Video Details JSON: " + videoDetailsJson);

                            if (feed == YouTubeFeed.Trending)
                            {
                                var uniqueVideos = videos
                                    .GroupBy(v => v.VideoId)
                                    .Select(group => group.First())
                                    .ToList();

                                currentTrendingVideos = uniqueVideos;

                                var initialVideos = uniqueVideos.Take(3).ToList();
                                var alreadyAddedVideoIds = new HashSet<string>();

                                foreach (var video in initialVideos)
                                {
                                    if (!alreadyAddedVideoIds.Contains(video.VideoId))
                                    {
                                        AddVideoCard(video, TrendingItemsControl);
                                        alreadyAddedVideoIds.Add(video.VideoId);
                                    }
                                }
                            }

                            else if (feed == YouTubeFeed.Spotlight)
                            {
                                var uniqueVideos = videos
                                    .GroupBy(v => v.VideoId)
                                    .Select(group => group.First())
                                    .ToList();

                                currentSpotlightVideos = uniqueVideos;

                                var initialVideos = uniqueVideos.Take(3).ToList();
                                var alreadyAddedVideoIds = new HashSet<string>();

                                foreach (var video in initialVideos)
                                {
                                    if (!alreadyAddedVideoIds.Contains(video.VideoId))
                                    {
                                        AddVideoCard(video, SpotlightItemsControl);
                                        alreadyAddedVideoIds.Add(video.VideoId);
                                    }
                                }
                            }

                            else if (feed == YouTubeFeed.Recommended)
                            {
                                var uniqueVideos = videos
                                    .GroupBy(v => v.VideoId)
                                    .Select(group => group.First())
                                    .ToList();

                                currentRecommendedVideos = uniqueVideos;

                                var initialVideos = uniqueVideos.Take(3).ToList();
                                var alreadyAddedVideoIds = new HashSet<string>();

                                foreach (var video in initialVideos)
                                {
                                    if (!alreadyAddedVideoIds.Contains(video.VideoId))
                                    {
                                        AddVideoCard(video, RecommendedItemsControl);
                                        alreadyAddedVideoIds.Add(video.VideoId);
                                    }
                                }
                            }

                        }
                    }

                    catch (HttpRequestException httpEx)
                    {
                        MessageBox.Show("HTTP request error: " + httpEx.Message);
                        System.Diagnostics.Debug.WriteLine("HTTP request error details: " +
                            (httpEx.InnerException != null ? httpEx.InnerException.Message : "No inner exception"));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error fetching videos: " + ex.Message);
                    }
                    finally
                    {
       
                        if(feed == YouTubeFeed.Recommended)
                        {
                            BubbleLoadingAnimationRec.Stop();
                            LoadingPanelRecommended.Visibility = Visibility.Collapsed;
                            LoadMoreRec.Visibility =  Visibility.Visible;
                        }

                        else if (feed == YouTubeFeed.Trending)
                        {
                            BubbleLoadingAnimationTrending.Stop();
                            LoadingPanelTrending.Visibility = Visibility.Collapsed;
                            LoadMore.Visibility = Visibility.Visible;
                        }

                        else if(feed == YouTubeFeed.Spotlight)
                        {
                            BubbleLoadingAnimationSpotlight.Stop();
                            LoadingPanelSpot.Visibility = Visibility.Collapsed;
                            LoadMoreSpot.Visibility = Visibility.Visible;
                        }


                    }
                }

                private async Task<string> FetchRecommendedVideos(HttpClient httpClient, string videoId)
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

                        HttpResponseMessage response = await httpClient.PostAsync(url, content);
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

                public async Task<LikesDislikesInfo> GetLikeDislikeCounts(string videoId)
                {
                    var uri = new Uri($"https://returnyoutubedislikeapi.com/Votes?videoId={videoId}");

                    using (var client = new HttpClient())
                    {
                        try
                        {
                            var response = await client.GetStringAsync(uri);
                            var json = JsonConvert.DeserializeObject<JObject>(response);

                            var likes = json["likes"]?.ToString() ?? "0";
                            var dislikes = json["dislikes"]?.ToString() ?? "0";

                            return new LikesDislikesInfo
                            {
                                Likes = likes,
                                Dislikes = dislikes
                            };
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error fetching like/dislike data: {ex.Message}");
                            return new LikesDislikesInfo
                            {
                                Likes = "0",
                                Dislikes = "0"
                            };
                        }
                    }
                }

                private JToken ExtractField(JToken item, string fieldName)
                {
                    return item.SelectToken($"..{fieldName}");
                }

                private void MainPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    foreach (object item in MainPivot.Items)
                    {
                        PivotItem pivotItem = item as PivotItem;
                        if (pivotItem != null)
                        {

                            if (pivotItem == MainPivot.SelectedItem)
                            {
                                pivotItem.Header = pivotItem.Tag;

                                if (pivotItem.Tag.ToString() == "trending" && !IsTrendingLoaded)
                                {
                                    currentTrendingVideos.Clear();
                                    FetchVideos(YouTubeFeed.Trending);
                                    IsTrendingLoaded = true;
                                }
                                else if (pivotItem.Tag.ToString() == "spotlight" && !IsSpotlightLoaded)
                                {
                                    currentSpotlightVideos.Clear();
                                    FetchVideos(YouTubeFeed.Spotlight);
                                    IsSpotlightLoaded = true;
                                }
                                else if (pivotItem.Tag.ToString() == "recommended" && !IsRecIsRecommendedLoaded)
                                {
                                    currentRecommendedVideos.Clear();
                                    FetchVideos(YouTubeFeed.Recommended);
                                    IsRecIsRecommendedLoaded = true;
                                }
                            }
                            else
                            {
                                pivotItem.Header = "";
                            }
                        }
                    }
                }

                private void AddVideoCard(VideoDetails video, ItemsControl itemsControl)
                {
                    var videoCard = CreateVideoCard(video);
                    itemsControl.Items.Add(videoCard);
                }

                private void LoadMoreButton_Click(object sender, RoutedEventArgs e)
                {
                    var remainingVideos = new List<VideoDetails>();
                    ItemsControl itemControlToUse = null;

                    if (sender == LoadMoreRec)
                    {
                        itemControlToUse = RecommendedItemsControl;
                        remainingVideos = currentRecommendedVideos.Skip(displayedVideosCountRecommended + 3).ToList();
                        System.Diagnostics.Debug.WriteLine("Loading more for Recommendations.");
                    }
                    else if (sender == LoadMore)
                    {
                        itemControlToUse = TrendingItemsControl;
                        remainingVideos = currentTrendingVideos.Skip(displayedVideosCountTrending + 3).ToList();
                        System.Diagnostics.Debug.WriteLine("Loading more for Trending.");
                    }
                    else if (sender == LoadMoreSpot)
                    {
                        itemControlToUse = SpotlightItemsControl;
                        remainingVideos = currentSpotlightVideos.Skip(displayedVideosCountSpotlight + 3).ToList();
                        System.Diagnostics.Debug.WriteLine("Loading more for Spotlight.");
                    }

                    var alreadyAddedVideoIds = new HashSet<string>();

                    foreach (var video in remainingVideos)
                    {
                        if (alreadyAddedVideoIds.Contains(video.VideoId))
                        {
                            System.Diagnostics.Debug.WriteLine($"Duplicate VideoId detected: {video.VideoId}");
                            continue;
                        }

                        AddVideoCard(video, itemControlToUse);
                        alreadyAddedVideoIds.Add(video.VideoId);
                    }

                    if (sender == LoadMoreRec)
                    {
                        IsRecIsRecommendedLoaded = true;
                        displayedVideosCountRecommended += remainingVideos.Count;
                        System.Diagnostics.Debug.WriteLine($"Displayed Recommended Videos Count: {displayedVideosCountRecommended}");
                    }
                    else if (sender == LoadMore)
                    {
                        displayedVideosCountTrending += remainingVideos.Count;
                        IsTrendingLoaded = true;
                        System.Diagnostics.Debug.WriteLine($"Displayed Trending Videos Count: {displayedVideosCountTrending}");
                    }
                    else if (sender == LoadMoreSpot)
                    {
                        displayedVideosCountSpotlight += remainingVideos.Count;
                        IsSpotlightLoaded = true;
                        System.Diagnostics.Debug.WriteLine($"Displayed Spotlight Videos Count: {displayedVideosCountSpotlight}");
                    }

                    if (sender == LoadMoreRec)
                    {
                        LoadMoreRec.Visibility = Visibility.Collapsed;
                    }
                    if (sender == LoadMore)
                    {
                        LoadMore.Visibility = Visibility.Collapsed;
                    }
                    if (sender == LoadMoreSpot)
                    {
                        LoadMoreSpot.Visibility = Visibility.Collapsed;
                    }

                    isAllVideosLoaded = (displayedVideosCountRecommended >= currentRecommendedVideos.Count ||
                                         displayedVideosCountTrending >= currentTrendingVideos.Count ||
                                         displayedVideosCountSpotlight >= currentSpotlightVideos.Count);

                    System.Diagnostics.Debug.WriteLine($"All videos loaded: {isAllVideosLoaded}");
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

                private Border CreateVideoCard(VideoDetails video)
                {
                    var border = new Border
                    {
                        Margin = new Thickness(20,2.5, 10, 5),
                        Background = new SolidColorBrush(Color.FromArgb(255, 246, 246, 246)),
                        BorderBrush = new SolidColorBrush(Color.FromArgb(255, 204, 204, 204)),
                        BorderThickness = new Thickness(0.5),
                        Height = 150,
                        Width = Double.NaN 
                    };

                    border.DataContext = video;

                    border.Tag = video.VideoId;

                    // Assign the Tap event handler to the Border
                    border.Tap += Border_Tap;

                    var grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) }); // 80% Thumbnail
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 20% Info Panel

                    var thumbnailGrid = new Grid();
                    thumbnailGrid.Children.Add(new Image
                    {
                        Source = new BitmapImage(new Uri(video.Thumbnail, UriKind.Absolute)),
                        Stretch = Stretch.UniformToFill,
                        HorizontalAlignment = HorizontalAlignment.Stretch
                    });

                    var overlay = new Border
                    {
                        Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    thumbnailGrid.Children.Add(overlay);

                    // Author (Top Left)
                    var authorTextBlock = new TextBlock
                    {
                        Text = video.Author,
                        FontSize = 18,
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5, 0, 0, 5),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    thumbnailGrid.Children.Add(authorTextBlock);

                    // Title (Bottom Left)
                    var titleTextBlock = new TextBlock
                    {
                        Text = video.Title,
                        FontSize = 18,
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(5, 5, 0, 0),
                        VerticalAlignment = VerticalAlignment.Bottom,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        TextWrapping = TextWrapping.Wrap,
                        MaxWidth = 360,
                        TextTrimming = TextTrimming.WordEllipsis,
                        LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                    };
                    thumbnailGrid.Children.Add(titleTextBlock);

                    // Video Length (Top Right)
                    var lengthTextBlock = new TextBlock
                    {
                        Text = video.Length, 
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Colors.White),
                        Margin = new Thickness(0, 5, 5, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    thumbnailGrid.Children.Add(lengthTextBlock);

                    grid.Children.Add(thumbnailGrid);
                    Grid.SetColumn(thumbnailGrid, 0);

                    // Info Panel (20%)
                    var infoPanel = new StackPanel
                    {
                        Margin = new Thickness(5, 5, 15, 10),
                        VerticalAlignment = VerticalAlignment.Top
                    };

                    var authorImage = new Image
                    {
                        Source = new BitmapImage(new Uri(video.AuthorProfilePictureUrl, UriKind.Absolute)),
                        Width = 42.5,
                        Height = 42.5,
                        Margin = new Thickness(5, 0, 0, 5),
                        Stretch = Stretch.UniformToFill,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    infoPanel.Children.Add(authorImage);

                    var textPanel = new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    var dateTextBlock = new TextBlock
                    {
                        Text = video.Date,
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Colors.Black),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        FontWeight = FontWeights.Medium,
                        TextWrapping = TextWrapping.Wrap
                    };
                    textPanel.Children.Add(dateTextBlock);

                    var viewsTextBlock = new TextBlock
                    {
                        Text = $"{video.Views}",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Colors.Black),
                        Margin = new Thickness(0, 0, 0, 0),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        FontWeight = FontWeights.Medium,
                        TextWrapping = TextWrapping.Wrap
                    };
                    textPanel.Children.Add(viewsTextBlock);

                    infoPanel.Children.Add(textPanel);

                    grid.Children.Add(infoPanel);
                    Grid.SetColumn(infoPanel, 1);

                    border.Child = grid;
                    return border;
                }


                private void RefreshButton_Click(object sender, EventArgs e)
                {

                }

                private void MainPivot_Loaded(object sender, System.Windows.RoutedEventArgs e)
                {

                }
            }
        }