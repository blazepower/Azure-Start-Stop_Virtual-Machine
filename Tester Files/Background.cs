using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.WebJobs;
using System.IO;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using System;
using Microsoft.Azure.Management.Network.Fluent;

namespace StopStartVMTests
{
    class Background
    {
        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;

            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
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
        public static IVirtualMachine CreateTheVMBackground(ExecutionContext context, ILogger logger)
        {
            var azure = GetCredentials(context);
            var groupName = "GROUPNAME";
            var vmName = "VMNAME";
            var location = Region.USWest;

            //Create Resource Group
            var resourceGroup = createResourceGroup(context, logger);

            //Get public IP address
            var publicIPAddress = createPublicIP(context, logger);

            //Create subnet and Virtual Network
            var network = createNetworks(context, logger);

            //Create Network Interface
            var networkInterface = createNetworkInterface(context, logger);

            //Create the VM
            int retryTimeVM = 2000;
        VMCreation:
            var VM = azure.VirtualMachines.Define(vmName)
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetworkInterface(networkInterface)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2012-R2-Datacenter")
                .WithAdminUsername("User")
                .WithAdminPassword("Password12345678")
                .WithComputerName(vmName)
                .WithSize(VirtualMachineSizeTypes.StandardDS1)
                .Create();
            if (VM != null)
                logger.LogInformation("Virtual Machine Created");
            else
            {
                retryTimeVM *= 2;
                if (retryTimeVM > 60000)
                    throw new Exception("Virtual Machine failed");
                logger.LogError("Virtual Machine unable to be created \n retrying....");
                System.Threading.Thread.Sleep(retryTimeVM);
                goto VMCreation;
            }
            return VM;
        }
        public static IResourceGroup createResourceGroup(ExecutionContext context, ILogger logger)
        {
            var azure = GetCredentials(context);
            var groupName = "GROUPNAME";
            var location = Region.USWest;


            int retryTimeRG = 2000;
        ResourceGroupCreation:
            var resourceGroup = azure.ResourceGroups.Define(groupName)
                .WithRegion(location)
                .Create();

            if (resourceGroup != null)
                logger.LogInformation("Resource Group Created");
            else
            {
                retryTimeRG *= 2;
                if (retryTimeRG > 60000)
                    throw new Exception("Resource Group creation failed");
                logger.LogError("Resource Group unable to be created \n retrying....");
                System.Threading.Thread.Sleep(retryTimeRG);
                goto ResourceGroupCreation;
            }
            return resourceGroup;
        }

        public static IPublicIPAddress createPublicIP(ExecutionContext context, ILogger logger)
        {
            var azure = GetCredentials(context);
            var groupName = "GROUPNAME";
            var location = Region.USWest;


            int retryTimeIP = 2000;
        publicIPCreation:
            var publicIPAddress = azure.PublicIPAddresses.Define("PublicIP")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithDynamicIP()
                .Create();
            if (publicIPAddress != null)
                logger.LogInformation("IP Created");
            else
            {
                retryTimeIP *= 2;
                if (retryTimeIP > 60000)
                    throw new Exception("Public IP creation failed");
                logger.LogError("Public IP unable to be created \n retrying....");
                System.Threading.Thread.Sleep(retryTimeIP);
                goto publicIPCreation;
            }
            return publicIPAddress;
        }
        public static INetwork createNetworks(ExecutionContext context, ILogger logger)
        {
            var azure = GetCredentials(context);
            var groupName = "GROUPNAME";
            var location = Region.USWest;

            int retryTimeVNet = 2000;
        VNetCreation:
            var network = azure.Networks.Define("VNet")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithAddressSpace("10.0.0.0/16")
                .WithSubnet("Subnet", "10.0.0.0/24")
                .Create();
            if (network != null)
                logger.LogInformation("subnet and Virtual Network Created");
            else
            {
                retryTimeVNet *= 2;
                if (retryTimeVNet > 60000)
                    throw new Exception("subnet and Virtual Network creation failed");
                logger.LogError("subnet and Virtual Network unable to be created \n retrying....");
                System.Threading.Thread.Sleep(retryTimeVNet);
                goto VNetCreation;
            }
            return network;
        }
        public static INetworkInterface createNetworkInterface(ExecutionContext context, ILogger logger)
        {
            var azure = GetCredentials(context);
            var groupName = "GROUPNAME";
            var location = Region.USWest;
            var network = createNetworks(context, logger);
            var publicIPAddress = createPublicIP(context, logger);

            int retryTimeNetworkInterface = 2000;
        NetworkInterfaceCreation:
            var networkInterface = azure.NetworkInterfaces.Define("NetworkInterface")
                .WithRegion(location)
                .WithExistingResourceGroup(groupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet("Subnet")
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(publicIPAddress)
                .Create();
            if (networkInterface != null)
                logger.LogInformation("Network Interface created");
            else
            {
                retryTimeNetworkInterface *= 2;
                if (retryTimeNetworkInterface > 60000)
                    throw new Exception("Network Interface creation failed");
                logger.LogError("Network Interface unable to be created \n retrying....");
                System.Threading.Thread.Sleep(retryTimeNetworkInterface);
                goto NetworkInterfaceCreation;
            }
            return networkInterface;
        }
    }
}
