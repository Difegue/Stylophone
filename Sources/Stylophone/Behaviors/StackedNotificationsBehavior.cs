// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// TODO: Remove this when StackedNotificationsBehavior lands in Toolkit

using Microsoft.Toolkit.Uwp.UI.Behaviors;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using DQ = Windows.System.DispatcherQueue;

namespace CommunityToolkit.Labs.WinUI
{
    /// <summary>
    /// The content of a notification to display in <see cref="StackedNotificationsBehavior"/>.
    /// The <see cref="Title"/>, <see cref="Message"/>, <see cref="Duration"/> and <see cref="Severity"/> values will
    /// always be applied to the targeted <see cref="InfoBar"/>.
    /// The <see cref="IsIconVisible"/>, <see cref="Content"/>, <see cref="ContentTemplate"/> and <see cref="ActionButton"/> values
    /// will be applied only if set.
    /// </summary>
    public class Notification
    {
        private NotificationOverrides _overrides;
        private bool _isIconVisible;
        private object? _content;
        private DataTemplate? _contentTemplate;
        private ButtonBase? _actionButton;

        /// <summary>
        /// Gets or sets the notification title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the notification message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the duration of the notification.
        /// Set to null for persistent notification.
        /// </summary>
        public TimeSpan? Duration { get; set; }

        /// <summary>
        /// Gets or sets the type of the <see cref="InfoBar"/> to apply consistent status color, icon,
        /// and assistive technology settings dependent on the criticality of the notification.
        /// </summary>
        public InfoBarSeverity Severity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the icon is visible or not.
        /// True if the icon is visible; otherwise, false. The default is true.
        /// </summary>
        public bool IsIconVisible
        {
            get => _isIconVisible;
            set
            {
                _isIconVisible = value;
                _overrides |= NotificationOverrides.Icon;
            }
        }

        /// <summary>
        /// Gets or sets the XAML Content that is displayed below the title and message in
        ///  the InfoBar.
        /// </summary>
        public object? Content
        {
            get => _content;
            set
            {
                _content = value;
                _overrides |= NotificationOverrides.Content;
            }
        }

        /// <summary>
        /// Gets or sets the data template for the <see cref="Content"/>.
        /// </summary>
        public DataTemplate? ContentTemplate
        {
            get => _contentTemplate;
            set
            {
                _contentTemplate = value;
                _overrides |= NotificationOverrides.ContentTemplate;
            }
        }

        /// <summary>
        /// Gets or sets the action button of the InfoBar.
        /// </summary>
        public ButtonBase? ActionButton
        {
            get => _actionButton;
            set
            {
                _actionButton = value;
                _overrides |= NotificationOverrides.ActionButton;
            }
        }

        internal NotificationOverrides Overrides => _overrides;
    }

    /// <summary>
    /// The overrides which should be set on the notification.
    /// </summary>
    [Flags]
    internal enum NotificationOverrides
    {
        None,
        Icon,
        Content,
        ContentTemplate,
        ActionButton,
    }

    /// <summary>
    /// A behavior to add the stacked notification support to <see cref="InfoBar"/>.
    /// </summary>
    public class StackedNotificationsBehavior : BehaviorBase<InfoBar>
    {
        private readonly LinkedList<Notification> _stackedNotifications;
        private readonly DispatcherQueueTimer _dismissTimer;
        private Notification? _currentNotification;
        private bool _initialIconVisible;
        private object? _initialContent;
        private DataTemplate? _initialContentTemplate;
        private ButtonBase? _initialActionButton;

        /// <summary>
        /// Initializes a new instance of the <see cref="StackedNotificationsBehavior"/> class.
        /// </summary>
        public StackedNotificationsBehavior()
        {
            _stackedNotifications = new LinkedList<Notification>();

            // TODO: On WinUI 3 we can use the local DispatcherQueue, so we need to abstract better for UWP
            var dispatcherQueue = DQ.GetForCurrentThread();
            _dismissTimer = dispatcherQueue.CreateTimer();
            _dismissTimer.Tick += OnTimerTick;
        }

        /// <summary>
        /// Show <paramref name="notification"/>.
        /// </summary>
        /// <param name="notification">The notification to display.</param>
        public void Show(Notification notification)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            _stackedNotifications.AddLast(notification);
            ShowNext();
        }

