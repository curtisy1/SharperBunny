using System.Threading.Tasks;
using SharperBunny.Exceptions;

namespace SharperBunny.Declare {
    ///<summary>
    ///entry for declaration builder
    ///</summary>
    public class DeclareBase : IDeclare {
        public IBunny Bunny { get; set; }
        public Task DeclareAsync () {
            throw DeclarationException.BaseNotValid ();
        }
    }
}