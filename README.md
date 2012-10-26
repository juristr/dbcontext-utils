dbcontext-utils
===============
This repo is intended to contain a collection utilities for the Entity Framework's DbContext.

# Usage
In order to use the DbContextUtils you have to adapt your Entity Framework connection. For this to happen, you have to register the `DbContextConnectionAdapter` as follows:

    public partial class Entities : DbContext
    {
        public Entities()
            : base(new DbContextConnectionAdapter(System.Reflection.Assembly.GetExecutingAssembly()).AdaptConnection("name=Entities"))
        {
            //...
        }
    }

## Schema Translations
Once the `DbContextConnectionAdapter` has been registered you can configure your schema translations as described in [one of my blog posts](http://juristr.com/blog/2012/07/entity-framework-schema-translations/).

# Credits
Large parts of the code have been taken from [Entity Framework Runtime Model Adapter project](http://efmodeladapter.codeplex.com/) on Codeplex.