        /// <summary>
        /// Remove the <paramref name="notification"/>.
        /// If the notification is displayed, it will be closed.
        /// If the notification is still in the queue, it will be removed.
        /// </summary>
        /// <param name="notification">The notification to remove.</param>
        public void Remove(Notification notification)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (notification == _currentNotification)
            {
                // We close the notification. This will trigger the display of the next one.
                // See OnInfoBarClosed.
                AssociatedObject.IsOpen = false;
                return;
            }

            _stackedNotifications.Remove(notification);
        }

        /// <inheritdoc/>
        protected override bool Initialize()
        {
            AssociatedObject.Closed += OnInfoBarClosed;
            AssociatedObject.PointerEntered += OnPointerEntered;
            AssociatedObject.PointerExited += OnPointedExited;
            return true;
        }

        /// <inheritdoc/>
        protected override bool Uninitialize()
        {
            AssociatedObject.Closed -= OnInfoBarClosed;
            AssociatedObject.PointerEntered -= OnPointerEntered;
            AssociatedObject.PointerExited -= OnPointedExited;
            return true;
        }

        private void OnInfoBarClosed(InfoBar sender, InfoBarClosedEventArgs args)
        {
            // We display the next notification.
            ShowNext();
        }

        private void ShowNext()
        {
            if (AssociatedObject.IsOpen)
            {
                // One notification is already displayed. We wait for it to close
                return;
            }

            StopTimer();
            AssociatedObject.IsOpen = false;
            RestoreOverridenProperties();

            if (_stackedNotifications.Count == 0)
            {
                _currentNotification = null;
                return;
            }

            var notificationToDisplay = _stackedNotifications!.First!.Value;
            _stackedNotifications.RemoveFirst();

            _currentNotification = notificationToDisplay;
            SetNotification(notificationToDisplay);
            AssociatedObject.IsOpen = true;

            StartTimer(notificationToDisplay.Duration);
        }

        private void SetNotification(Notification notification)
        {
            AssociatedObject.Title = notification.Title ?? string.Empty;
            AssociatedObject.Message = notification.Message ?? string.Empty;
            AssociatedObject.Severity = notification.Severity;

            if (notification.Overrides.HasFlag(NotificationOverrides.Icon))
            {
                _initialIconVisible = AssociatedObject.IsIconVisible;
                AssociatedObject.IsIconVisible = notification.IsIconVisible;
            }

            if (notification.Overrides.HasFlag(NotificationOverrides.Content))
            {
                _initialContent = AssociatedObject.Content;
                AssociatedObject.Content = notification.Content!;
            }

            if (notification.Overrides.HasFlag(NotificationOverrides.ContentTemplate))
            {
                _initialContentTemplate = AssociatedObject.ContentTemplate;
                AssociatedObject.ContentTemplate = notification.ContentTemplate!;
            }

            if (notification.Overrides.HasFlag(NotificationOverrides.ActionButton))
            {
                _initialActionButton = AssociatedObject.ActionButton;
                AssociatedObject.ActionButton = notification.ActionButton!;
            }
        }

        private void RestoreOverridenProperties()
        {
            if (_currentNotification is null)
            {
                return;
            }

            if (_currentNotification.Overrides.HasFlag(NotificationOverrides.Icon))
            {
                AssociatedObject.IsIconVisible = _initialIconVisible;
            }

            if (_currentNotification.Overrides.HasFlag(NotificationOverrides.Content))
            {
                AssociatedObject.Content = _initialContent!;
            }

            if (_currentNotification.Overrides.HasFlag(NotificationOverrides.ContentTemplate))
            {
                AssociatedObject.ContentTemplate = _initialContentTemplate!;
            }

            if (_currentNotification.Overrides.HasFlag(NotificationOverrides.ActionButton))
            {
                AssociatedObject.ActionButton = _initialActionButton!;
            }
        }

        private void StartTimer(TimeSpan? duration)
        {
            if (duration is null)
            {
                return;
            }

            _dismissTimer.Interval = duration.Value;
            _dismissTimer.Start();
        }

        private void StopTimer() => _dismissTimer.Stop();

        private void OnTimerTick(DispatcherQueueTimer sender, object args) => AssociatedObject.IsOpen = false;

        private void OnPointedExited(object sender, PointerRoutedEventArgs e) => StartTimer(_currentNotification?.Duration);

        private void OnPointerEntered(object sender, PointerRoutedEventArgs e) => StopTimer();
    }

}

