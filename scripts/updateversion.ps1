
if($env:APPVEYOR_REPO_TAG -eq 'True'){
  Update-AppveyorBuild -version $env:APPVEYOR_REPO_BRANCH
}