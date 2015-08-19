module BuildHelpers
#load "build-config.fsx"

open Fake
open Fake.XamarinHelper
open Fake.HockeyAppHelper
open System
open System.IO
open System.Linq
open BuildConfig

let Exec command args =
    let result = Shell.Exec(command, args)
    if result <> 0 then failwithf "%s exited with error %d" command result

let RestorePackages solutionFile =
    Exec "nuget" ("restore " + solutionFile)
    solutionFile |> RestoreComponents (fun defaults -> {defaults with ToolPath = "xfake/xamarin-component.exe" })

let RunNUnitTests dllPath xmlPath =
    Exec "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/nunit-console.exe" (dllPath + " -xml=" + xmlPath)
    //TeamCityHelper.sendTeamCityNUnitImport xmlPath

let CommitVersionChange file message =
    let args = String.Format("commit -m \"{0}\" -- {1}", message, file)
    Exec "git" args

/// Version/BuildNumber is updated only when it is not empty
let SetTouchVersion version buildNumber =
    let path = Path.Combine(projectNameIOS, "Info.plist")
    if not (isNullOrEmpty version) then
      Exec "/usr/libexec/PlistBuddy" ("-c 'Set :CFBundleShortVersionString " + version + "' " + path)
    if not (isNullOrEmpty buildNumber) then
      Exec "/usr/libexec/PlistBuddy" ("-c 'Set :CFBundleVersion " + buildNumber + "' " + path)

/// Version/BuildNumber is updated only when it is not empty
let SetAndroidVersion version buildNumber =
    let path = (projectNameAndroid + "/Properties/AndroidManifest.xml")
    let ns = Seq.singleton(("android", "http://schemas.android.com/apk/res/android"))
    XmlPokeNS path ns "manifest/@android:versionName" version
    XmlPokeNS path ns "manifest/@android:versionCode" buildNumber

let AutoincrementTouchBuildNumber () =
    let path = Path.Combine(".", projectNameIOS, "Info.plist")
    Exec "build-touch-autoincrement.sh" path

    // TODO: read current build number from Info.plist
    CommitVersionChange path "** [iOS] Build number auto-incremented"

let AutoincrementAndroidBuildNumber () =
    let path = (projectNameAndroid + "/Properties/AndroidManifest.xml")
    let oldBuildNumber = (XMLRead true path "http://schemas.android.com/apk/res/android" "android" "manifest/@android:versionCode").First()
    let ns = Seq.singleton(("android", "http://schemas.android.com/apk/res/android"))
    let buildNumber = (Int32.Parse(oldBuildNumber) + 1).ToString()
    XmlPokeNS path ns "manifest/@android:versionCode" buildNumber

    CommitVersionChange path ("** [Droid] Build number auto-incremented to " + buildNumber)

let SetTouchVersionFromEnvOrAutoincrement () =
    match (environVarOrNone "BUILD_NUMBER") with
    | Some(buildNumber) -> SetTouchVersion null buildNumber
    | None -> AutoincrementTouchBuildNumber()

let SetAndroidVersionFromEnvOrAutoincrement () =
    match (environVarOrNone "BUILD_NUMBER") with
    | Some(buildNumber) -> SetAndroidVersion null buildNumber
    | None -> AutoincrementAndroidBuildNumber()

let UploadBuild file =
    HockeyApp (fun p ->
      {p with
        ApiToken = environVarOrFail "HockeyAppApiToken"
        File = file
        OwnerId = hockeyAppOwnerId
        DownloadStatus = DownloadStatusOption.Downloadable
        Notes = "** Continuous integration build"
        Notify = NotifyOption.CanInstallApp
      })
    |> fun r -> printfn "-- Build successfully uploaded: %s" r.PublicUrl

let RunUITests projectName =
    //MSBuild "" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ] [ solutionFile ] |> ignore

    RunNUnitTests (projectName + "/bin/Debug/" + projectName + ".dll") (projectName + "/bin/Debug/testresults.xml")

let RunTestCloudTests appFile deviceList =
    let testCloudToken = Environment.GetEnvironmentVariable("TestCloudApiToken")
    let args = String.Format(@"submit ""{0}"" {1} --devices {2} --series ""master"" --locale ""en_US"" --assembly-dir ""<path>/bin/Debug"" --nunit-xml <path>/testresults.xml", appFile, testCloudToken, deviceList)

    Exec "packages/Xamarin.UITest.0.8.0/tools/test-cloud.exe" args

    //TeamCityHelper.sendTeamCityNUnitImport "Apps/tests/apps/testresults.xml"
