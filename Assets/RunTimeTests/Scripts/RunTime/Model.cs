#if DEBUG
using System;

namespace Tests
{
    [Serializable]
    public sealed class Model
    {
        public int Id;
        public int SaveCount;
    }
}
#endif