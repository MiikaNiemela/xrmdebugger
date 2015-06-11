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

namespace MiikaNiemela.Convenience
{
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Client;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class APlugin 
    {
        internal int prevDepth = 0;
        internal Guid token = Guid.Empty;
        internal IPluginExecutionContext getContext(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = null;
            if (serviceProvider == null)
            {
                throw new ArgumentNullException("serviceProvider");
            }
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            return context;
        }
        internal IOrganizationService getService(IServiceProvider serviceProvider, IPluginExecutionContext context)
        {
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = ((IOrganizationServiceFactory)(IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(context.UserId);
            return service;
        }
    }
}
