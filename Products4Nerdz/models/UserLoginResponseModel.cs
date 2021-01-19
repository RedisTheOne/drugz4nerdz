using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Products4Nerdz.models
{
    class UserLoginResponseModel
    {
        public string msg { get; set; }
        public bool status { get; set; }
        public string token { get; set; }
    }
}
