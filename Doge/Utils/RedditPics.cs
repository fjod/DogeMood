using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doge.Utils
{
    public class RedditPics : IGetPics
    {
        public RedditPics(IGetToken _token)
        {
            token = _token;
        }
        IGetToken token { get; }
    }

    public interface IGetPics
    {
    }
}
