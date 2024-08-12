using System;
using Windows.ApplicationModel.AppService;
using Windows.Storage;
using Windows.Foundation.Collections;
using System.Text.Json;
using System.Threading.Tasks;

namespace Widgetopia.Core;
public static class PayloadSender
{
    public const string APPSERVICE_NAME = "com.widgetopia.service";
    public const string PACKAGE_FAMILYNAME = "047166cb-9231-4829-8c73-3227576cbf91_sm6ssk3qw71mj";
    public static async Task<ValueSet> SendDataAsync(WidgetPayload payload)
    {
        var message = new ValueSet();
        var result = new ValueSet();
        var connection = new AppServiceConnection()
        {
            // Here, we use the app service name defined in the app service 
            // provider's Package.appxmanifest file in the <Extension> section.
            AppServiceName = APPSERVICE_NAME,
            // Use Windows.ApplicationModel.Package.Current.Id.FamilyName 
            // within the app service provider to get this value.
            PackageFamilyName = PACKAGE_FAMILYNAME
        };

        var status = await connection.OpenAsync();

        if (status != AppServiceConnectionStatus.Success)
        {
            result.Add("Status", false);
            result.Add("Error", "Failed to connect");
            return result;
        }

        try
        {
            // Serialize payload and send it to service
            message.Add("WidgetPayload", JsonSerializer.Serialize(payload));
            AppServiceResponse response = await connection.SendMessageAsync(message);

            if (response.Status == AppServiceResponseStatus.Success)
            {
                // Get the data that the service sent to us.
                return response.Message;
            }
            else
            {
                result.Add("Status", false);
                result.Add("Error", "Incorrect response received from app service (" + response.Status + ")");
            }
        }
        catch (Exception e)
        {
            result.Add("Status", false);
            result.Add("Error", "Error while communicating with app service");
            result.Add("Exception", e);
        }

        return result;

    }

    public static string ReadPackageFileFromUri(string packageUri)
    {
        try
        {
            var uri = new Uri(packageUri);
            var storageFileTask = StorageFile.GetFileFromApplicationUriAsync(uri).AsTask();
            storageFileTask.Wait();

            var readTextTask = FileIO.ReadTextAsync(storageFileTask.Result).AsTask();
            readTextTask.Wait();

            return readTextTask.Result;
        }
        catch (Exception e)
        {
            return e.ToString();
        }
        
    }

}
