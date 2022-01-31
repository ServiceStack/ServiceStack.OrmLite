Follow [@ServiceStack](https://twitter.com/servicestack), [view the docs](https://docs.servicestack.net), use [StackOverflow](https://stackoverflow.com/questions/ask?tags=servicestack,ormlite-servicestack) or [Customer Forums](https://forums.servicestack.net/) for support.

# Read ServiceStack.OrmLite Docs at [docs.servicestack.net/ormlite](https://docs.servicestack.net/ormlite/)

## Community Resources

  - [OrmLite and Redis: New alternatives for handling db communication](http://www.abtosoftware.com/blog/servicestack-ormlite-and-redis-new-alternatives-for-handling-db-communication) by [@abtosoftware](https://twitter.com/abtosoftware)
  - [Object Serialization as Step Towards Normalization](http://www.unpluggeddevelopment.com/post/85225892120/object-serialization-as-step-towards-normalization) by [@ 82unpluggd](https://twitter.com/82unpluggd)
  - [Creating a Data Access Layer using OrmLite](http://blogs.askcts.com/2014/05/07/getting-started-with-servicestack-part-2/) by [Lydon Bergin](http://blogs.askcts.com/)
  - [Code Generation using ServiceStack.OrmLite and T4 Text templates](http://jokecamp.wordpress.com/2013/09/07/code-generation-using-servicestack-ormlite-and-t4-text-templates/) by [@jokecamp](https://twitter.com/jokecamp)
  - [Simple ServiceStack OrmLite Example](http://www.curlette.com/?p=1068) by [@robrtc](https://twitter.com/robrtc)
  - [OrmLite Blobbing done with NHibernate and Serialized JSON](http://www.philliphaydon.com/2012/03/ormlite-blobbing-done-with-nhibernate-and-serialized-json/) by [@philliphaydon](https://twitter.com/philliphaydon)
  - [Creating An ASP.NET MVC Blog With ServiceStack.OrmLite](http://www.eggheadcafe.com/tutorials/asp-net/285cbe96-9922-406a-b193-3a0b40e31c40/creating-an-aspnet-mvc-blog-with-servicestackormlite.aspx) by [@peterbromberg](https://twitter.com/peterbromberg)

## Other notable Micro ORMs for .NET
Many performance problems can be mitigated and a lot of use-cases can be simplified without the use of a heavyweight ORM, and their config, mappings and infrastructure. 
As [performance is the most important feature](https://github.com/mythz/ScalingDotNET) we can recommend the following list, each with their own unique special blend of features. 

  * **[Dapper](http://code.google.com/p/dapper-dot-net/)** - by [@samsaffron](http://twitter.com/samsaffron) and [@marcgravell](http://twitter.com/marcgravell) 
    - The current performance king, supports both POCO and dynamic access, fits in a single class. Put in production to solve [StackOverflow's DB Perf issues](http://samsaffron.com/archive/2011/03/30/How+I+learned+to+stop+worrying+and+write+my+own+ORM). Requires .NET 4.
  * **[PetaPoco](http://www.toptensoftware.com/petapoco/)** - by [@toptensoftware](http://twitter.com/toptensoftware)
    - Fast, supports dynamics, expandos and typed POCOs, fits in a single class, runs on .NET 3.5 and Mono. Includes optional T4 templates for POCO table generation.
  * **[Massive](https://github.com/robconery/massive)** - by [@robconery](http://twitter.com/robconery)
    - Fast, supports dynamics and expandos, smart use of optional params to provide a wrist-friendly api, fits in a single class. Multiple RDBMS support. Requires .NET 4.
  * **[Simple.Data](https://github.com/markrendle/Simple.Data)** - by [@markrendle](http://twitter.com/markrendle)
    - A little slower than above ORMS, most wrist-friendly courtesy of a dynamic API, multiple RDBMS support inc. Mongo DB. Requires .NET 4.
