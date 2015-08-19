module BuildConfig

// Required env properties:
//  HockeyAppApiToken <token>
//  KeystorePassword <password>

let solutionName = "MyApp";
let projectNameCore = solutionName + ".Core"
let projectNameCoreTest = solutionName + ".Core.Tests"
let projectNameIOS = solutionName + ".Touch"
let projectNameAndroid = solutionName + ".Droid"
let projectNameUITest = solutionName + ".UITest"

let pathCoreDebug = projectNameCore + "/bin/Debug"
let pathCoreTestDebug = projectNameCoreTest + "/bin/Debug"
let pathIosDebug = projectNameIOS + "/bin/Debug"
let pathDroidDebug = projectNameAndroid + "/bin/Debug"

let solutionFile = solutionName + ".sln";

let keystorePath = "path-to-keystore.keystore"
let keystoreAlias = "keystore-alias"

let hockeyAppOwnerId = ""

let submodules = []
