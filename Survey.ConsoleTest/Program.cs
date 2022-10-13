using Microsoft.Extensions.Configuration;
using Survey.BL.EFCoreModels;
using Survey.BL.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

//https://asp.net-hacker.rocks/2017/02/08/using-dependency-injection-in-dotnet-core-console-apps.html
//https://siderite.dev/blog/creating-console-app-with-dependency-injection-in-/
//https://www.learnentityframeworkcore5.com/connection-strings-entity-framework-core

namespace Survey.ConsoleTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var newbuilder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json");

            IConfiguration iconfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var serviceCol = new ServiceCollection();
                        
            serviceCol.AddDbContext<SurveyDbContext>(options =>
            options.UseSqlServer(
                iconfig.GetConnectionString("SurveyDbConnStr")));

            var provider = serviceCol.BuildServiceProvider();

            //string fullFilePath = @"C:\DevelopmentArea\SurveyManagment\Survey_New.xlsx";
            string fullFilePath = @"C:\DevelopmentArea\SurveyManagment\Survey_New - Update.xlsx";

            

            using (var ctx = provider.GetService<SurveyDbContext>())
            {
                //using (var fstream = File.OpenRead(fullFilePath))
                //{
                //    SurveyManager manager = new SurveyManager(ctx);

                //    manager.LoadSurvey(fstream);
                //}

                var test = ctx.Surveys.ToList();
                //SurveyManager manager = new SurveyManager(ctx);

                //using (var ms = manager.DownloadSurvey(3))
                //{
                //    using (var fs = new FileStream(String.Format(@"C:\DevelopmentArea\SurveyManagment\Download.xlsx"), FileMode.Create))
                //    {
                //        ms.WriteTo(fs);
                //    }

                //}

            }
        }

        private static void Init()
        {
            var newbuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            IConfiguration iconfig = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            
        }
    }
}