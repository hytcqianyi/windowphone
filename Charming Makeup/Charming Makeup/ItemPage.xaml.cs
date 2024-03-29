﻿using Charming_Makeup.Common;
using Charming_Makeup.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using System.Text;
using Windows.Storage.Streams;
using Windows.Media.Capture;
using Windows.Storage;


// 有关“项页”项模板的信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=234232

namespace Charming_Makeup
{
    /// <summary>
    /// 显示组内某一项的详细信息的页面。
    /// </summary>
    public sealed partial class ItemPage : Page
    {
        private async void OnCapturePhoto(object sender, RoutedEventArgs e)
        {
            var camera = new CameraCaptureUI();
            var file = await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);

            if (file != null)
            {
                var item = (SampleDataItem)this.DefaultViewModel["Item"];
                item.Media.Add(file);

                _photo = file;
                DataTransferManager.ShowShareUI();
            }
        }



        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();
        private StorageFile _photo; // Photo file to share

        /// <summary>
        /// NavigationHelper 在每页上用于协助导航和
        /// 进程生命期管理
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// 可将其更改为强类型视图模型。
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public ItemPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.SizeChanged += ItemPage_SizeChanged;
        }

        /// <summary>
        /// 使用在导航过程中传递的内容填充页。  在从以前的会话
        /// 重新创建页时，也会提供任何已保存状态。
        /// </summary>
        /// <param name="sender">
        /// 事件的来源；通常为 <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">事件数据，其中既提供在最初请求此页时传递给
        /// <see cref="Frame.Navigate(Type, Object)"/> 的导航参数，又提供
        /// 此页在以前会话期间保留的状态的
        /// 字典。首次访问页面时，该状态将为 null。</param>
        private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: 创建适用于问题域的合适数据模型以替换示例数据
            var item = await SampleDataSource.GetItemAsync((String)e.NavigationParameter);
            this.DefaultViewModel["Item"] = item;
        }

        void ItemPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width <= 800)
            {
                VisualStateManager.GoToState(this, "NarrowLayout", true);
            }
            else if (e.NewSize.Width > 800 && e.NewSize.Width <= 1250)
            {
                VisualStateManager.GoToState(this, "IntermediateLayout", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "DefaultLayout", true);
            }
        }

        //void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        //{
        //    var request = args.Request;
        //    var item = (SampleDataItem)this.DefaultViewModel["Item"];
        //    request.Data.Properties.Title = item.Title;
        //    request.Data.Properties.Description = "Recipe ingredients and directions";

        //    //// Share recipe text
        //    var recipe = "\r\nINGREDIENTS\r\n";
        //    recipe += String.Join("\r\n", item.Ingredients);
        //    recipe += ("\r\n\r\nDIRECTIONS\r\n" + item.Content);
        //    request.Data.SetText(recipe);

        //    // Share recipe image
        //    var reference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///" + item.ImagePath));
        //    request.Data.Properties.Thumbnail = reference;
        //    request.Data.SetBitmap(reference);
        //}

        void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            var item = (SampleDataItem)this.DefaultViewModel["Item"];
            request.Data.Properties.Title = item.Title;

            if (_photo != null)
            {
                request.Data.Properties.Description = "Recipe photo";
                var reference = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromFile(_photo);
                request.Data.Properties.Thumbnail = reference;
                request.Data.SetBitmap(reference);
                _photo = null;
            }

            else
            {
                request.Data.Properties.Description = "Recipe ingredients and directions";

                // Share recipe text
                var recipe = "\r\nINGREDIENTS\r\n";
                recipe += String.Join("\r\n", item.Ingredients);
                recipe += ("\r\n\r\nDIRECTIONS\r\n" + item.Content);
                request.Data.SetText(recipe);

                // Share recipe image
                var reference = RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///" + item.ImagePath));
                request.Data.Properties.Thumbnail = reference;
                request.Data.SetBitmap(reference);
            }
        }

        #region NavigationHelper 注册

        /// 此部分中提供的方法只是用于使
        /// NavigationHelper 可响应页面的导航方法。
        /// 
        /// 应将页面特有的逻辑放入用于
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// 和 <see cref="GridCS.Common.NavigationHelper.SaveState"/> 的事件处理程序中。
        /// 除了在会话期间保留的页面状态之外
        /// LoadState 方法中还提供导航参数。


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
            // Register for DataRequested events
            DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
            // Deregister the DataRequested event handler
            DataTransferManager.GetForCurrentView().DataRequested -= OnDataRequested;
        }

        #endregion

        //private void OnCapturePhoto(object sender, RoutedEventArgs e)
        //{
        //    DataTransferManager.ShowShareUI();
        //}

        private void OnCaptureVideo(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }




    }
}