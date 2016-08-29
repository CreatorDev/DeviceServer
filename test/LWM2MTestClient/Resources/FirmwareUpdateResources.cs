//******************************************************************************************************
//*
//* Name:         FirmwareUpdateResources
//* Author:       delme.thomas
//* Date Written: 7/16/2015 4:42:18 PM
//* Description:  
//*
//*        Copyright (c)  Imagination Technologies 2015
//*
//******************************************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoAP;
using CoAP.Server.Resources;

namespace Imagination.LWM2M.Resources
{
	internal class FirmwareUpdateResources : LWM2MResources
	{

		public FirmwareUpdateResources()
			: base("5", true)
        { }

        public override LWM2MResource CreateResource(string name)
        {
            FirmwareUpdateResource firmwareUpdateResource = new FirmwareUpdateResource();
            firmwareUpdateResource.Name = name;
            firmwareUpdateResource.PackageName = "xyz";
            this.Add(firmwareUpdateResource);
            return firmwareUpdateResource;
        }

        protected override void DoPost(CoapExchange exchange)
		{
			FirmwareUpdateResource firmwareUpdateResource = FirmwareUpdateResource.Deserialise(exchange.Request);
			if (firmwareUpdateResource == null)
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.BadRequest);
				exchange.Respond(response);
			}
			else
			{
				firmwareUpdateResource.Name = this.GetNextChildName(); 
				this.Add(firmwareUpdateResource);
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Created);
				response.AddOption(Option.Create(OptionType.LocationPath, string.Concat(firmwareUpdateResource.Path,firmwareUpdateResource.Name)));
				exchange.Respond(response);
				OnChildCreated(firmwareUpdateResource);
			}
		}

		protected override void DoPut(CoapExchange exchange)
		{
			FirmwareUpdateResource firmwareUpdateResource = FirmwareUpdateResource.Deserialise(exchange.Request);
			if (firmwareUpdateResource == null)
			{
				Response response = Response.CreateResponse(exchange.Request, StatusCode.BadRequest);
				exchange.Respond(response);
			}
			else
			{
				firmwareUpdateResource.Name = this.GetNextChildName();
				this.Add(firmwareUpdateResource);
				Response response = Response.CreateResponse(exchange.Request, StatusCode.Changed);
				exchange.Respond(response);
				OnChildCreated(firmwareUpdateResource);
			}
		}
	}
}
