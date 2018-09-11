# SimpleFeed

A rich and extremely tolerant .NET RSS and ATOM feed parser. The central objectives are:

1) To get *as much information* as possible from the feeds, including supporting as many of the standard extensions to RSS (including for that matter, the extension of RSS with the ATOM namespace in many cases).

2) To be extremely fault tolerant. Only a minimal number of conditions are needed to have the feed and its items be valid (like each item needs an identifier), and invalid items do not throw exceptions, they just are skipped (though there is error reporting). 

3) To let a single set of types represent both RSS and Atom feeds. To be clear, the point is *not* to loose any specific ATOM or specific RSS information (see #1), but rather to make the SimpleFeed types rich enough in properties and so forth to allow a single C# type to represent what was originally either an ATOM or RSS feed.

4) The focus is on deserializing, though the task of serializing seems the simpler task and might be added in the future.

## SimpleFeed.NetFX

The main project is built against netstandard 2.0, but for some of us where there have been problems when our own projects reference a netstandard project, at least for now, this extra `SimpleFeed.NetFX` project has been made, built against NET 4.7.1, and with 0 (zero) source files except for the basic project files, as main source are links to the main project.

## DotNetXtensions dependency

There is extensive usage of DotNetXtensions in this project, so for now there is a dependency on DotNetXtensions and DotNetXtensions.Common (which contains `BasicMimeTypes` and related). We have gone back and forth over the years between directly referencing DNX project, while at other times keeping a local copy to the project of needed private helper code. But since `BasicMimeTypes` is something we want to expose publicly, and to not limit that type to only `SimpleFeed`, this seems the best thing to do.

