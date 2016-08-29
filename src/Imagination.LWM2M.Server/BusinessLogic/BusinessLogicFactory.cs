/***********************************************************************************************************************
 Copyright (c) 2016, Imagination Technologies Limited and/or its affiliated group companies.
 All rights reserved.

 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the
 following conditions are met:
     1. Redistributions of source code must retain the above copyright notice, this list of conditions and the
        following disclaimer.
     2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the
        following disclaimer in the documentation and/or other materials provided with the distribution.
     3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote
        products derived from this software without specific prior written permission.

 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, 
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE 
 USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
***********************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Imagination.DataAccess;
using Imagination.Model;

namespace Imagination.LWM2M
{
	internal class BusinessLogicFactory
	{
		static BusinessLogicFactory()
		{
			//TenantLookups lookups = new TenantLookups();
			//Tenant tenant = new Tenant();
			//tenant.TenantID = ServiceConfiguration.DefaultTenantID;
			//tenant.Name = "Default";
            //tenant.Enabled = true;

            //DBConfig dbConfig = new DBConfig();
            //dbConfig.ConnectionString = "mongodb://";
            //dbConfig.DBAccessPermission = TDBAccessPermission.ReadWrite;
            //dbConfig.DBCategory = TDBCategory.DeviceManagement;
            //dbConfig.DBType = TDBType.MongoDB;
            //dbConfig.IsActive = true;
            //lookups.AddDBConfig(dbConfig);

            //lookups.AddSystemSetting(new SystemSetting() { SystemSettingID = (int)TSystemSetting.DBNotificationServer, DefaultValue = servers, TypeName = "system.string" });
            //lookups.AddSystemSetting(new SystemSetting() { SystemSettingID = (int)TSystemSetting.DBNotificationPort, DefaultValue = port, TypeName = "system.int32" });

			//lookups.AddTenant(tenant);
			//DataAccessBase.Initialise(lookups);
		}

		public static Clients Clients
		{
			get { return Singleton<Clients>.Instance; }
		}

		public static Events Events
		{
			get { return Singleton<Events>.Instance; }
		}

        public static Identities Identities
        {
            get { return Singleton<Identities>.Instance; }
        }

        public static Imagination.BusinessLogic.ServiceMessages ServiceMessages
		{
			get { return Singleton<Imagination.BusinessLogic.ServiceMessages>.Instance; }
		}

	}
}
