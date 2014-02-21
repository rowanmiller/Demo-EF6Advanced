using AdventureWorks;
using AdventureWorks.Controllers;
using AdventureWorks.Models;
using AdventureWorks.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;

namespace AdventureWorks.Tests.Controllers
{
    [TestClass]
    public class DepartmentsControllerTests
    {
        [TestMethod]
        public void IndexSortedByName()
        {
            // Create some test data that is not ordered alphabetically
            var data = new List<Department>
            {
                new Department { Name = "CCC" },
                new Department { Name = "AAA" },
                new Department { Name = "BBB" }
            }.AsQueryable();

            var set = new Mock<DbSet<Department>>();

            // Wire up LINQ on mock set to use LINQ to Objects against test data
            // Includes wire up to make async work (see http://msdn.com/data/dn314429#async for details)
            set.As<IDbAsyncEnumerable<Department>>()
                .Setup(m => m.GetAsyncEnumerator())
                .Returns(new TestDbAsyncEnumerator<Department>(data.GetEnumerator()));
            set.As<IQueryable<Department>>()
                .Setup(m => m.Provider)
                .Returns(new TestDbAsyncQueryProvider<Department>(data.Provider));
            set.As<IQueryable<Department>>().Setup(m => m.Expression).Returns(data.Expression);
            set.As<IQueryable<Department>>().Setup(m => m.ElementType).Returns(data.ElementType);
            set.As<IQueryable<Department>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            // Create a mock context that returns mock set with test data
            var context = new Mock<AdventureWorksContext>();
            context.Setup(c => c.Departments).Returns(set.Object);

            // Create a controller based on mock context and invoke Index action
            var controller = new DepartmentsController(context.Object);
            var result = controller.Index().Result;

            // Ensure we get a ViewResult back
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;

            // Ensure model is a collection of all Departments ordered by name
            Assert.IsInstanceOfType(viewResult.Model, typeof(IEnumerable<Department>));
            var departments = (IEnumerable<Department>)viewResult.Model;
            Assert.AreEqual(3, departments.Count());
            Assert.AreEqual("AAA", departments.First().Name, "Results not sorted alphabetically");
            Assert.AreEqual("BBB", departments.Skip(1).First().Name, "Results not sorted alphabetically");
            Assert.AreEqual("CCC", departments.Skip(2).First().Name, "Results not sorted alphabetically");
        }
    }
}
