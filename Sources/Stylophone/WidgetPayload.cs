using System.Text.Json;

namespace Widgetopia.Core;

public record WidgetPayload
{
    /// <summary>
    /// Unique ID for the App sending the payload.
    /// Make sure to preface your data keys with this ID.
    /// e.g, with an ID of "mycoolapp", "mycoolapp_mydata".
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// Friendly name for the App to show on the UI
    /// </summary>
    public string AppName { get; init; }

    /// <summary>
    /// Base64-encoded icon for the App
    /// </summary>
    public string? Base64Icon { get; init; }

    /// <summary>
    /// Prebuilt Widget Templates for the App
    /// </summary>
    public string[]? Templates { get; init; }

    /// <summary>
    /// Up-to-date data you want to show in widgets.
    /// Make sure to prepend your data keys with the app ID, eg "mycoolapp_mydata".
    /// </summary>
    public WidgetData[] Data { get; init; }

    /// <summary>
    /// A URL Widgetopia should invoke when widget actions are executed.
    /// If the action has a verb, it'll be appended to the end of the URL.
    /// </summary>
    public string ActionCallbackUrl { get; init; }
}

public enum DataType
{
    STRING,
    BOOL,
    INT,
    FLOAT,
    IMAGE // URL or path to an image. Widgetopia doesn't cache images and will pass this directly to the widget renderer.
}

public record WidgetData
{
    public DataType Type { get; init; }
    public string Key { get; init; }
    public string Value { get; init; }
}
