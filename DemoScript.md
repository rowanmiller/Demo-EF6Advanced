Setup
Reset code base
	git reset --hard StartingPoint
	git clean -fdx
Reset database
	Ensure __MigrationsHistory and __Transactions tables are removed from database
	Ensure Rating column, default constraint and index are removed from HumanResources.Department table
Setup solution
	Open solution as Admin
	Run Get-ExecutionPolicy in Package Manager Console and ensure it returns RemoteSigned
	Restore NuGet packages
	Ensure only Local NuGet feed is enabled
	Ensure AdventureWorksContext  is in clipboard
	Set Departments as startup page
	Ensure snippets are loaded

Tooling Consolidation (OneEF)
Show existing AdventureWorks2012 database

Right-click on Models folder -> Add -> New Item -> Data -> ADO.NET Entity Data Model
	Use AdventureWorks as the name
	Ensure connection string name is AdventureWorksContext (affects the context name)
	Complete Code First to Existing Database wizard to AdventureWorks2012 database
		
Walk through the generated code
	Key points:
		? Code is close to what we expect folks would write by hand
		? Config only specified where needed
		? Annotations used where possible
		? Connection string always in config file (name=xyz syntax in constructor)
		
Right-click on Controllers folder -> Add -> Controller
	Select MVC 5 Controller with views, using Entity Framework
	Controller name: DepartmentsController
	Model class: Department
	Data context class: AdventureWorksContext
	
Run app, navigate to /departments to show everything is working
Tip: Use Ctrl+F5 to run without the debugger (the debugger adds quite a bit of warm-up time to ASP.NET)

Dependency Resolution
Add a constructor to Departments controller - Code snippet: ANewConstructor
Remove initialization of db field

Install Autofac.MVC5 NuGet package
Setup Autofac in Global.asax Application_Start - Code snippet: SetupAutofac

Add an EF dependency resolver that uses Autofac - Code snippet: DefineAnEfResolverThatResolvesFromAutofac
Wire up EF resolver in Application_Start
	DbConfiguration.Loaded += (s, e) => 
	    e.AddDependencyResolver(new AutofacDbDependencyResolver(container), overrideConfigFile: false);
	
Install NLog.Config NuGet package
Open NLog.config and uncomment the default settings (highlighted below)
Tip: Highlight text and use Ctrl+K, Ctrl+U
	  <targets>
	    <!-- add your targets here -->
	    
	    <!--
	    <target xsi:type="File" name="f" fileName="${basedir}/logs/${shortdate}.log"
	            layout="${longdate} ${uppercase:${level}} ${message}" />
	    -->
	  </targets>
	
	  <rules>
	    <!-- add your logging rules here -->
	    
	    <!--
	    <logger name="*" minlevel="Trace" writeTo="f" />
	    -->
	  </rules>

Define an EF=>NLog interceptor - Code snippet: DefineAnEfInterceptorThatLogsToNLog
Register interceptor with Autofac
	builder.Register<IDbInterceptor>((_) => new NLogInterceptor());

Run app and hit /departments and navigate around
Open directory of MVC project and show file from \logs\ directory

Configuring for Azure
Register Execution strategy and transaction handler - Code snippet: ConfigureDependenciesForSqlAzure
Run, edit a Department, and show __Transactions table

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



