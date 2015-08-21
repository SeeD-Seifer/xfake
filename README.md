## Setup

1. Copy folder `xfake` to solution root
1. Open `xfake/build-config.fsx` and update the next properties: `solutionName`, `keystorePath`, `keystoreAlias`
1. Add two scripts to the root (add these scripts to .gitignore)

publish-android.sh
```
sh ./xfake/build.sh android-package -ev HockeyAppApiToken <hockey-app-key> -ev KeystorePassword <keystore-password>
```

publish-ios.sh
```
sh ./xfake/build.sh ios-adhoc -ev HockeyAppApiToken <hockey-app-key>
```

## Troubleshooting

#### System.ComponentModel.Win32Exception: ApplicationName='zipalign', CommandLine='-f -v 4 path-to-signed.apk path-to-SignedAndAligned.apk ', CurrentDirectory='current-dir', Native error= Cannot find the specified file

* Copy `android-sdk/builds-tools/version/zipalign` to `android-sdk/platform-tools`.
* Ensure `android-sdk/platform-tools` folder added to PATH and command `zipalign` works from terminal. See  http://stackoverflow.com/a/7609388/4478393

#### WARNING (restore): You don't appear to have credentials for components.xamarin.com saved in your cookie jar! ERROR (restore): No credentials and didn't want to run `xamarin-component login'. Cannot proceed.

* Run `xamarin-components.exe` manually once: `mono ./xfake/xamarin-component.exe restore MyApp.sln`. And provide Xamarin login/password
