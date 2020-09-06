using System;
using System.Threading.Tasks;
using System.Xml;
using FluentMPC.Helpers;
using FluentMPC.ViewModels.Items;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Storage;
using Windows.UI.Notifications;

namespace FluentMPC.Services
{
    internal partial class LiveTileService
    {

        public async void UpdatePlayingSong(TrackViewModel track)
        {

            var title = track.Name;
            var artist = track.File.Artist;
            var album = track.File.Album;

            var f = track.File;
            var uniqueIdentifier = f.HasAlbum ? f.Album : f.HasTitle ? f.Title : f.Path;
            uniqueIdentifier = MiscHelpers.EscapeFilename(uniqueIdentifier);

            // Use the cached albumart if it exists
            var artUri = $"ms-appdata:///local/AlbumArt/{uniqueIdentifier}";

            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("AlbumArt", CreationCollisionOption.OpenIfExists);

            if (!await pictureFolder.FileExistsAsync(uniqueIdentifier))
            {
                artUri = "ms-appx:///Assets/AlbumPlaceholder.png";
            }

            // Construct the tile content
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = title,
                        HintStyle = AdaptiveTextStyle.Base,
                        HintWrap = true,
                        HintMaxLines = 2,
                        HintAlign = AdaptiveTextAlign.Center
                    },
                    new AdaptiveText()
                    {
                        Text = artist,
                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                        HintAlign = AdaptiveTextAlign.Center
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = artUri,
                                HintOverlay = 70
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 33,
                                Children =
                                {
                                    new AdaptiveImage()
                                    {
                                       Source = artUri
                                    }
                                }
                            },
                            new AdaptiveSubgroup()
                            {
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = title,
                                        HintStyle = AdaptiveTextStyle.Subtitle,
                                        HintWrap = true,
                                        HintMaxLines = 2
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
                                },
                                HintTextStacking = AdaptiveSubgroupTextStacking.Center
                            }
                        }
                    }
                }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Branding = TileBranding.NameAndLogo,
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Top,
                            Children =
                {
                    new AdaptiveGroup()
                    {
                        Children =
                        {
                            new AdaptiveSubgroup()
                            {
                                HintWeight = 33,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = title,
                                        HintStyle = AdaptiveTextStyle.Title,
                                        HintWrap = true,
                                        HintAlign = AdaptiveTextAlign.Left
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = artist,
                                        HintStyle = AdaptiveTextStyle.SubtitleSubtle,
                                        HintAlign = AdaptiveTextAlign.Left
                                    }
                                }
                            }
                        }
                    },
                    new AdaptiveText()
                    {
                        Text = album,
                        HintStyle = AdaptiveTextStyle.BaseSubtle,
                        HintWrap = true,
                        HintAlign = AdaptiveTextAlign.Left
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = artUri,
                                HintOverlay = 70
                            }
                        }
                    }
                }
            };

            // Create the tile notification
            var xml = tileContent.GetXml();
            var xmlStr = xml.GetXml();

            var tileNotif = new TileNotification(xml);
            tileNotif.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(track.File.Time);

            // And send the notification to the primary tile
            UpdateTile(tileNotif);
        }
    }
}
