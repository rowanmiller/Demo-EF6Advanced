using AdventureWorks.Models;
using Autofac;
using Autofac.Integration.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace AdventureWorks
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // Create container and register dependencies
            var builder = new ContainerBuilder();
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.Register<AdventureWorksContext>((_) => new AdventureWorksContext());
            builder.Register<IDbInterceptor>((_) => new NLogInterceptor());

            builder.Register<Func<IDbExecutionStrategy>>((_) => () => new SqlAzureExecutionStrategy());
            builder.Register<Func<TransactionHandler>>((_) => () => new CommitFailureHandler());

            var container = builder.Build();

            // Wire up MVC to use Autofac to resolve dependencies
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            // Wire up EF to use Autofac to resolve dependencies
            DbConfiguration.Loaded += (s, e) =>
                e.AddDependencyResolver(new AutofacDbDependencyResolver(container), overrideConfigFile: false);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }

    public class NLogInterceptor : IDbCommandInterceptor
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            LogCommandComplete(command, interceptionContext);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            LogCommandComplete(command, interceptionContext);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            LogCommandComplete(command, interceptionContext);
        }

        private void LogCommandComplete<TResult>(DbCommand command, DbCommandInterceptionContext<TResult> interceptionContext)
        {
            if (interceptionContext.Exception == null)
            {
                logger.Trace("Command completed with result {0}", interceptionContext.Result);
                logger.Trace(command.CommandText);
            }
            else
            {
                logger.WarnException("Command failed", interceptionContext.Exception);
                logger.Trace(command.CommandText);
            }
        }
    }

    public class AutofacDbDependencyResolver : IDbDependencyResolver
    {
        private ILifetimeScope container;

        public AutofacDbDependencyResolver(ILifetimeScope container)
        {
            this.container = container;
        }

        public object GetService(Type type, object key)
        {
            if (container.IsRegistered(type))
            {
                return container.Resolve(type);
            }

            return null;
        }

        public IEnumerable<object> GetServices(Type type, object key)
        {
            if (container.IsRegistered(type))
            {
                return new object[] { container.Resolve(type) };
            }

            return Enumerable.Empty<object>();
        }
    }
}
