mkdir .\packages
..\.nuget\nuget.exe pack ..\nuspecs\Snork.FluentNHibernateTools.nuspec -outputdirectory .\packages
..\.nuget\nuget.exe pack ..\nuspecs\Log4Net.FluentNHibernateAppender.nuspec -outputdirectory .\packages
pause