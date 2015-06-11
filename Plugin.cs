/**
 *    
 * 
 *    Copyright (C) 2015  Miika Niemelä
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace MiikaNiemela.XRMDebugger
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.ServiceModel;
    using Microsoft.Xrm.Sdk;
    using MiikaNiemela.Convenience;
    using Microsoft.Xrm.Sdk.Metadata;
    using Microsoft.Xrm.Sdk.Messages;
    using Microsoft.Xrm.Sdk.Query;

    public class DebuggerPlugin : APlugin, IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                IPluginExecutionContext context = getContext(serviceProvider);
                IOrganizationService service = getService(serviceProvider, context);

                //Check that the creation, update, delete etc. of debugline does not get debugged.
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity
                    && ((Entity)context.InputParameters["Target"]).LogicalName == "mp_debugline") return;

                Debugger.debugContext(context, service);

            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException("debugging failure: " + e.Message + e.StackTrace);
            }
        }
    }
}