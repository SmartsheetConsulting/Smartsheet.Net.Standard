using System.Collections.Generic;
using Smartsheet.Net.Standard.Interfaces;

namespace Smartsheet.NET.Standard.Entities
{
    public class Home : ISmartsheetObject
    {
        public Home()
        {

        }

        public IEnumerable<Sheet> Sheets { get; set; }
        public IEnumerable<Folder> Folders { get; set; }
        public IEnumerable<Report> Reports { get; set; }
        public IEnumerable<Template> Templates { get; set; }
        public IEnumerable<Workspace> Workspaces { get; set; }
        public IEnumerable<Sight> Sights { get; set; }
    }
}

