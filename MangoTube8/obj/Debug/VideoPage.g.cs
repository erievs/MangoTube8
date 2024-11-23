﻿#pragma checksum "C:\Users\Nicholas Bryan Brook\documents\visual studio 2015\Projects\MangoTube8\MangoTube8\VideoPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "F2516BFCB1DA072A521F1774342ADB86"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace MangoTube8 {
    
    
    public partial class VideoPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Media.Animation.Storyboard BubbleLoadingAnimationRel;
        
        internal System.Windows.Media.Animation.Storyboard BubbleLoadingAnimationCom;
        
        internal System.Windows.Media.Animation.Storyboard BubbleLoadingAnimationDel;
        
        internal System.Windows.Media.Animation.Storyboard BubbleLoadingAnimationPlayer;
        
        internal System.Windows.Media.Animation.Storyboard ShowSearchBox;
        
        internal System.Windows.Media.Animation.Storyboard HideSearchBox;
        
        internal System.Windows.Media.Animation.Storyboard ShowControlsAnimation;
        
        internal System.Windows.Media.Animation.Storyboard HideControlsAnimation;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Grid Header;
        
        internal System.Windows.Controls.Image YouTubeLogo;
        
        internal System.Windows.Controls.Grid SearchContainer;
        
        internal System.Windows.Controls.TextBox SearchTextBox;
        
        internal System.Windows.Controls.Button SearchButton;
        
        internal System.Windows.Controls.Image SearchIcon;
        
        internal System.Windows.Controls.MediaElement VideoPlayer;
        
        internal System.Windows.Controls.MediaElement AudioPlayer;
        
        internal System.Windows.Controls.Border IconAndStuffBackground;
        
        internal System.Windows.Controls.Grid IconAndStuff;
        
        internal System.Windows.Controls.StackPanel LoadingPanelPlayer;
        
        internal System.Windows.Shapes.Ellipse Bubble1Player;
        
        internal System.Windows.Shapes.Ellipse Bubble2Player;
        
        internal System.Windows.Shapes.Ellipse Bubble3Player;
        
        internal System.Windows.Controls.Image PlayPauseIcon;
        
        internal System.Windows.Controls.TextBlock SkipTimeTextMinus;
        
        internal System.Windows.Controls.TextBlock SkipTimeTextPlus;
        
        internal System.Windows.Controls.Image FullScreenIcon;
        
        internal System.Windows.Controls.Slider ProgressSlider;
        
        internal System.Windows.Controls.TextBlock StartTimeText;
        
        internal System.Windows.Controls.TextBlock EndTimeText;
        
        internal Microsoft.Phone.Controls.Pivot VideoPivot;
        
        internal System.Windows.Controls.StackPanel LoadingPanelDel;
        
        internal System.Windows.Shapes.Ellipse Bubble1Del;
        
        internal System.Windows.Shapes.Ellipse Bubble2Del;
        
        internal System.Windows.Shapes.Ellipse Bubble3Del;
        
        internal System.Windows.Controls.TextBlock VideoTitle;
        
        internal System.Windows.Controls.TextBlock VideoViews;
        
        internal System.Windows.Controls.Border StuffPanel;
        
        internal System.Windows.Controls.Image ProfilePicture;
        
        internal System.Windows.Controls.TextBlock Username;
        
        internal System.Windows.Controls.TextBlock SubscriptionStatus;
        
        internal System.Windows.Controls.TextBlock VideoUploadDate;
        
        internal System.Windows.Controls.RichTextBox VideoDescription;
        
        internal System.Windows.Controls.ItemsControl CommentsItemControl;
        
        internal System.Windows.Controls.StackPanel LoadingPanelCom;
        
        internal System.Windows.Shapes.Ellipse Bubble1Com;
        
        internal System.Windows.Shapes.Ellipse Bubble2Com;
        
        internal System.Windows.Shapes.Ellipse Bubble3Com;
        
        internal System.Windows.Controls.Button LoadMore;
        
        internal System.Windows.Controls.ItemsControl RealtedVideoItemControl;
        
        internal System.Windows.Controls.StackPanel LoadingPanelRel;
        
        internal System.Windows.Shapes.Ellipse Bubble1Rel;
        
        internal System.Windows.Shapes.Ellipse Bubble2Rel;
        
        internal System.Windows.Shapes.Ellipse Bubble3Rel;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/MangoTube8;component/VideoPage.xaml", System.UriKind.Relative));
            this.BubbleLoadingAnimationRel = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BubbleLoadingAnimationRel")));
            this.BubbleLoadingAnimationCom = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BubbleLoadingAnimationCom")));
            this.BubbleLoadingAnimationDel = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BubbleLoadingAnimationDel")));
            this.BubbleLoadingAnimationPlayer = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BubbleLoadingAnimationPlayer")));
            this.ShowSearchBox = ((System.Windows.Media.Animation.Storyboard)(this.FindName("ShowSearchBox")));
            this.HideSearchBox = ((System.Windows.Media.Animation.Storyboard)(this.FindName("HideSearchBox")));
            this.ShowControlsAnimation = ((System.Windows.Media.Animation.Storyboard)(this.FindName("ShowControlsAnimation")));
            this.HideControlsAnimation = ((System.Windows.Media.Animation.Storyboard)(this.FindName("HideControlsAnimation")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.Header = ((System.Windows.Controls.Grid)(this.FindName("Header")));
            this.YouTubeLogo = ((System.Windows.Controls.Image)(this.FindName("YouTubeLogo")));
            this.SearchContainer = ((System.Windows.Controls.Grid)(this.FindName("SearchContainer")));
            this.SearchTextBox = ((System.Windows.Controls.TextBox)(this.FindName("SearchTextBox")));
            this.SearchButton = ((System.Windows.Controls.Button)(this.FindName("SearchButton")));
            this.SearchIcon = ((System.Windows.Controls.Image)(this.FindName("SearchIcon")));
            this.VideoPlayer = ((System.Windows.Controls.MediaElement)(this.FindName("VideoPlayer")));
            this.AudioPlayer = ((System.Windows.Controls.MediaElement)(this.FindName("AudioPlayer")));
            this.IconAndStuffBackground = ((System.Windows.Controls.Border)(this.FindName("IconAndStuffBackground")));
            this.IconAndStuff = ((System.Windows.Controls.Grid)(this.FindName("IconAndStuff")));
            this.LoadingPanelPlayer = ((System.Windows.Controls.StackPanel)(this.FindName("LoadingPanelPlayer")));
            this.Bubble1Player = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble1Player")));
            this.Bubble2Player = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble2Player")));
            this.Bubble3Player = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble3Player")));
            this.PlayPauseIcon = ((System.Windows.Controls.Image)(this.FindName("PlayPauseIcon")));
            this.SkipTimeTextMinus = ((System.Windows.Controls.TextBlock)(this.FindName("SkipTimeTextMinus")));
            this.SkipTimeTextPlus = ((System.Windows.Controls.TextBlock)(this.FindName("SkipTimeTextPlus")));
            this.FullScreenIcon = ((System.Windows.Controls.Image)(this.FindName("FullScreenIcon")));
            this.ProgressSlider = ((System.Windows.Controls.Slider)(this.FindName("ProgressSlider")));
            this.StartTimeText = ((System.Windows.Controls.TextBlock)(this.FindName("StartTimeText")));
            this.EndTimeText = ((System.Windows.Controls.TextBlock)(this.FindName("EndTimeText")));
            this.VideoPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("VideoPivot")));
            this.LoadingPanelDel = ((System.Windows.Controls.StackPanel)(this.FindName("LoadingPanelDel")));
            this.Bubble1Del = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble1Del")));
            this.Bubble2Del = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble2Del")));
            this.Bubble3Del = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble3Del")));
            this.VideoTitle = ((System.Windows.Controls.TextBlock)(this.FindName("VideoTitle")));
            this.VideoViews = ((System.Windows.Controls.TextBlock)(this.FindName("VideoViews")));
            this.StuffPanel = ((System.Windows.Controls.Border)(this.FindName("StuffPanel")));
            this.ProfilePicture = ((System.Windows.Controls.Image)(this.FindName("ProfilePicture")));
            this.Username = ((System.Windows.Controls.TextBlock)(this.FindName("Username")));
            this.SubscriptionStatus = ((System.Windows.Controls.TextBlock)(this.FindName("SubscriptionStatus")));
            this.VideoUploadDate = ((System.Windows.Controls.TextBlock)(this.FindName("VideoUploadDate")));
            this.VideoDescription = ((System.Windows.Controls.RichTextBox)(this.FindName("VideoDescription")));
            this.CommentsItemControl = ((System.Windows.Controls.ItemsControl)(this.FindName("CommentsItemControl")));
            this.LoadingPanelCom = ((System.Windows.Controls.StackPanel)(this.FindName("LoadingPanelCom")));
            this.Bubble1Com = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble1Com")));
            this.Bubble2Com = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble2Com")));
            this.Bubble3Com = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble3Com")));
            this.LoadMore = ((System.Windows.Controls.Button)(this.FindName("LoadMore")));
            this.RealtedVideoItemControl = ((System.Windows.Controls.ItemsControl)(this.FindName("RealtedVideoItemControl")));
            this.LoadingPanelRel = ((System.Windows.Controls.StackPanel)(this.FindName("LoadingPanelRel")));
            this.Bubble1Rel = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble1Rel")));
            this.Bubble2Rel = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble2Rel")));
            this.Bubble3Rel = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble3Rel")));
        }
    }
}

