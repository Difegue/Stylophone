## Updating the iOS/macOS app icon  

Xamarin doesn't support Icon Composer yet, so we bundle an `Assets.car` built with actool.  

The About section of the Settings shows a render of the icon (`AboutIcon` in `Assets.xcassets`), so export that too when the icon changes:  

ICTOOL="$(xcode-select -p)/../Applications/Icon Composer.app/Contents/Executables/ictool"
for scale in 2 3; do "$ICTOOL" AppIcon.icon --export-image --output-file Assets.xcassets/AboutIcon.imageset/AboutIcon@${scale}x.png --platform iOS --rendition Default --width 64 --height 64 --scale $scale; done
mkdir -p /tmp/appicon
xcrun actool Assets.xcassets AppIcon.icon --app-icon AppIcon --platform iphoneos --target-device iphone --target-device ipad --minimum-deployment-target 16.0 --compile /tmp/appicon --output-partial-info-plist /tmp/appicon/partial.plist
cp /tmp/appicon/Assets.car .

The Mac app also bundles `AppIcon.icns`, which the Mac App Store requires:  

mkdir -p /tmp/AppIcon.iconset
"$ICTOOL" AppIcon.icon --export-image --output-file /tmp/appicon-full.png --platform macOS --rendition Default --width 512 --height 512 --scale 2
sips -Z 824 /tmp/appicon-full.png --out /tmp/appicon-body.png
sips --padToHeightWidth 1024 1024 /tmp/appicon-body.png --out /tmp/AppIcon.iconset/icon_512x512@2x.png
for size in 16 32 128 256 512; do sips -z $size $size /tmp/AppIcon.iconset/icon_512x512@2x.png --out /tmp/AppIcon.iconset/icon_${size}x${size}.png; done
for size in 16 32 128 256; do sips -z $((size * 2)) $((size * 2)) /tmp/AppIcon.iconset/icon_512x512@2x.png --out /tmp/AppIcon.iconset/icon_${size}x${size}@2x.png; done
iconutil -c icns /tmp/AppIcon.iconset -o AppIcon.icns
