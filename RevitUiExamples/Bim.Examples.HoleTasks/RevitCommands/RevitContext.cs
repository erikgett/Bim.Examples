using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.RevitAddIns;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReSaveFamily.Core
{
    public class RevitContext : IDisposable
    {
        static readonly string[] Searchs = RevitProductUtility.GetAllInstalledRevitProducts().Select(x => x.InstallLocation).ToArray();
        static readonly object lockobj = new object();
        static RevitContext _instance;
        private Product _product;

        public Application App { get => _product.Application; }

        public static RevitContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (lockobj)
                    {
                        if (_instance == null)
                        {
                            _instance = new RevitContext();
                        }
                    }
                }
                return _instance;
            }
        }

        static RevitContext()
        {
            AddEnvironmentPaths(Searchs);

            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
        }

        public void Run()
        {
            _product = Product.GetInstalledProduct();

            var clientId = new ClientApplicationId(Guid.NewGuid(), "DotNet", "BIMAPI");
            _product.Init(clientId, "I am authorized by Autodesk to use this UI-less functionality.");

        }

        public void Stop()
        {
            _product?.Exit();
        }

        static void AddEnvironmentPaths(params string[] paths)
        {
            var path = new[] { Environment.GetEnvironmentVariable("PATH") ?? string.Empty };

            var newPath = string.Join(System.IO.Path.PathSeparator.ToString(), path.Concat(paths));

            Environment.SetEnvironmentVariable("PATH", newPath);
        }

        public static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name);

            foreach (var item in Searchs)
            {
                var file = string.Format("{0}.dll", System.IO.Path.Combine(item, assemblyName.Name));

                if (File.Exists(file))
                {
                    return Assembly.LoadFile(file);
                }
            }

            return args.RequestingAssembly;
        }

        public void Dispose()
        {
            _product?.Exit();
        }
    }
}
