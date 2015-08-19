#r @"FakeLib.dll"
#load "build-config.fsx"
#load "build-helpers.fsx"
open Fake
open System
open System.IO
open System.Linq
open Fake.XamarinHelper
open BuildConfig
open BuildHelpers

// ----------------
// Core
// ----------------
Target "clean" (fun _ ->
    CleanDirs [ projectNameCore + "/bin"; projectNameCore + "/obj";
                projectNameCoreTest + "/bin"; projectNameCoreTest + "/obj";
                projectNameIOS + "/bin"; projectNameIOS + "/obj";
                projectNameAndroid + "/bin"; projectNameAndroid + "/obj";
                projectNameUITest + "/bin"; projectNameUITest + "/obj"; ]
)

Target "restore-packages" (fun () ->
    RestorePackages solutionFile
    submodules |> List.iter RestorePackages
)

Target "core-build" (fun () ->
    MSBuild "" "Build" [ ("Configuration", "Debug"); ("Platform", "Any CPU") ] [ solutionName + ".sln"; ] |> ignore
)

Target "core-tests" (fun () ->
    let dllPath = String.Format("{0}/{1}.dll", pathCoreTestDebug, projectNameCoreTest)
    RunNUnitTests dllPath (pathCoreTestDebug + "/testresults.xml")
)

// ----------------
// iOS
// ----------------
Target "ios-build" (fun () ->
    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = solutionFile
            Configuration = "Debug|iPhoneSimulator"
            Target = "Build"
        })
)

Target "ios-adhoc" (fun () ->
    SetTouchVersionFromEnvOrAutoincrement()
    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = solutionFile
            Configuration = "Ad-Hoc|iPhone"
            Target = "Build"
        })

    let appPath = Directory.EnumerateFiles(Path.Combine(projectNameIOS, "bin", "iPhone", "Ad-Hoc"), "*.ipa").First()
    UploadBuild appPath
)

Target "ios-appstore" (fun () ->
    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = solutionFile
            Configuration = "AppStore|iPhone"
            Target = "Build"
        })

    let outputFolder = Path.Combine(projectNameIOS, "bin", "iPhone", "AppStore")
    let appPath = Directory.EnumerateDirectories(outputFolder, "*.app").First()
    let zipFilePath = Path.Combine(outputFolder, projectNameIOS + ".zip")
    let zipArgs = String.Format("-r -y '{0}' '{1}'", zipFilePath, appPath)

    Exec "zip" zipArgs

    //TeamCityHelper.PublishArtifact zipFilePath
)

Target "ios-uitests" (fun () ->
    RunUITests projectNameUITest
)

Target "ios-testcloud" (fun () ->
    iOSBuild (fun defaults ->
        {defaults with
            ProjectPath = solutionFile
            Configuration = "Debug|iPhone"
            Target = "Build"
        })

    let appPath = Directory.EnumerateFiles(Path.Combine(projectNameIOS, "bin", "iPhone", "Debug"), "*.ipa").First()

    getBuildParam "devices" |> RunTestCloudTests appPath
)

// ----------------
// Android
// ----------------
Target "android-build" (fun () ->
    SetAndroidVersionFromEnvOrAutoincrement()
    MSBuild "" "Build" [ ("Configuration", "Release"); ("Platform", "Any CPU") ] [ solutionName + ".sln"; ] |> ignore
)

Target "android-package" (fun () ->
    AndroidPackage (fun defaults ->
        {defaults with
            ProjectPath = String.Format("{0}/{0}.csproj", projectNameAndroid)
            //ProjectPath = solutionFile
            Configuration = "Release"
            OutputPath = projectNameAndroid + "/bin/Release"
        })
    |> AndroidSignAndAlign (fun defaults ->
        {defaults with
            KeystorePath = keystorePath
            KeystorePassword = environVarOrFail "KeystorePassword" // TODO: don't store this in the build script for a real app!
            KeystoreAlias = keystoreAlias
        })
    |> fun file -> UploadBuild file.FullName
)

Target "android-uitests" (fun () ->
    AndroidPackage (fun defaults ->
        {defaults with
            ProjectPath = String.Format("{0}/{0}.csproj", projectNameAndroid)
            Configuration = "Release"
            OutputPath = projectNameAndroid + "/bin/Release"
        }) |> ignore

    let appPath = Directory.EnumerateFiles(Path.Combine(projectNameAndroid, "bin", "Release"), "*.apk", SearchOption.AllDirectories).First()

    RunUITests appPath
)

Target "android-testcloud" (fun () ->
    AndroidPackage (fun defaults ->
        {defaults with
            ProjectPath = String.Format("{0}/{0}.csproj", projectNameAndroid)
            Configuration = "Release"
            OutputPath = projectNameAndroid + "/bin/Release"
        }) |> ignore

    let appPath = Directory.EnumerateFiles(Path.Combine(projectNameAndroid, "bin", "Release"), "*.apk", SearchOption.AllDirectories).First()

    getBuildParam "devices" |> RunTestCloudTests appPath
)

"clean"
  ==> "restore-packages"

"restore-packages"
  ==> "core-build"
  ==> "core-tests"

"restore-packages"
  ==> "ios-build"
  ==> "ios-uitests"

"restore-packages"
  ==> "ios-adhoc"

"restore-packages"
  ==> "android-build"
  ==> "android-package"

RunTarget()
