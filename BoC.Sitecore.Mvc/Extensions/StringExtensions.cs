using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BoC.Sitecore.Mvc.Extensions
{
    public static class StringExtensions
    {
        public static Guid ToUniqueGuid(this string input)
        {
            //this code will generate a unique Guid for a string (unique with a 2^20.96 probability of a collision) 
            //http://stackoverflow.com/questions/2190890/how-can-i-generate-guid-for-a-string-values
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                return new Guid(hash);
            }
            
        }
    }
}
