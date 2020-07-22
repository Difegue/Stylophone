using System;
using System.Threading.Tasks;
using System.Xml;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Notifications;

using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace FluentMPC.Services
{
    internal partial class LiveTileService
    {
        public void UpdatePlayingSong(TrackViewModel track)
        {
            var title = track.Name;
            var artist = track.File.Artist;
            var album = track.File.Album;

            var f = track.File;
            var uniqueIdentifier = f.HasAlbum ? f.Album : f.HasTitle ? f.Title : f.Path;

            // Use the cached albumart
            var artUri = $"ms-appdata:///local/AlbumArt/{uniqueIdentifier}";

            // Construct the tile content
            // TODO: Rework typography
            var content = new TileContent()
            {
                
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = artUri
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title
                                },
                                new AdaptiveText()
                                {
                                    Text = artist,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                                new AdaptiveText()
                                {
                                    Text = album,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },

                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = artUri
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = artist,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                                new AdaptiveText()
                                {
                                    Text = album,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    },

                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = artUri
                            },
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Subtitle
                                },
                                new AdaptiveText()
                                {
                                    Text = artist,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                },
                                new AdaptiveText()
                                {
                                    Text = album,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle
                                }
                            }
                        }
                    }
                }
            };

            // Then create the tile notification
            var notification = new TileNotification(content.GetXml());
            UpdateTile(notification);
        }
    }
}
