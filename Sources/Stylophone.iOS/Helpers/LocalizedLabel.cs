using System;
using Foundation;
using Stylophone.Localization.Strings;
using UIKit;

namespace Stylophone.iOS.Helpers
{
    [Register(nameof(LocalizedLabel))]
    public class LocalizedLabel: UILabel
    {

        [Export(nameof(stringIdentifier))]
        public NSString stringIdentifier { get; set; }

        public LocalizedLabel(IntPtr handle) : base(handle)
        {
        }

        void ReleaseDesignerOutlets()
        {
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            // Use the text set in IB to find the matching property.
            // Set the identifier in "User Defined Runtime Attributes".
            var identifier = stringIdentifier ?? "AppDisplayName";

            // Get the property value to have the localized string.
            Text = Resources.ResourceManager.GetString(identifier);
        }
    }
}

/*
  
  class LocalisableButton: UIButton {

    @IBInspectable var localisedKey: String? {
        didSet {
            guard let key = localisedKey else { return }
            UIView.performWithoutAnimation {
                setTitle(key.localized, for: .normal)
                layoutIfNeeded()
            }
        }
    }

}
 */