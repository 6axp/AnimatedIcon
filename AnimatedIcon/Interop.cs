using System;
using System.Runtime.InteropServices.ComTypes;

namespace AnimatedIcon
{
    namespace Interop
    {
        public static class Contants
        {
            public static readonly Guid IID_IDispatch = new Guid("00020400-0000-0000-C000-000000000046");
            public static readonly Guid IID_IDataObject = new Guid("0000010E-0000-0000-C000-000000000046");
        }

        public class ConnectionPoint<TSource, TSink>
        {
            TSink sink;
            IConnectionPoint point;
            int cookie;

            public ConnectionPoint(TSource source, TSink sink)
            {
                this.sink = sink;
                var container = (IConnectionPointContainer)(TSource)source;
                container.FindConnectionPoint((typeof(TSink)).GUID, out this.point);
            }
            public void Advise()
            {
                if (this.cookie != 0)
                    throw new InvalidOperationException("already advised");

                this.point.Advise(this.sink, out this.cookie);
            }
            public void Unadvise()
            {
                if (this.cookie != 0)
                {
                    this.point.Unadvise(this.cookie);
                    this.cookie = 0;
                }
            }
        }
    }
}
