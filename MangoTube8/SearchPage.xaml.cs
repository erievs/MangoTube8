using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Phone.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using static MangoTube8.YouTubeModal;
using System.Windows.Input;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MangoTube8
{
    public partial class SearchPage : PhoneApplicationPage
    {
        private bool _isSearching = false;
        private bool _hasMoreResults = false;
        private string _continuationToken = null;
        private List<SearchVideoDetail> _searchResults = new List<SearchVideoDetail>();

        public SearchPage()
        {
            InitializeComponent();

        }

        protected override async void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.ContainsKey("query"))
            {
                string query = NavigationContext.QueryString["query"];
                Debug.WriteLine("Search query: " + query);

                await SearchAsync(query);
            }
            else
            {
                MessageBox.Show("Search query is required.");
                NavigationService.GoBack();
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

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

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

        private async Task SearchAsync(string query, string continuationToken = null)
        {
            if (!_isSearching && string.IsNullOrEmpty(continuationToken))
            {
                _hasMoreResults = true;
                _continuationToken = null;
            }

            if (_isSearching || (!_hasMoreResults && continuationToken == null))
            {
                Debug.WriteLine("Skipping search: either already searching or no more results.");
                return;
            }

            LoadingPanel.Visibility = Visibility.Visible;
            LoadMore.Visibility = Visibility.Collapsed;
            BubbleLoadingAnimation.Begin();

            _isSearching = true;
            Debug.WriteLine("Starting search with query: " + query + " and continuation token: " + (continuationToken ?? "None"));

            string innerTubeUrl = $"https://www.youtube.com/youtubei/v1/search?key={Settings.InnerTubeApiKey}";
            Debug.WriteLine("Making search request to: " + innerTubeUrl);

            using (var httpClient = new HttpClient())
            {
                try
                {
                    var searchData = new
                    {
                        query = query,
                        context = new
                        {
                            client = new
                            {
                                hl = "en",
                                gl = "US",
                                clientName = "WEB",
                                clientVersion = "2.20211122.09.00"
                            }
                        },
                        @params = "",
                        continuation = continuationToken
                    };

                    var jsonContent = new StringContent(JsonConvert.SerializeObject(searchData), System.Text.Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(innerTubeUrl, jsonContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"Error: Failed to fetch data. Status code: {response.StatusCode}");
                        return;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Raw JSON Response: " + jsonResponse);

                    var jsonObject = JObject.Parse(jsonResponse);

                    var items = jsonObject.SelectTokens("$.contents.twoColumnSearchResultsRenderer.primaryContents.sectionListRenderer.contents[*].itemSectionRenderer.contents[*].videoRenderer");

                    if (items == null || !items.Any())
                    {
                    
                        items = jsonObject.SelectTokens("..videoRenderer");

                        if (items == null || !items.Any())
                        {
                            Debug.WriteLine("No videoRenderer found in continuation response.");
                            _hasMoreResults = false;
                            return;
                        }
                    }

                    if (items != null)
                    {
                        var videos = new List<SearchVideoDetail>();

                        foreach (var item in items)
                        {
                            var videoResult = new SearchVideoDetail
                            {
                                VideoId = item.SelectToken("videoId")?.ToString(),
                                Title = item.SelectToken("title.runs[0].text")?.ToString(),
                                Thumbnail = item.SelectToken("thumbnail.thumbnails[1].url")?.ToString(),
                                Author = item.SelectToken("ownerText.runs[0].text")?.ToString(),
                                Views = item.SelectToken("viewCountText.simpleText")?.ToString(),
                                Date = item.SelectToken("publishedTimeText.simpleText")?.ToString(),
                                Length = item.SelectToken("lengthText.simpleText")?.ToString()
                            };

                            if (videoResult != null)
                            {
                                videos.Add(videoResult);
                                AddVideoCard(videoResult, SearchItemsControl);
                                _searchResults.Add(videoResult);
                            }
                        }

                        string videoSearchDetailsJson = Newtonsoft.Json.JsonConvert.SerializeObject(videos, Newtonsoft.Json.Formatting.Indented);
                        Debug.WriteLine("Video Details JSON: " + videoSearchDetailsJson);

                        const string continuationCommandKey = "\"continuationCommand\": {";
                        const string tokenKey = "\"token\": \"";

                        int startIndex = jsonResponse.IndexOf(continuationCommandKey);
                        if (startIndex != -1)
                        {
                            startIndex = jsonResponse.IndexOf(tokenKey, startIndex);
                            if (startIndex != -1)
                            {
                                startIndex += tokenKey.Length;
                                int endIndex = jsonResponse.IndexOf("\"", startIndex);
                                if (endIndex != -1)
                                {
                                    _continuationToken = jsonResponse.Substring(startIndex, endIndex - startIndex);
                                    _hasMoreResults = !string.IsNullOrEmpty(_continuationToken);
                                    Debug.WriteLine("Continuation token extracted: " + _continuationToken);
                                }
                            }
                        }
                        else
                        {
                            _hasMoreResults = false;
                            Debug.WriteLine("Continuation command not found in the response.");
                        }
                    }

                    else
                    {
                        Debug.WriteLine("No items found in the response.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error fetching search results: " + ex.Message);
                }
                finally
                {
                    BubbleLoadingAnimation.Stop();
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    LoadMore.Visibility = _hasMoreResults ? Visibility.Visible : Visibility.Collapsed;
                    _isSearching = false;
                }
            }
        }

        private async void LoadMoreButton_Click(object sender, RoutedEventArgs e)
        {

            Debug.WriteLine("Loading more results...");

            if (_hasMoreResults && !string.IsNullOrEmpty(_continuationToken))
            {
                Debug.WriteLine("Loading more results...");
                await SearchAsync(SearchTextBox.Text, _continuationToken);
            }
        }

        private void AddVideoCard(SearchVideoDetail video, ItemsControl itemsControl)
        {
            var videoCard = CreateVideoCard(video);
            itemsControl.Items.Add(videoCard);
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

        private Border CreateVideoCard(SearchVideoDetail video)
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


    }
}