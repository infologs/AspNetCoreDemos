using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JWTTokenStandard.Model
{
    public class TokenResponse
    {
        public string access_token { get; set; }
        public DateTime expire_in { get; set; }
    }
}
