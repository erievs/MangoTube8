﻿#pragma checksum "C:\Users\Nicholas Bryan Brook\documents\visual studio 2015\Projects\MangoTube8\MangoTube8\MainPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "BF2F8576CED38301FB918A2111E220E2"
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
    
    
    public partial class MainPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Media.Animation.Storyboard ShowSearchBox;
        
        internal System.Windows.Media.Animation.Storyboard HideSearchBox;
        
        internal System.Windows.Media.Animation.Storyboard BubbleLoadingAnimationRec;
        
        internal System.Windows.Media.Animation.Storyboard BubbleLoadingAnimationTrending;
        
        internal System.Windows.Media.Animation.Storyboard BubbleLoadingAnimationSpotlight;
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Image YouTubeLogo;
        
        internal System.Windows.Controls.Grid SearchContainer;
        
        internal System.Windows.Controls.TextBox SearchTextBox;
        
        internal System.Windows.Controls.Button SearchButton;
        
        internal System.Windows.Controls.Image SearchIcon;
        
        internal Microsoft.Phone.Controls.Pivot MainPivot;
        
        internal System.Windows.Controls.StackPanel LoadingPanelRecommended;
        
        internal System.Windows.Shapes.Ellipse Bubble1Rec;
        
        internal System.Windows.Shapes.Ellipse Bubble2Rec;
        
        internal System.Windows.Shapes.Ellipse Bubble3Rec;
        
        internal System.Windows.Controls.ItemsControl RecommendedItemsControl;
        
        internal System.Windows.Controls.Button LoadMoreRec;
        
        internal System.Windows.Controls.StackPanel LoadingPanelTrending;
        
        internal System.Windows.Shapes.Ellipse Bubble1Trend;
        
        internal System.Windows.Shapes.Ellipse Bubble2Trend;
        
        internal System.Windows.Shapes.Ellipse Bubble3Trend;
        
        internal System.Windows.Controls.ItemsControl TrendingItemsControl;
        
        internal System.Windows.Controls.Button LoadMore;
        
        internal System.Windows.Controls.StackPanel LoadingPanelSpot;
        
        internal System.Windows.Shapes.Ellipse Bubble1Spot;
        
        internal System.Windows.Shapes.Ellipse Bubble2Spot;
        
        internal System.Windows.Shapes.Ellipse Bubble3Spot;
        
        internal System.Windows.Controls.ItemsControl SpotlightItemsControl;
        
        internal System.Windows.Controls.Button LoadMoreSpot;
        
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
            System.Windows.Application.LoadComponent(this, new System.Uri("/MangoTube8;component/MainPage.xaml", System.UriKind.Relative));
            this.ShowSearchBox = ((System.Windows.Media.Animation.Storyboard)(this.FindName("ShowSearchBox")));
            this.HideSearchBox = ((System.Windows.Media.Animation.Storyboard)(this.FindName("HideSearchBox")));
            this.BubbleLoadingAnimationRec = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BubbleLoadingAnimationRec")));
            this.BubbleLoadingAnimationTrending = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BubbleLoadingAnimationTrending")));
            this.BubbleLoadingAnimationSpotlight = ((System.Windows.Media.Animation.Storyboard)(this.FindName("BubbleLoadingAnimationSpotlight")));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.YouTubeLogo = ((System.Windows.Controls.Image)(this.FindName("YouTubeLogo")));
            this.SearchContainer = ((System.Windows.Controls.Grid)(this.FindName("SearchContainer")));
            this.SearchTextBox = ((System.Windows.Controls.TextBox)(this.FindName("SearchTextBox")));
            this.SearchButton = ((System.Windows.Controls.Button)(this.FindName("SearchButton")));
            this.SearchIcon = ((System.Windows.Controls.Image)(this.FindName("SearchIcon")));
            this.MainPivot = ((Microsoft.Phone.Controls.Pivot)(this.FindName("MainPivot")));
            this.LoadingPanelRecommended = ((System.Windows.Controls.StackPanel)(this.FindName("LoadingPanelRecommended")));
            this.Bubble1Rec = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble1Rec")));
            this.Bubble2Rec = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble2Rec")));
            this.Bubble3Rec = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble3Rec")));
            this.RecommendedItemsControl = ((System.Windows.Controls.ItemsControl)(this.FindName("RecommendedItemsControl")));
            this.LoadMoreRec = ((System.Windows.Controls.Button)(this.FindName("LoadMoreRec")));
            this.LoadingPanelTrending = ((System.Windows.Controls.StackPanel)(this.FindName("LoadingPanelTrending")));
            this.Bubble1Trend = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble1Trend")));
            this.Bubble2Trend = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble2Trend")));
            this.Bubble3Trend = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble3Trend")));
            this.TrendingItemsControl = ((System.Windows.Controls.ItemsControl)(this.FindName("TrendingItemsControl")));
            this.LoadMore = ((System.Windows.Controls.Button)(this.FindName("LoadMore")));
            this.LoadingPanelSpot = ((System.Windows.Controls.StackPanel)(this.FindName("LoadingPanelSpot")));
            this.Bubble1Spot = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble1Spot")));
            this.Bubble2Spot = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble2Spot")));
            this.Bubble3Spot = ((System.Windows.Shapes.Ellipse)(this.FindName("Bubble3Spot")));
            this.SpotlightItemsControl = ((System.Windows.Controls.ItemsControl)(this.FindName("SpotlightItemsControl")));
            this.LoadMoreSpot = ((System.Windows.Controls.Button)(this.FindName("LoadMoreSpot")));
        }
    }
}

