using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Startup_Editor
{
    class startItems
    {

        public string name { get; set; }
        public string executableLocation { get; set; }
        public string registryLocation { get; set; }
        public Boolean currentUser { get; set; }

        public Boolean promptForDelete { get; set; }


    }
}
