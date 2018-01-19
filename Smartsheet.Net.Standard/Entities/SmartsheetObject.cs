using Smartsheet.NET.Standard.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{
    public class SmartsheetObject : ISmartsheetObject
    {
        public SmartsheetHttpClient _Client { get; set; }

        public SmartsheetObject()
        {

        }

        public SmartsheetObject(SmartsheetHttpClient client)
        {
            this._Client = client;
        }
    }
}
