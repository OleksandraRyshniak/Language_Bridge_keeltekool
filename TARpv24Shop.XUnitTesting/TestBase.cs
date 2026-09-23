using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using Keeltekooli.Controllers;
using Keeltekooli.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TARpv24Keeltekooli.XUnitTesting.Mock;
using TARpv24Keeltekooli.XUnitTesting.Macros;
using Microsoft.EntityFrameworkCore.InMemory;


namespace TARpv24Keeltekooli.XUnitTesting
{
    public abstract class TestBase
    {
        protected IServiceProvider serviceProvider { get; set; }

        protected TestBase()
        {
            var services = new ServiceCollection();
            SetupServices(services);
            serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Seab üles testide läbiviimiseks vajalikud kontrollerid ja teenused mujalt projektist.
        /// See meetod annab ka mälusisese andmebaasi, mida saab testides kasutada.
        /// VIPER-tüüpi projektis toimib see kui "Program.cs" analoog, ent lühendatud kujul.
        /// </summary>
        /// <param name="services">kollektsioon, kuhu asetatakse kontrollerite ja teenuste instantsid</param>

        public virtual void SetupServices(ServiceCollection services)
        {
            services.AddScoped<KoolitusController>();
            services.AddScoped<IHostEnvironment, MockIHostEnvironment>();

            services.AddDbContext<ApplicationUser>
                (x =>
                {
                    x.UseInMemoryDatabase("Test");
                    x.ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
                });
            RegisterMacros(services);
          
        }

        public void Dispose() { }

        /// <summary>
        /// Leia üles kindel teenus, teenusepakkujalt.
        /// serviceprovider omab kontrollerite instantse, ning GetService hangib selle
        /// X tüüpi kontrolleri
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>

        protected T Svc<T>()
        {
            return serviceProvider.GetService<T>();
        }

        /// <summary>
        /// Registreerib macrodes teenusied kui nad ei ole liidesed ja ei ole abstraktsed
        /// On vaja test setupide sedistuseks
        /// Makro --> Teenus
        /// </summary>
        /// <param name="services"> Teenused, siia lisatakse makrodest muid teenuseid.</param>
        private void RegisterMacros(ServiceCollection services)
        {
            var macroBaseType = typeof(IMacros);

            var macros = macroBaseType.Assembly.GetTypes()
                .Where(t => macroBaseType.IsAssignableFrom(t)
                && !t.IsInterface && !t.IsAbstract);
            foreach(var macro in macros)
            {
                services.AddSingleton(macro);
            }
        }
    }
}
