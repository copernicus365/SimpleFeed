# SimpleFeed

A rich and extremely tolerant .NET RSS and ATOM feed parser. The central objectives are:

1) To get *as much information* as possible from the feeds, including supporting as many of the standard extensions to RSS (including for that matter, the extension of RSS with the ATOM namespace in many cases).

2) To be extremely fault tolerant. Only a minimal number of conditions are needed to have the feed and its items be valid (like each item needs an identifier), and invalid items do not throw exceptions, they just are skipped (though there is error reporting). 

3) To let a single set of types represent both RSS and Atom feeds. To be clear, the point is *not* to loose any specific ATOM or specific RSS information (see #1), but rather to make the SimpleFeed types rich enough in properties and so forth to allow a single C# type to represent what was originally either an ATOM or RSS feed.

4) The focus is on deserializing, though the task of serializing seems the simpler task and might be added in the future.

## Todo

RSS feeds can have an ATOM updated field, handle this:
`<atom:updated xmlns:atom="http://www.w3.org/2005/Atom">2014-10-21T13:27:16.942-07:00</atom:updated>`
