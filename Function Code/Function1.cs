using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Configuration;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using Microsoft.WindowsAzure.Storage;

namespace StartStopVM2
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 0 6 * * 1-5")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        { 
            VMBackground createVM = new VMBackground();

            IAzure azure = GetCredentials(context);

            var groupName = "GROUPNAME";    //Enter Resource Group Name
            var vmName = "VMNAME";          //Enter Virtual Machine Name

            log.LogInformation(groupName + "   " + vmName);
            try
            {
                if (azure.VirtualMachines.GetByResourceGroup(groupName, vmName) == null)
                {
                    createVM.CreateTheVMBackground(context, log);
                }

                //Instance of VM
                var vm = azure.VirtualMachines.GetByResourceGroup(groupName, vmName);
                vm.Start();
                log.LogInformation("VM has started");
            }
            catch(ArgumentNullException e)
            {
                log.LogError("ERROR 407 - Null Value has been entered for either Resource Group Name or VM Name \n" +
                    e.ToString());
            }
        }
        public static IAzure GetCredentials(ExecutionContext context)
        {
                string credFilePath = Path.Combine(context.FunctionAppDirectory, "azureauth.properties");
                var credentials = SdkContext.AzureCredentialsFactory
                    .FromFile(credFilePath);

                return Azure
                        .Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials)
                        .WithDefaultSubscription();
        }
    }
}
