using System;
using System.Management.Automation;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Docker.PowerShell.Support;

namespace Docker.PowerShell.Cmdlets
{
    [Cmdlet(VerbsCommon.Enter, "Container",
            DefaultParameterSetName = CommonParameterSetNames.Default)]
    public class EnterContainer : SingleContainerOperationCmdlet
    {
        protected override async Task ProcessRecordAsync()
        {
            var id = Id ?? Container.ID;

            var inspect = await DkrClient.Containers.InspectContainerAsync(id);
            if (!inspect.State.Running || inspect.State.Paused)
            {
                throw new Exception("Cannot enter a stopped or paused container.");
            }

            var parameters = new ContainerAttachParameters
            {
                Stdin = inspect.Config.OpenStdin,
                Stdout = true,
                Stream = true
            };

            using (var stream = await DkrClient.Containers.AttachContainerAsync(inspect.ID, inspect.Config.Tty, parameters, CmdletCancellationToken))
            {
                await stream.CopyToConsoleAsync(inspect.Config, CmdletCancellationToken);
            }
        }
    }
}