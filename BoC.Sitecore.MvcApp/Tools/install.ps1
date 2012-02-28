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

copy $path::Combine($target, "web.config") -destination $path::Combine($target, "web.config.backup")
copy $path::Combine($toolsPath, "transform.proj") -destination $path::Combine($target, "transform.proj")
copy $path::Combine($toolsPath, "web.transform.config") -destination $path::Combine($target, "web.transform.config")
$transform = $path::Combine($target, "transform.proj")
Invoke-Expression "$env:systemroot\Microsoft.Net\Framework\v4.0.30319\MSBuild.exe $transform /t:transform"
