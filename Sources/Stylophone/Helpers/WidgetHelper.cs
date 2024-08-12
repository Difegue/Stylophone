using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.Helpers;
using Stylophone.Common.Helpers;
using Stylophone.Common.ViewModels;
using Widgetopia.Core;
using Windows.Storage;

namespace Stylophone.Services
{
    public static class WidgetHelper
    {
        public async static Task UpdatePlayingSongAsync(TrackViewModel track)
        {
            var title = track.Name;
            var artist = track.File.Artist;
            var album = track.File.Album;

            var f = track.File;
            var uniqueIdentifier = Miscellaneous.GetFileIdentifier(f);

            // Use the cached albumart if it exists
            var artUri = $"ms-appdata:///local/AlbumArt/{uniqueIdentifier}";

            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);

            if (!await pictureFolder.FileExistsAsync(uniqueIdentifier))
            {
                artUri = "ms-appx:///Assets/AlbumPlaceholder.png";
            }

            var widgetTemplate = PayloadSender.ReadPackageFileFromUri("ms-appx:///Assets/widgetTemplate.json");

            // Construct the widget payload
            var widgetData = new List<WidgetData>
            {
                new()
                {
                    Key = "stylophone_title",
                    Type = DataType.STRING,
                    Value = title
                },
                new()
                {
                    Key = "stylophone_artist",
                    Type = DataType.STRING,
                    Value = artist
                },
                new()
                {
                    Key = "stylophone_album",
                    Type = DataType.STRING,
                    Value = album
                },
                new()
                {
                    Key = "stylophone_albumArt",
                    Type = DataType.IMAGE,
                    Value = artUri
                }
            };

            var payload = new WidgetPayload
            {
                AppName = "Stylophone",
                Id = "stylophone",
                Templates = new string[] { widgetTemplate },
                ActionCallbackUrl = "stylophone:",
                Base64Icon = "",
                Data = widgetData.ToArray()
            };

            // Send data to widget app service
            var result = await PayloadSender.SendDataAsync(payload);
        }

    }
}
