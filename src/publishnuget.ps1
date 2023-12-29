$currentFolder = $MyInvocation.MyCommand.Path
$solutionFolder = $currentFolder.FullName
$rootFolder = (get-item $currentFolder).Directory.Parent.FullName
$nugpkgOutputFolder = [System.IO.Path]::Combine($rootFolder, "nuget")
$nugpkgOutputClearPathExpr = $nugpkgOutputFolder + "\*"
$nugpkgOutputPushExpr = $nugpkgOutputFolder + "\*.nupkg"

Remove-Item -path $nugpkgOutputClearPathExpr -include *.nupkg

Get-ChildItem $solutionFolder -filter ConsoleJobScheduler.Messaging.csproj -recurse | 
        Where-Object { -not ($_.PSIsContainer) } | 
        ForEach-Object {
			dotnet pack $_.FullName --output $nugpkgOutputFolder -c Release
		}

Get-ChildItem $nugpkgOutputFolder -filter *.nupkg -recurse | 
        Where-Object { -not ($_.PSIsContainer) } | 
        ForEach-Object {
            Try {
			    dotnet nuget push $_.FullName --api-key oy2lqvoxuusa6bwgb4maychj77m7ygderjhz2mf5u7biqy --source https://api.nuget.org/v3/index.json 
            }
            Catch {
                Write-Host("Nuget Push Error: " + $_.Exception.Message)
            }
		}
