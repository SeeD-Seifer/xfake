## Setup

1. Copy folder `xfake` to solution root
1. Open `xfake/build-config.fsx` and update the next properties: `solutionName`, `keystorePath`, `keystoreAlias`
1. Find `xamarin-components.exe` somewhere and add to `xfake` folder
1. Add two scripts to the root

publish-android.sh
```
sh ./xfake/build.sh android-package -ev HockeyAppApiToken <hockey-app-key> -ev KeystorePassword <keystore-password>
```

publish-ios.sh
```
sh ./xfake/build.sh ios-adhoc -ev HockeyAppApiToken <hockey-app-key>
```
