## Updating the iOS/macOS app icon  

Xamarin doesn't support Icon Composer yet, so we bundle an `Assets.car` built with actool.  

The About section of the Settings shows a render of the icon (`AboutIcon` in `Assets.xcassets`), so export that too when the icon changes:  

ICTOOL="$(xcode-select -p)/../Applications/Icon Composer.app/Contents/Executables/ictool"
for scale in 2 3; do "$ICTOOL" AppIcon.icon --export-image --output-file Assets.xcassets/AboutIcon.imageset/AboutIcon@${scale}x.png --platform iOS --rendition Default --width 64 --height 64 --scale $scale; done
mkdir -p /tmp/appicon
xcrun actool Assets.xcassets AppIcon.icon --app-icon AppIcon --platform iphoneos --target-device iphone --target-device ipad --minimum-deployment-target 16.0 --compile /tmp/appicon --output-partial-info-plist /tmp/appicon/partial.plist
cp /tmp/appicon/Assets.car .
