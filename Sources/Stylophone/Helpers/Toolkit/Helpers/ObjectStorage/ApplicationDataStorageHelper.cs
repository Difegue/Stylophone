// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

#nullable enable

// Stylophone: trimmed-down vendored copy. The upstream type implements
// Microsoft.Toolkit.Helpers.IFileStorageHelper / ISettingsStorageHelper from the
// CommunityToolkit.Common package which we don't reference. SystemInformation only
// calls Read/Save/KeyExists, so the file/folder helpers and composite-settings
// overloads are dropped. If a future caller needs them, port back from
// https://github.com/CommunityToolkit/WindowsCommunityToolkit (MIT).
namespace Microsoft.Toolkit.Uwp.Helpers
{
    /// <summary>
    /// Storage helper for files and folders living in Windows.Storage.ApplicationData storage endpoints.
    /// </summary>
    public partial class ApplicationDataStorageHelper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDataStorageHelper"/> class.
        /// </summary>
        /// <param name="appData">The data store to interact with.</param>
        /// <param name="objectSerializer">Serializer for converting stored values. Defaults to <see cref="SystemSerializer"/>.</param>
        public ApplicationDataStorageHelper(ApplicationData appData, IObjectSerializer? objectSerializer = null)
        {
            AppData = appData ?? throw new ArgumentNullException(nameof(appData));
            Serializer = objectSerializer ?? new SystemSerializer();
        }

        /// <summary>
        /// Gets the settings container.
        /// </summary>
        public ApplicationDataContainer Settings => AppData.LocalSettings;

        /// <summary>
        ///  Gets the storage folder.
        /// </summary>
        public StorageFolder Folder => AppData.LocalFolder;

        /// <summary>
        /// Gets the storage host.
        /// </summary>
        protected ApplicationData AppData { get; }

        /// <summary>
        /// Gets the serializer for converting stored values.
        /// </summary>
        protected IObjectSerializer Serializer { get; }

        /// <summary>
        /// Get a new instance using ApplicationData.Current and the provided serializer.
        /// </summary>
        /// <param name="objectSerializer">Serializer for converting stored values. Defaults to <see cref="SystemSerializer"/>.</param>
        /// <returns>A new instance of ApplicationDataStorageHelper.</returns>
        public static ApplicationDataStorageHelper GetCurrent(IObjectSerializer? objectSerializer = null)
        {
            var appData = ApplicationData.Current;
            return new ApplicationDataStorageHelper(appData, objectSerializer);
        }

        /// <summary>
        /// Get a new instance using the ApplicationData for the provided user and serializer.
        /// </summary>
        /// <param name="user">App data user owner.</param>
        /// <param name="objectSerializer">Serializer for converting stored values. Defaults to <see cref="SystemSerializer"/>.</param>
        /// <returns>A new instance of ApplicationDataStorageHelper.</returns>
        public static async Task<ApplicationDataStorageHelper> GetForUserAsync(User user, IObjectSerializer? objectSerializer = null)
        {
            var appData = await ApplicationData.GetForUserAsync(user);
            return new ApplicationDataStorageHelper(appData, objectSerializer);
        }

        /// <summary>
        /// Determines whether a setting already exists.
        /// </summary>
        public bool KeyExists(string key)
        {
            return Settings.Values.ContainsKey(key);
        }

        /// <summary>
        /// Retrieves a single item by its key.
        /// </summary>
        public T? Read<T>(string key, T? @default = default)
        {
            if (Settings.Values.TryGetValue(key, out var valueObj))
            {
                if (valueObj is string valueString)
                {
                    return Serializer.Deserialize<T>(valueString);
                }

                // SystemSerializer round-trips primitives as their native CLR type, so accept them directly too.
                if (valueObj is T direct)
                {
                    return direct;
                }

                try
                {
                    return Serializer.Deserialize<T>(valueObj);
                }
                catch
                {
                    // fall through to default
                }
            }

            return @default;
        }

        public bool TryRead<T>(string key, out T? value)
        {
            if (Settings.Values.TryGetValue(key, out var valueObj))
            {
                if (valueObj is string valueString)
                {
                    value = Serializer.Deserialize<T>(valueString);
                    return true;
                }

                if (valueObj is T direct)
                {
                    value = direct;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public void Save<T>(string key, T value)
        {
            Settings.Values[key] = Serializer.Serialize(value);
        }

        public bool TryDelete(string key)
        {
            return Settings.Values.Remove(key);
        }

        public void Clear()
        {
            Settings.Values.Clear();
        }
    }
}
