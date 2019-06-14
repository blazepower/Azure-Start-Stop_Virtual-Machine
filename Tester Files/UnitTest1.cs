using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Compute.Fluent;

namespace StopStartVMTests
{
    public class UnitTest1
    {
        [Fact]
        public void GetCredentials_should_return_nonnull_value()
        {
            ExecutionContext context = new ExecutionContext();
            var cred = Background.GetCredentials(context);
            Assert.NotNull(cred);
        }

        [Fact]
        public void testVMCreation()
        {
            ExecutionContext context = new ExecutionContext();
            var azure = Background.GetCredentials(context);
            var groupName = "GROUPNAME";
            var vmName = "VMNAME";
            var location = Region.USWest;
            
            ILogger logger = Background.CreateLogger();
            var myVM = Background.CreateTheVMBackground(context, logger);
            var testVM = azure.VirtualMachines.GetByResourceGroup(groupName, vmName);
            Assert.NotNull(myVM);
            Assert.Equal(testVM, myVM);
        }

        [Fact]
        public void testResourceGroupCreation()
        {
            ExecutionContext context = new ExecutionContext();
            ILogger logger = Background.CreateLogger();

            var rg = Background.createResourceGroup(context, logger);
            Assert.NotNull(rg);
        }

        [Fact]
        public void testIPCreation()
        {
            ExecutionContext context = new ExecutionContext();
            ILogger logger = Background.CreateLogger();

            var ip = Background.createPublicIP(context, logger);
            Assert.NotNull(ip);
        }

        [Fact]
        public void testNetworkCreation()
        {
            ExecutionContext context = new ExecutionContext();
            ILogger logger = Background.CreateLogger();

            var net = Background.createNetworks(context, logger);
            Assert.NotNull(net);
        }
        [Fact]
        public void testNetInterfaceCreation()
        {
            ExecutionContext context = new ExecutionContext();
            ILogger logger = Background.CreateLogger();

            var networkInterface = Background.createNetworkInterface(context, logger);
            Assert.NotNull(networkInterface);
        }
        [Fact]
        public void testStart()
        {
            ExecutionContext context = new ExecutionContext();
            ILogger logger = Background.CreateLogger();
            var vm = Background.CreateTheVMBackground(context, logger);

            if (System.DateTime.Now.Hour > 6)
            {
                Assert.True(vm.PowerState == PowerState.Running || vm.PowerState == PowerState.Starting && vm.PowerState != PowerState.Stopping);
            }
        }
        [Fact]
        public void testStop()
        {
            ExecutionContext context = new ExecutionContext();
            ILogger logger = Background.CreateLogger();
            var vm = Background.CreateTheVMBackground(context, logger);

            if (System.DateTime.Now.Hour > 21)
            {
                Assert.True(vm.PowerState == PowerState.Stopped || vm.PowerState == PowerState.Stopping && vm.PowerState != PowerState.Starting);
            }
        }
    }
}
