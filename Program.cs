using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Polyrific.Catapult.TaskProviders.Core;

namespace Polyrific.Catapult.TaskProviders.GreetingConsole
{
    class Program : CodeGeneratorProvider
    {
      
        public Program(string[] args) :base(args){

        }

        public override string Name => "Polyrific.Catapult.TaskProviders.GreetingConsole";

        static void Main(string[] args)
        {
            var app = new Program(args);
            var result = app.Execute().Result;
            Console.Write(result);
        }

        public override async System.Threading.Tasks.Task<(string outputLocation, Dictionary<string, string> outputValues, string errorMessage)> Generate()
        {
                        // set the default title to project name
            string projectTitle = string.Empty; 
            if (AdditionalConfigs != null && AdditionalConfigs.ContainsKey("Title") && !string.IsNullOrEmpty(AdditionalConfigs["Title"]))
                projectTitle = AdditionalConfigs["Title"];

            // set the output location
            Config.OutputLocation = Config.OutputLocation ?? Config.WorkingLocation;

            // TODO: call the code generator logic
            await GenerateCode(ProjectName, Config.OutputLocation);


            if(File.Exists(Path.Combine(Config.OutputLocation,ProjectName,$"Startup.cs"))){
                
                var content = await LoadFile(Path.Combine(Config.OutputLocation,ProjectName,$"Startup.cs"));
                content = content.Replace("Hello World!", "Hello World From Catapult Task Provider!");
                await File.WriteAllTextAsync(Path.Combine(Config.OutputLocation,ProjectName,$"Startup.cs"), content);
            }

            return (Config.OutputLocation, null, "");     
        }

        private Task GenerateCode(string projectName, string outputLocation)
        {
            // if this code is run in linux/mac, change the "powershell" into "pwsh" and the arguments should be $"-c \"dotnet new mvc --name {projectName}\""
            var info = new ProcessStartInfo("powershell")
            {
                UseShellExecute = false,
                Arguments = $"dotnet new web --name {projectName}",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = outputLocation
            };

            using (var process = Process.Start(info))
            {
                process.WaitForExit();
            }

            return Task.CompletedTask;
        }

        private async Task<string> LoadFile(string filePath)
        {
            var content = await File.ReadAllTextAsync(filePath);

            content = content.Replace("// @ts-ignore", "");

            return content;
        }
    }
}
