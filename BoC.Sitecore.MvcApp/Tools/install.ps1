param($installPath, $toolsPath, $package, $project) 

$path = [System.IO.Path]
$mvcsource = $path::Combine($installPath, "MvcApp\**")
$target = $path::GetDirectoryName($project.FileName)
copy $mvcsource -destination $target -recurse

$oldproj = $path::Combine($target, "MvcApp.csproj")
$newproj = $path::Combine($target, $project.Name + ".MvcApp.csproj")
Rename-Item -path $oldproj -newname $newproj

$mvcproject = $project.FileName.Replace(".csproj", ".MvcApp.csproj")
$project.DTE.Solution.AddFromFile($mvcproject, $false)
