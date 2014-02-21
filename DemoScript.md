The demo is broken up into a series of sections that showcase a specific features. Each section has a corresponding slide in the slide deck, which you can use to provide a quick summary of the feature before demoing it. This also helps break up the long demo section of the talk into logical 5-10min chunks, which helps folks keep up to speed.

### One Time Setup
* You'll need a machine with Visual Studio 2013 installed
* Install the EF6.1 (or later) Tooling. At the time of writing this demo the tooling was available in Beta form and could be [downloaded from the Microsoft Download Center](http://www.microsoft.com/en-us/download/details.aspx?id=41928).
* Install [ZoomIt](http://technet.microsoft.com/en-us/sysinternals/bb897434.aspx) and get familiar with how to use it - it's a great tool for making you demos look professional.
* Clone this Git repo to your local machine
* Register the code snippets in Visual Studio
 * Open VS -> Tools -> Code Snippets Manager...
 * Make sure **Visual C#** is selected in the dropdown and add the **CodeSnippets** directory from your local clone of this repository
* To insulate yourself against network outages, slow WiFi, etc. I suggest you [setup a local NuGet feed on your machine](http://docs.nuget.org/docs/creating-packages/hosting-your-own-nuget-feeds) that contains all the NuGet packages needed to complete the talk to your local feed.
 * I usually disable everything but my local feed - nothing worse than getting long pauses due to a slow network. 
 * The easiest way to get a copy of the required .nupkg files is to complete this demo once whilst online then go to the packages folder of the completed solution and you'll find the .nupkg's nested under the sub-directory for each package. 
  * You'll notice there are a lot more packages than just the ones you install during the demo. The extras are the ones that are included in the ASP.NET project template - you will need all of them to be able to restore packages when you reset the code base to the starting point. Be sure to copy them all so that you can sucessfully perform the 'Every Time Setup' without a network connection.

### Every Time Setup
* Reset the code base to the starting solution
 * How you do this will depend on what Git tools you have installed. Here is how to do it using the Git command line:
  * Open a console to the local repo directoy and run the following commands 
  * `git reset --hard StartingPoint`
  * `git clean -fdx`
* Reset the **AdventureWorks2012** database using the **ResetDatabase.sql** file in the repo.
* Run Visual Studio as an administrator (running as an administrator seems to minimize occurrences of the issue mentioned in the next point)
* Run **Get-ExecutionPolicy** in Package Manager Console (PMC) and ensure it returns **RemoteSigned**
 * Occasionally PMC sets the **Restricted** execution policy and won't allow running install scripts from the NuGet packages. It's really hard to recover from, **don't skip this step!**
* Connect to **(localdb)\v11.0** in SQL Server Object Explorer and open the AdventureWorks2012 database (if you have a SKU of Visual Studio which includes SQL Server Object Explorer). If you have a version of VS without it, you should grab [SQL Server Management Studio](http://www.microsoft.com/en-us/download/details.aspx?id=29062) instead.
  * I recommend dropping all databases except AdventureWorks2012 from LocalDb before the demo - it's just less noise for folks to process.
* Open the **SourceCode\AdventureWorks.sln** and build the solution. It's good to run it too, just to make sure everything is working.

### Tooling Consolidation
* Show existing AdventureWorks2012 database
* Right-click on Models folder -> Add -> New Item -> Data -> ADO.NET Entity Data Model...
 * Enter **AdventureWorks** as the name in the **Add New Item** screen (i.e. before launching the wizard)
 * Complete **Code First to Existing Database** wizard using the **AdventureWorks2012** database
  * On the select objects screen select all tables and then uncheck the tables in the **dbo** schema
* Walk through the generated code, the key points are:
 * Code is close to what we expect folks would write by hand
 * Config only specified where needed
 * Annotations used where possible
 * Connection string always in config file

Quickly scaffold a controller to show the model in action		
* Right-click on Controllers folder -> Add -> Controller
* Select MVC 5 Controller with views, using Entity Framework
 * **Controller name:** DepartmentsController
 * **Model class:** Department
 * **Data context class:** AdventureWorksContext
* Run app, navigate to /departments to show everything is working (there is a Departments link in the site header)
  * **Tip:** Use **Ctrl+F5** to run without the debugger (the debugger adds quite a bit of warm-up time to ASP.NET)

### Dependency Resolution
* Add a constructor to Departments controller | **Code snippet: ANewConstructor**
 * Remove initialization of db field (i.e. you should be left with `private AdventureWorksContext db;`)

Setup MVC and EF to get dependencies from AutoFac
* Install **Autofac.MVC5** NuGet package
* Setup Autofac in Global.asax at the top of the Application_Start method | **Code snippet: SetupAutofac**
* Add an EF dependency resolver that uses Autofac (I just do this below the **MvcApplication** in **Global.asax**) | **Code snippet: DefineAnEfResolverThatResolvesFromAutofac**
* Wire up EF resolver in Application\_Start (there is a TODO comment in Application_Start showing where to do this).
```
DbConfiguration.Loaded += (s, e) => 
    e.AddDependencyResolver(new AutofacDbDependencyResolver(container), overrideConfigFile: false);
```

Register a logger dependency that EF will pull from AutoFac	
* Install **NLog.Config** NuGet package
* Open the **NLog.config** file that was added to your project and uncomment the default settings
  * Tip: You can highlight the lines and type **Ctrl+k Ctrl+u**
```
<target xsi:type="File" name="f" fileName="${basedir}/logs/${shortdate}.log"
    layout="${longdate} ${uppercase:${level}} ${message}" />
```
```
<logger name="*" minlevel="Trace" writeTo="f" />
```
* Define an EF=>NLog interceptor (I just do this below the **MvcApplication** in **Global.asax**) | **Code snippet: DefineAnEfInterceptorThatLogsToNLog**
* Register interceptor with Autofac (below the other calls to **Register** at the top of Application_Start)
```
builder.Register<IDbInterceptor>((_) => new NLogInterceptor());
```
* Run app and hit /departments and navigate around
* Open directory of MVC project and show the log file from the **\logs** directory

### Configuring for Azure
* Register Execution strategy and transaction handler (below the other calls to **Register** at the top of Application_Start | **Code snippet: ConfigureDependenciesForSqlAzure**
* Run the app, edit a Department, and show the **__Transactions** table that was created in the database

Unit Testing
In test project
	Install 'Moq'  NuGet package

Implement the test explaining each section
// Create some test data that is not ordered alphabetically	Code Snippet: CreateSomeUsefulTestData
var data = new List<Blog>
{
    new Blog { Name = "CCC" },
    new Blog { Name = "AAA" },
    new Blog { Name = "BBB" }
}.AsQueryable();

var set = new Mock<DbSet<Blog>>();	Write by hand
// Wire up LINQ on fake set to use LINQ to Objects against test data	Code Snippet: MakeThatMagicLinqStuffWork
set.As<IQueryable<Blog>>().Setup(m => m.Provider).Returns(data.Provider);
set.As<IQueryable<Blog>>().Setup(m => m.Expression).Returns(data.Expression);
set.As<IQueryable<Blog>>().Setup(m => m.ElementType).Returns(data.ElementType);
set.As<IQueryable<Blog>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

// Create a mock context that returns mock set with test data	Code Snippet: SetupAMockContext
var context = new Mock<BloggingContext>();	Talk about why we don't use Autofac here
context.Setup(c => c.Blogs).Returns(set.Object);

// Create a controller based on fake context and invoke Index action	Code Snippet: RunTheTest
var controller = new BlogsController(context.Object);
var result = controller.Index();

// Ensure we get a ViewResult back	Code Snippet: CheckIfEverythingWorkedAsExpected
Assert.IsInstanceOfType(result, typeof(ViewResult));
var viewResult = (ViewResult)result;

// Ensure model is a collection of all Blogs ordered by name
Assert.IsInstanceOfType(viewResult.Model, typeof(IEnumerable<Blog>));
var listings = (IEnumerable<Blog>)viewResult.Model;
Assert.AreEqual(3, listings.Count());
Assert.AreEqual("AAA", listings.First().Name, "Blogs not sorted alphabetically");
Assert.AreEqual("BBB", listings.Skip(1).First().Name, "Blogs not sorted alphabetically");
Assert.AreEqual("CCC", listings.Skip(2).First().Name, "Blogs not sorted alphabetically");

Run the test (you'll need to open to Test Explorer) - it will fail the verification
	Add sorting into the Index action (return View(db.Departments.OrderBy(b => b.Name).ToList());)
	Re-run the test and it will pass

Async
Make the Index action on BlogsController async
	public async Task<ActionResult> Index()
	{
	    return View(await db.Blogs.OrderBy(b => b.Name).ToListAsync());
	}
Run app and show that everything just works (no need to change consuming code)

Migrations with an existing database
Run Enable-Migrations in Package Manager Console
From error message copy paste command to enable for BloggingContext
Run Add-Migration Initial -IgnoreChanges
Run Update-Database

Add a Code property to Department 

Run Add-Migration Code
Run Update-Database
Show updated schema

[Index] Attribute
Upgrade to 6.1.0-alpha1
Add [Index] to Department.Code
Add-Migration CodeIndex
Update-Database



