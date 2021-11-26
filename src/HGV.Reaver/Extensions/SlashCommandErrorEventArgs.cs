using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace HGV.Reaver
{
    public static class ErrorEventArgsExtensions
    {
        public static void LogError(this SlashCommandErrorEventArgs e)
        {
            try
            {
                var username = e?.Context?.User?.Username ?? "<unknown user>";
                var cmd = e?.Context?.CommandName ?? "<unknown command>";
                var msg = e.Exception.Message ?? "<no message>";
                var type = e.Exception.GetType();
                e.Context.Client.Logger.LogError($"{username} tried executing '{cmd}' but it errored: {type} ({msg})");
            }
            catch (Exception ex)
            {
                throw new Exception("COULD NOT LOG THE ERROR.... SOMTHING IS VERY WORNG!", ex);
            }
        }

        public static void LogError(this ContextMenuErrorEventArgs e)
        {
            try
            {
                var username = e?.Context?.User?.Username ?? "<unknown user>";
                var cmd = e?.Context?.CommandName ?? "<unknown command>";
                var msg = e.Exception.Message ?? "<no message>";
                var type = e.Exception.GetType();
                e.Context.Client.Logger.LogError($"{username} tried executing '{cmd}' but it errored: {type} ({msg})");
            }
            catch (Exception ex)
            {
                throw new Exception("COULD NOT LOG THE ERROR.... SOMTHING IS VERY WORNG!", ex);
            }
        }
    }
}
